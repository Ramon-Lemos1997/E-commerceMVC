

using Dapper;
using Domain.Entities;
using Domain.Interfaces.Payment;
using Domain.Models;
using Infra.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

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

        public async Task<(OperationResultModel, string url)> CreateOrderForPayAsync(ClaimsPrincipal user, IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            try
            {
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



        //_____________________________________________________________________________________________
    }
}
