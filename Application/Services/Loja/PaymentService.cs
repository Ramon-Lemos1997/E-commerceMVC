using Dapper;
using Domain.Entities;
using Domain.Interfaces.Payment;
using Domain.Models;
using Infra.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.Security.Claims;
using System.Transactions;

namespace Application.Services.Loja
{ 
    public class PaymentService : IPaymentInterface
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _getConnection;
        private readonly ApplicationDbContext _context;
        private readonly IStripeInterface _stripeService;

        public PaymentService(IConfiguration config, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IStripeInterface stripeService)
        {
            _config = config;
            _userManager = userManager;
            _getConnection = _config.GetConnectionString("DefaultConnection");
            _context = context;
            _stripeService = stripeService;
        }

        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// Cria um pedido para pagamento, verificando a disponibilidade dos produtos, calculando o valor total, e obtendo a URL de checkout do Stripe.
        /// </summary>
        /// <param name="user">O objeto ClaimsPrincipal que representa o usuário atual.</param>
        /// <param name="productsId">A lista de IDs dos produtos do pedido.</param>
        /// <param name="productsQuantity">A lista de quantidades correspondentes aos produtos do pedido.</param>
        /// <returns>
        /// Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação e uma string contendo a URL de checkout do Stripe, se o pedido for criado com sucesso.
        /// </returns>
        public async Task<(OperationResultModel, string url)> CreateOrderForPayAsync(ClaimsPrincipal user, IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            try
            {
                var isAvailability = await VerifyProductAvailability(productsId, productsQuantity);

                if (!isAvailability.Success)
                {
                    return (new OperationResultModel(false, isAvailability.Message), null);
                }

                var (userResult, currentUser) = await GetCurrentUser(user);

                if (!userResult.Success)
                {
                    return (new OperationResultModel(false, userResult.Message), null);
                }

                decimal totalAmount = await CalculateTotalAmount(productsId, productsQuantity);

                if (totalAmount == 0)
                {
                    return (new OperationResultModel(false, "Erro ao somar o preço total a ser cobrado."), null);
                }

                var orderIds = await SaveOrders(currentUser.Id, productsId, productsQuantity);

                if (orderIds == null)
                {
                    return (new OperationResultModel(false, "Erro no processamento das ordens."), null);
                }

                var (result, url) = await _stripeService.GetStripeCheckoutUrl(orderIds, totalAmount);

                if (!result.Success)
                {
                    return (new OperationResultModel(false, result.Message), null);
                }

                return (new OperationResultModel(true, "Pedido criado com sucesso"), url);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Atualiza um pedido no banco de dados, confirmando o pagamento e processando a transação.
        /// </summary>
        /// <param name="orderId">ID do pedido a ser atualizado.</param>
        /// <returns>
        /// Um objeto OperationResultModel indicando o resultado da atualização do pedido.
        /// Se a atualização for bem-sucedida, retorna um OperationResultModel com sucesso.
        /// Em caso de erro (pedido não encontrado, erro de concorrência ou outro erro inesperado), retorna um OperationResultModel indicando o motivo do erro.
        /// </returns>
        public async Task<OperationResultModel> UpdateOrderAsync(int orderId)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var order = await RetrieveOrderFromDatabaseAsync(orderId);
                    if (order == null)
                    {
                        return HandleError("Pedido não encontrado");
                    }

                    order.PaymentConfirmed = true;
                    _context.Entry(order).State = EntityState.Modified;

                    var productResult = await ProcessProduct(order.ProductId, order.Quantity);
                    if (!productResult.Success)
                    {
                        return productResult;
                    }

                    await ResetShoppingCart(order.UserId);

                    await _context.SaveChangesAsync();
                    scope.Complete();

                    return new OperationResultModel(true, "Pedido processado com sucesso");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return HandleError($"Erro de concorrência ao processar pedido: {ex.Message}");
            }
            catch (Exception ex)
            {
                return HandleError($"Erro ao processar pedido: {ex.Message}");
            }
        }


        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calcula o valor total com base nos IDs dos produtos e suas quantidades correspondentes.
        /// </summary>
        /// <param name="productsId">A lista de IDs dos produtos.</param>
        /// <param name="productsQuantity">A lista de quantidades correspondentes aos produtos.</param>
        /// <returns>O valor total calculado com base nos preços dos produtos e suas quantidades.</returns>
        private async Task<decimal> CalculateTotalAmount(IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            decimal totalAmount = 0;
            for (int i = 0; i < productsId.Count(); i++)
            {
                int productId = productsId.ElementAt(i);
                int quantity = productsQuantity.ElementAt(i);

                var product = await RetrieveProductFromDatabaseAsync(productId);

                if (product == null)
                {
                    throw new Exception($"Produto não encontrado para o ID: {productId}");
                }

                decimal productTotal = product.Price * quantity;

                totalAmount += productTotal;
            }

            return totalAmount != null ? totalAmount : 0;
        }

        /// <summary>
        /// Recupera um produto do banco de dados com base no ID fornecido.
        /// </summary>
        /// <param name="id">O ID do produto a ser recuperado.</param>
        /// <returns>O objeto Produtos recuperado do banco de dados ou null se nenhum produto for encontrado.</returns>
        private async Task<Produtos> RetrieveProductFromDatabaseAsync(int id)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = "SELECT * FROM Produtos WHERE ID = @Id";
                var parameters = new { Id = id };
                var result = await connection.QueryFirstOrDefaultAsync<Produtos>(query, parameters);
                return result;
            }
        }

        /// <summary>
        /// Salva as ordens no banco de dados e retorna uma lista de IDs das ordens criadas com sucesso.
        /// </summary>
        /// <param name="userId">O ID do usuário para o qual as ordens estão sendo salvas.</param>
        /// <param name="productsId">A lista de IDs dos produtos para os quais as ordens estão sendo feitas.</param>
        /// <param name="productsQuantity">A lista de quantidades correspondentes aos produtos para os quais as ordens estão sendo feitas.</param>
        /// <returns>Uma lista de IDs das ordens que foram criadas com sucesso no banco de dados.</returns>
        private async Task<List<int>> SaveOrders(string userId, IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            List<int> createdOrderIds = new List<int>();

            if (productsId.Count() == productsQuantity.Count())
            {
                List<Order> checkoutList = new List<Order>();

                for (int i = 0; i < productsId.Count(); i++)
                {
                    var productInfo = new Order
                    {
                        PaymentConfirmed = false,
                        UserId = userId,
                        ProductId = productsId.ElementAt(i),
                        Quantity = productsQuantity.ElementAt(i)
                    };

                    checkoutList.Add(productInfo);
                }

                try
                {
                    _context.Orders.AddRange(checkoutList);
                    await _context.SaveChangesAsync();

                    // Recuperar os IDs para enviar para a API de pagamento
                    createdOrderIds = checkoutList.Select(i => i.OrderId).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar os pedidos: {ex.Message}");
                }
            }

            return createdOrderIds.Count > 0 ? createdOrderIds : null;
        }

        /// <summary>
        /// Obtém o usuário atual com base no objeto ClaimsPrincipal.
        /// </summary>
        /// <param name="user">O objeto ClaimsPrincipal que representa o usuário.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação e um objeto ApplicationUser representando o usuário atual, se encontrado com sucesso.</returns>
        private async Task<(OperationResultModel, ApplicationUser user)> GetCurrentUser(ClaimsPrincipal user)
        {
            try
            {
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido"), null);
                }

                var userId = _userManager.GetUserId(user);

                if (userId == null)
                {
                    return (new OperationResultModel(false, "Dados inválidos"), null);
                }

                var currentUser = await _userManager.FindByIdAsync(userId);

                if (currentUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado"), null);
                }

                return (new OperationResultModel(true, "Usuário encontrado com sucesso"), currentUser);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Verifica a disponibilidade dos produtos com base em seus IDs e quantidades correspondentes.
        /// </summary>
        /// <param name="productsId">A lista de IDs dos produtos.</param>
        /// <param name="productsQuantity">A lista de quantidades correspondentes aos produtos.</param>
        /// <returns>
        /// Um objeto OperationResultModel indicando se os produtos estão disponíveis em quantidades suficientes.
        /// Se um produto não estiver disponível, o método retornará uma mensagem de erro especificando o motivo.
        /// </returns>
        private async Task<OperationResultModel> VerifyProductAvailability(IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            for (int i = 0; i < productsId.Count(); i++)
            {
                int productId = productsId.ElementAt(i);
                int quantity = productsQuantity.ElementAt(i);

                var product = await RetrieveProductFromDatabaseAsync(productId);

                if (product == null)
                {
                    return new OperationResultModel(false, $"Produto não encontrado para o ID: {productId}");
                }

                if (product.Stock < quantity)
                {
                    return new OperationResultModel(false, $"Verifique o estoque do produto {product.Name}, pois o mesmo tem estoque inferior a quantidade que você deseja comprar, atualize seu carrinho e tente novamente.");
                }
            }

            return new OperationResultModel(true, "Todos os produtos estão disponíveis em quantidades suficientes.");
        }

        /// <summary>
        /// Recupera os detalhes de uma ordem do banco de dados com base no ID da ordem.
        /// </summary>
        /// <param name="orderId">O ID da ordem a ser recuperada.</param>
        /// <returns>Os detalhes da ordem se encontrados, caso contrário, null.</returns>
        private async Task<Order> RetrieveOrderFromDatabaseAsync(int orderId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = "SELECT * FROM Orders WHERE OrderId = @Id";
                var parameters = new { Id = orderId };
                var result = await connection.QueryFirstOrDefaultAsync<Order>(query, parameters);
                return result;
            }
        }

        /// <summary>
        /// Recupera um usuário do banco de dados com base no ID do usuário.
        /// </summary>
        /// <param name="userId">ID do usuário a ser recuperado.</param>
        /// <returns>
        /// Um objeto ApplicationUser correspondente ao usuário com o ID fornecido.
        /// Se nenhum usuário for encontrado, retorna null.
        /// </returns>
        //private async Task<ApplicationUser> RetrieveUserFromDatabaseAsync(string userId)
        //{
        //    using (var connection = new SqlConnection(_getConnection))
        //    {
        //        var query = "SELECT * FROM AspNetUsers WHERE Id = @Id";
        //        var parameters = new { Id = userId };
        //        var result = await connection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
        //        return result;
        //    }
        //}

        /// <summary>
        /// Cria um objeto OperationResultModel para lidar com erros, usando a mensagem de erro fornecida.
        /// </summary>
        /// <param name="errorMessage">A mensagem de erro a ser incluída no objeto OperationResultModel.</param>
        /// <returns>O objeto OperationResultModel com indicador de falha e a mensagem de erro especificada.</returns>
        private OperationResultModel HandleError(string errorMessage)
        {
            return new OperationResultModel(false, errorMessage);
        }

        /// <summary>
        /// Remove todos os itens do carrinho de compras de um usuário no banco de dados.
        /// </summary>
        /// <param name="userId">ID do usuário para o qual os itens do carrinho serão removidos.</param>
        private async Task ResetShoppingCart(string userId)
        {
            var resetShoppingCartItems = await _context.ShoppingCartUser.Where(i => i.UserId == userId).ToListAsync();
            _context.ShoppingCartUser.RemoveRange(resetShoppingCartItems);
        }

        /// <summary>
        /// Processa um produto verificando sua disponibilidade no estoque e atualiza o estoque após a compra.
        /// </summary>
        /// <param name="productId">ID do produto a ser processado.</param>
        /// <param name="quantity">Quantidade do produto a ser processada.</param>
        /// <returns>
        /// Um objeto OperationResultModel indicando o resultado do processamento do produto.
        /// Se o processamento for bem-sucedido, retorna um OperationResultModel com sucesso.
        /// Em caso de erro (produto não encontrado ou estoque insuficiente), retorna um OperationResultModel indicando o motivo do erro.
        /// </returns>
        private async Task<OperationResultModel> ProcessProduct(int productId, int quantity)
        {
            var product = await RetrieveProductFromDatabaseAsync(productId);

            if (product == null)
            {
                return HandleError("Produto associado ao pedido não encontrado");
            }

            if (product.Stock < quantity)
            {
                return HandleError("Estoque insuficiente para processar o pedido");
            }

            product.Stock -= quantity;
            _context.Entry(product).State = EntityState.Modified;

            return new OperationResultModel(true, string.Empty);
        }



        //_____________________________________________________________________________________________
    }
}



