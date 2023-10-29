using Domain.Models;
using Domain.Interfaces.Infra.Data;
using Domain.Interfaces.Produtos;
using Domain.Entities;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Loja
{
    public class ProdutosService : IProdutosInterface
    {
        private readonly ApplicationDbContext _context;
        private readonly IImagesInterface _imagesService;
        private readonly IPagesInterface _pagesService;
        private readonly IConfiguration _configuration;
        private readonly string _getConnection;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProdutosService(IImagesInterface imageService, ApplicationDbContext context, IPagesInterface pagesService, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _imagesService = imageService;
            _context = context;
            _pagesService = pagesService;
            _configuration = configuration;
            _getConnection = _configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
        }

        //-------------------------------------------------------------------------------------------------------------

        // O método GetProductsAsync é um método que recupera produtos de um banco de dados com base em critérios como categoria, termos de pesquisa
        // e número de página.Ele retorna uma tupla contendo um OperationResultModel e uma lista de produtos, representando o resultado da operação.
        //Aqui está uma explicação passo a passo do que o método faz:
        //Ele aceita três parâmetros: category (categoria do produto), searchString(termo de pesquisa) e page(número da página).
        //Se a searchString não estiver vazia, o método chama o método FilteredProductsBySearch para recuperar os produtos que correspondem ao termo de pesquisa
        //.Em seguida, chama o serviço _pagesService.GetProductsAsync para obter os produtos na página especificada.Se os produtos forem encontrados, retorna
        //uma tupla com um OperationResultModel indicando sucesso e a lista de produtos.
        //Se a category não estiver vazia, o método chama o método FilteredProductsByCategory para recuperar os produtos que correspondem à
        //categoria. Em seguida, chama o serviço _pagesService.GetProductsAsync para obter os produtos na página especificada. Se os produtos forem
        //encontrados, retorna uma tupla com um OperationResultModel indicando sucesso e a lista de produtos.
        //Se nenhuma categoria ou termo de pesquisa for especificado, o método recupera todos os produtos usando _context.Produtos.ToListAsync().
        //Se nenhum produto for encontrado, ele retorna uma tupla com um OperationResultModel indicando falha e uma mensagem apropriada.

        //Se ocorrer uma falha durante o processamento, o método retorna uma tupla com um OperationResultModel indicando falha e uma mensagem de erro.
        public async Task<(OperationResultModel, IEnumerable<Produtos>)> GetProductsAsync(string? category,string? searchString, int? page)
        {         
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var filter = await FilteredProductsBySearch(searchString);
                var pagedListFilteredBySearch = await _pagesService.PaginationProductsAsync(filter, page);

                if (pagedListFilteredBySearch != null && pagedListFilteredBySearch.Any())
                {
                    return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedListFilteredBySearch);
                }

            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryFilter = await FilteredProductsByCategory(category);
                var pagedListFilteredByCategory = await _pagesService.PaginationProductsAsync(categoryFilter, page);

                if (pagedListFilteredByCategory != null && pagedListFilteredByCategory.Any())
                {
                    return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedListFilteredByCategory);
                }
            }

            var produtos = await _context.Produtos.ToListAsync();

            if (produtos == null || produtos.Count == 0)
            {
                return (new OperationResultModel(false, "Não há produtos para ser exibido."), null);
            }

            var pagedList = await _pagesService.PaginationProductsAsync(produtos, page);

            if (pagedList == null)
            {
                return (new OperationResultModel(false, "Falha no processamento, contacte o administrador."), null);
            }

            return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedList);


        }

        //O método SaveAsync parece lidar com a tarefa de salvar um novo produto no banco de dados.Ele aceita um parâmetro ProductModel contendo os detalhes do produto
        //a ser salvo.Aqui está uma explicação detalhada do que o método está fazendo:
        //O método verifica se o parâmetro model não é nulo. Se for nulo, ele retorna imediatamente um OperationResultModel indicando que nenhum dado foi recebido.
        //Em seguida, o método chama o serviço _imagesService.UploadImageAsync para fazer upload da imagem do produto.Ele aguarda o resultado e verifica
        //se o upload foi bem-sucedido.Se não for bem-sucedido, o método retorna um OperationResultModel indicando falha e inclui a mensagem de erro
        //do serviço de imagens.
        //Se o upload da imagem for bem-sucedido, o método cria uma nova instância da classe Produtos e preenche as propriedades com os valores do objeto model.
        //A instância do produto é então adicionada ao contexto do banco de dados usando _context.Produtos.Add(product).
        //Finalmente, o método chama _context.SaveChangesAsync() para salvar as alterações no banco de dados.Se a operação for concluída com sucesso, o método
        //retorna um OperationResultModel indicando sucesso e uma mensagem apropriada.
        public async Task<OperationResultModel> SaveAsync(ProductModel model)
        {
            if (model == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }
           
            var (result, imagePath) = await _imagesService.UploadImageAsync(model.Image);

            if (!result.Success)
            {
                return new OperationResultModel(false, result.Message);
            }
            
            var product = new Produtos 
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                Category = model.Category,
                PathImage = imagePath
            };

            _context.Produtos.Add(product);
            await _context.SaveChangesAsync();

            return new OperationResultModel(true, "Operação bem sucedida.");
        }

        public async Task<(OperationResultModel, Produtos)> GetProductByIdAsync(int id)
        {
            if (id != null)
            {
                using (var connection = new SqlConnection(_getConnection))
                {
                    var query = "SELECT * FROM Produtos WHERE ID = @Id";
                    var parameters = new { Id = id };
                    var result = await connection.QueryFirstOrDefaultAsync<Produtos>(query, parameters);
                    if (result != null)
                    {
                        return (new OperationResultModel(true, "Operação bem sucedida."), result);
                    }
                    return (new OperationResultModel(false, "Nenhum produto encontrado com base no dados recebidos, caso esteja tudo correto recarregue a página, se o problema persistir contacte o administrador."), null);
                }
            }
            return (new OperationResultModel(false, "Nenhum dados recebido."), null); ;
        }

        public async Task<OperationResultModel> AddProductToFavorites(int productId, ClaimsPrincipal user)
        {
            if (productId == 0 || productId == null || user == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }

            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return new OperationResultModel(false, "Usuário não encontrado.");
            }

            var isFavorited = await CheckIfProductIsFavoritedAsync(productId, currUser.Id);

            if (isFavorited)
            {
                return new OperationResultModel(false, "Este produto já foi adicionado aos favoritos.");
            }

            var favoriteProduct = new FavoriteProducts(productId, currUser.Id);

            try
            {
                _context.FavoriteProductsUser.Add(favoriteProduct);
                await _context.SaveChangesAsync();
                return new OperationResultModel(true, "Produto adicionado aos favoritos com sucesso.");
            }
            catch (Exception)
            {
                return new OperationResultModel(false, "Ocorreu um erro ao adicionar o produto aos favoritos.");
            }
        }


        public async Task<(OperationResultModel, IEnumerable<ShoppingCartUser>)> GetShoppingCartAsync(ClaimsPrincipal user)
        {
            var currUser = await _userManager.GetUserAsync(user);

            if (currUser == null)
            {
                return (new OperationResultModel(false, "Usuário não encontrado."), null);
            }

            var userId = currUser.Id;

            var shoppingCartItems = await GetShoppingCartItemsAsync(userId);

            if (shoppingCartItems.Any())
            {
                return (new OperationResultModel(true, "Itens do carrinho de compras recuperados com sucesso."), shoppingCartItems);
            }
            
            return (new OperationResultModel(false, "Nenhum item encontrado no carrinho de compras."), null);
            
        }



        //---------------------------------------------------------------------------------------------------------

        //Neste método, FilteredProductsByCategory, você está utilizando parâmetros para evitar injeção de SQL.Aqui está uma explicação detalhada
        //do que o método está fazendo:
        //O método aceita um parâmetro category que representa a categoria pela qual os produtos devem ser filtrados.
        //Dentro do bloco using, você está criando uma nova instância de SqlConnection usando a string de conexão _getConnection.
        //Você está executando uma consulta SQL parametrizada que seleciona todos os produtos onde a categoria é igual ao parâmetro @Category fornecido.
        //O Dapper trata automaticamente o uso do parâmetro, o que evita possíveis ataques de injeção de SQL.
        //O método QueryAsync é usado para enviar a consulta SQL para o banco de dados e aguardar os resultados.A lista de resultados
        //é armazenada na variável result.
        //Finalmente, o método retorna a lista de produtos resultante ou null se nenhum resultado for encontrado.
        private async Task<IEnumerable<Produtos>> FilteredProductsByCategory(string? category)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var result = await connection.QueryAsync<Produtos>("SELECT * FROM Produtos WHERE Category = @Category", new { Category = category });
                return result ?? null;
            }
        }

        //Neste método, FilteredProductsBySearch, você também está utilizando parâmetros para evitar injeção de SQL.Aqui está uma explicação
        //detalhada do que o método está fazendo:
        //O método aceita um parâmetro searchString, que representa o termo de pesquisa para filtrar os produtos.
        //Dentro do bloco using, você está criando uma nova instância de SqlConnection usando a string de conexão _getConnection.
        //Você está executando uma consulta SQL parametrizada que seleciona todos os produtos onde o nome (convertido para minúsculas) é semelhante
        //ao parâmetro @SearchString fornecido.O uso de parâmetros evita possíveis ataques de injeção de SQL.
        //O método QueryAsync é usado para enviar a consulta SQL para o banco de dados e aguardar os resultados.A lista de resultados
        // é armazenada na variável result.
        //Finalmente, o método retorna a lista de produtos resultante ou null se nenhum resultado for encontrado.
        private async Task<IEnumerable<Produtos>> FilteredProductsBySearch(string? searchString)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var result = await connection.QueryAsync<Produtos>("SELECT * FROM Produtos WHERE LOWER(Name) LIKE @SearchString", new { SearchString = $"{searchString}%" });
                return result ?? null;
            }
        }

        //Este método verifica se um produto específico foi marcado como favorito por um determinado usuário.Ele executa uma consulta SQL
        //parametrizada para verificar se há pelo menos uma entrada na tabela FavoriteProductsUser que corresponda aos IDs do produto e do usuário
        //fornecidos.O método retorna verdadeiro se o produto foi favoritado pelo usuário e falso caso contrário.
        private async Task<bool> CheckIfProductIsFavoritedAsync(int productId, string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                string query = "SELECT TOP 1 1 FROM FavoriteProductsUser WHERE ProductId = @ProductId AND UserId = @UserId";
                var parameters = new { ProductId = productId, UserId = userId };
                var result = connection.QueryFirstOrDefault<int?>(query, parameters);
                return result != null && result.Value > 0;
            }
        }


        //O método GetShoppingCartItemsAsync é responsável por recuperar os itens do carrinho de compras de um usuário específico a partir de um banco de dados SQL Server
        //.Ele utiliza a biblioteca Dapper para mapear os resultados da consulta para objetos C#.
        //A consulta SQL realizada no método tem a finalidade de buscar os itens presentes no carrinho de compras do usuário com base no ID desse usuário. Ela seleciona
        //os campos ProductId e Name da tabela ShoppingCartUser e também utiliza a função de agregação COUNT para calcular a quantidade de cada item presente
        //no carrinho de compras. A tabela Produtos é referenciada na consulta por meio da cláusula JOIN, o que permite incluir informações detalhadas sobre cada produto.
        //O método, ao receber o ID do usuário como parâmetro, utiliza-o para filtrar os resultados da consulta e retorna uma lista de objetos ShoppingCartUser. Esses
        //objetos representam os itens presentes no carrinho de compras do usuário, incluindo o ID do produto, o nome do produto e a quantidade desse produto no carrinho.
        private async Task<IEnumerable<ShoppingCartUser>> GetShoppingCartItemsAsync(string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = @"SELECT sc.ProductId, p.Name, p.Price, p.PathImage, COUNT(sc.ProductId) AS Quantity 
                   FROM ShoppingCartUser sc 
                   JOIN Produtos p ON sc.ProductId = p.Id 
                   WHERE sc.UserId = @UserId 
                   GROUP BY sc.ProductId, p.Name, p.Price, p.PathImage";

                var parameters = new { UserId = userId };

                var shoppingCartItems = await connection.QueryAsync<ShoppingCartUser>(query, parameters);

                return shoppingCartItems;
            }
        }





        //----------------------------------------------------------------------------------------------------------
    }
}
