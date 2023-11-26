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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Application.Services.Loja
{
    public class ProdutosService : IProdutosInterface
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IImagesInterface _imagesService;
        private readonly IPagesInterface _pagesService;
        private readonly IConfiguration _configuration;
        private readonly string _getConnection;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProdutosService(IImagesInterface imageService, ApplicationDbContext context, IPagesInterface pagesService, IConfiguration configuration, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _imagesService = imageService;
            _context = context;
            _pagesService = pagesService;
            _configuration = configuration;
            _getConnection = _configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
            _environment = environment;
        }

        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Obtém produtos com base nos parâmetros especificados, incluindo a categoria, a sequência de pesquisa e a página.
        /// </summary>
        /// <param name="user">O usuário atual.</param>
        /// <param name="category">A categoria pela qual os produtos serão filtrados.</param>
        /// <param name="searchString">A sequência de pesquisa para filtrar os produtos.</param>
        /// <param name="page">O número da página a ser exibida.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação, uma coleção de objetos Produtos correspondente aos produtos recuperados e uma coleção de objetos FavoriteProducts correspondente aos produtos favoritos do usuário atual.</returns>
        public async Task<(OperationResultModel, IEnumerable<Produtos>, IEnumerable<FavoriteProducts>)> GetProductsAsync(ClaimsPrincipal user, string? category, string? searchString, int? page)
        {
            try
            {
                var currUser = await _userManager.GetUserAsync(user);
                string userId = null;

                if (currUser != null)
                {
                    userId = currUser.Id;
                }

                var userFavorites = await GetFavoritesByUserIdAsync(userId);

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var filter = await FilteredProductsBySearch(searchString);
                    var pagedListFilteredBySearch = await _pagesService.PaginationProductsAsync(filter, page);

                    if (pagedListFilteredBySearch != null && pagedListFilteredBySearch.Any())
                    {
                        return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedListFilteredBySearch, userFavorites);
                    }

                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    var categoryFilter = await FilteredProductsByCategory(category);
                    var pagedListFilteredByCategory = await _pagesService.PaginationProductsAsync(categoryFilter, page);

                    if (pagedListFilteredByCategory != null && pagedListFilteredByCategory.Any())
                    {
                        return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedListFilteredByCategory, userFavorites);
                    }
                }

                var produtos = await _context.Produtos.ToListAsync();

                if (produtos == null || produtos.Count == 0)
                {
                    return (new OperationResultModel(false, "Não há produtos para ser exibido."), null, null);
                }

                var pagedList = await _pagesService.PaginationProductsAsync(produtos, page);

                if (pagedList == null)
                {
                    return (new OperationResultModel(false, "Falha no processamento, contacte o administrador."), null, null);
                }

                return (new OperationResultModel(true, "Dados obtidos com sucesso."), pagedList, userFavorites);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null, null);
            }
        }

        /// <summary>
        /// Salva um novo produto no banco de dados com base nos dados fornecidos no objeto ProductModel.
        /// </summary>
        /// <param name="model">O objeto ProductModel contendo os dados do produto a ser salvo.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação de salvamento.</returns>
        public async Task<OperationResultModel> SaveAsync(ProductModel model)
        {
            try
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
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Erro durante a operação de salvamento: {ex.Message}");
            }
        }

        /// <summary>
        /// Edita um produto existente com base no modelo fornecido.
        /// </summary>
        /// <param name="model">O modelo de produto a ser editado.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação.</returns>
        public async Task<OperationResultModel> EditAsync(EditProductModel model)
        {
            try
            {
                if (model == null)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }            

                var product = await _context.Produtos.FindAsync(model.ID);

                if (product == null)
                {
                    return new OperationResultModel(false, "Produto não encontrado.");
                }

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.Category = model.Category;
               
                await _context.SaveChangesAsync();

                return new OperationResultModel(true, "Operação bem sucedida.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Erro durante a edição do produto: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza a imagem de um produto específico com a nova imagem fornecida.
        /// </summary>
        /// <param name="productId">O ID do produto a ser atualizado.</param>
        /// <param name="image">A nova imagem a ser associada ao produto.</param>
        /// <returns>Um objeto OperationResultModel indicando o sucesso ou a falha da operação, juntamente com uma mensagem correspondente.</returns>
        public async Task<OperationResultModel> UpdateImageAsync(int productId, IFormFile image)
        {
            try
            {
                if (productId == 0)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var product = await _context.Produtos.FindAsync(productId);

                if (product == null)
                {
                    return new OperationResultModel(false, "Produto não encontrado.");
                }

                var (result, imagePath) = await _imagesService.UploadImageAsync(image);

                if (!result.Success)
                {
                    return new OperationResultModel(false, result.Message);
                }

                // Chamar o método para excluir a imagem antiga
                DeleteImageFromRoot(product.PathImage);

                // Atualizar o caminho da nova imagem
                product.PathImage = imagePath;

                await _context.SaveChangesAsync();

                return new OperationResultModel(true, "Operação bem sucedida.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Erro durante a edição do produto: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para excluir um produto pelo ID.
        /// </summary>
        /// <param name="productId">O ID do produto a ser excluído.</param>
        /// <returns>Um objeto OperationResultModel indicando se a operação foi bem-sucedida ou não.</returns>
        public async Task<OperationResultModel> DeleteAsync(int productId)
        {
            try
            {
                if (productId == 0)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var product = await _context.Produtos.FindAsync(productId);

                if (product == null)
                {
                    return new OperationResultModel(false, "Produto não encontrado.");
                }              

                // Chamar o método para excluir a imagem do root
                DeleteImageFromRoot(product.PathImage);
                // Remove do database
                _context.Produtos.Remove(product);

                await _context.SaveChangesAsync();

                return new OperationResultModel(true, "Operação bem sucedida.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Erro durante a exclusão do produto: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém um produto por ID juntamente com os favoritos do usuário atual, se houver.
        /// </summary>
        /// <param name="id">O ID do produto a ser recuperado.</param>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação, um objeto Produtos correspondente ao produto recuperado e uma coleção de objetos FavoriteProducts correspondente aos produtos favoritos do usuário atual.</returns>
        public async Task<(OperationResultModel, Produtos, IEnumerable<FavoriteProducts>)> GetProductByIdAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                if (id == 0)
                {
                    return (new OperationResultModel(true, "Nenhum dado recebido."), null, null);
                }

                var currUser = await _userManager.GetUserAsync(user);
                string userId = null;

                if (currUser != null)
                {
                    userId = currUser.Id;
                }

                var userFavorites = await GetFavoritesByUserIdAsync(userId);
                var result = await RetrieveProductFromDatabaseAsync(id);

                if (result != null)
                {
                    return (new OperationResultModel(true, "Operação bem sucedida."), result, userFavorites);
                }

                return (new OperationResultModel(false, "Nenhum produto encontrado com base nos dados recebidos. Se estiver tudo correto, recarregue a página. Se o problema persistir, entre em contato com o administrador."), null, null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null, null);
            }
        }

        /// <summary>
        /// Adiciona um produto aos favoritos do usuário atual com base no ID do produto.
        /// </summary>
        /// <param name="productId">O ID do produto a ser adicionado aos favoritos.</param>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação de adição.</returns>
        public async Task<OperationResultModel> AddProductToFavorites(int productId, ClaimsPrincipal user)
        {
            try
            {
                if (productId == 0 || user == null)
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

                _context.FavoriteProductsUser.Add(favoriteProduct);
                await _context.SaveChangesAsync();
                return new OperationResultModel(true, "Produto adicionado aos favoritos com sucesso.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um produto dos favoritos do usuário atual com base no ID do produto.
        /// </summary>
        /// <param name="productId">O ID do produto a ser removido dos favoritos.</param>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação de remoção.</returns>
        public async Task<OperationResultModel> RemoveProductFromFavorites(int productId, ClaimsPrincipal user)
        {
            try
            {
                if (productId == 0 || user == null)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return new OperationResultModel(false, "Usuário não encontrado.");
                }

                var isFavorited = await CheckIfProductIsFavoritedAsync(productId, currUser.Id);

                if (!isFavorited)
                {
                    return new OperationResultModel(false, "Este produto não foi adicionado aos favoritos.");
                }

                var favoriteProductToRemove = await _context.FavoriteProductsUser.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == currUser.Id);

                if (favoriteProductToRemove != null)
                {
                    _context.FavoriteProductsUser.Remove(favoriteProductToRemove);
                    await _context.SaveChangesAsync();
                    return new OperationResultModel(true, "Produto removido dos favoritos com sucesso.");
                }

                return new OperationResultModel(false, "O produto não foi encontrado nos favoritos.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona um produto ao carrinho de compras para o usuário atual com base no ID do produto.
        /// </summary>
        /// <param name="productId">O ID do produto a ser adicionado ao carrinho de compras.</param>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação de adição.</returns>
        public async Task<OperationResultModel> AddProductToShoppingCart(int productId, ClaimsPrincipal user)
        {
            try
            {
                if (productId == 0 || user == null)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return new OperationResultModel(false, "Usuário não encontrado.");
                }

                var canAddProduct = await CheckIfProductCanBeAddedToShoppingCartAsync(productId, currUser.Id);

                if (!canAddProduct)
                {
                    return new OperationResultModel(false, "Você atingiu o limite do estoque deste produto.");
                }

                var shoppingCart = new ShoppingCart(productId, currUser.Id);

                _context.ShoppingCartUser.Add(shoppingCart);
                await _context.SaveChangesAsync();
                return new OperationResultModel(true, "Produto adicionado ao carrinho de compras com sucesso.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um produto do carrinho de compras para o usuário atual com base no ID do produto.
        /// </summary>
        /// <param name="productId">O ID do produto a ser removido do carrinho de compras.</param>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Um objeto OperationResultModel indicando o resultado da operação de remoção.</returns>
        public async Task<OperationResultModel> RemoveProductFromShoppingCart(int productId, ClaimsPrincipal user)
        {
            try
            {
                if (productId == 0 || user == null)
                {
                    return new OperationResultModel(false, "Nenhum dado recebido.");
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return new OperationResultModel(false, "Usuário não encontrado.");
                }

                var itemToRemove = await _context.ShoppingCartUser.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == currUser.Id);

                if (itemToRemove != null)
                {
                    _context.ShoppingCartUser.Remove(itemToRemove);
                    await _context.SaveChangesAsync();
                    return new OperationResultModel(true, "Produto removido do carrinho de compras com sucesso.");
                }

                return new OperationResultModel(false, "O produto não foi encontrado no carrinho de compras.");
            }
            catch (Exception ex)
            {
                return new OperationResultModel(false, $"Exceção não planejada: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém o carrinho de compras para o usuário atual, incluindo os itens do carrinho de compras e os produtos favoritos.
        /// </summary>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Uma tupla contendo o resultado da operação, os itens do carrinho de compras e os produtos favoritos do usuário.</returns>
        public async Task<(OperationResultModel, IEnumerable<ShoppingCartUser>, IEnumerable<FavoriteProducts>)> GetShoppingCartAsync(ClaimsPrincipal user)
        {
            try
            {
                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), null, null);
                }

                var userId = currUser.Id;

                var userFavorites = await GetFavoritesByUserIdAsync(userId);
                var shoppingCartItems = await GetShoppingCartItemsAsync(userId);

                if (shoppingCartItems != null && shoppingCartItems.Any())
                {
                    return (new OperationResultModel(true, "Itens do carrinho de compras recuperados com sucesso."), shoppingCartItems, userFavorites);
                }

                return (new OperationResultModel(false, "Nenhum item encontrado no carrinho de compras."), null, null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null, null);
            }
        }

        /// <summary>
        /// Obtém o cartão de favoritos para um usuário específico.
        /// </summary>
        /// <param name="user">O usuário atual.</param>
        /// <returns>Uma tupla contendo o resultado da operação e a lista de produtos favoritos do usuário.</returns>
        public async Task<(OperationResultModel, IEnumerable<Produtos>)> GetFavoriteCardAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null);
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), null);
                }

            
                var userId = currUser.Id;
                var favoritesProductsUser = await GetFavoriteProductsAsync(userId);

                if (favoritesProductsUser.Any())
                {
                    return (new OperationResultModel(true, "Itens recuperados com sucesso."), favoritesProductsUser);
                }
                return (new OperationResultModel(false, "Não há produtos salvos."), null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Erro ao recuperar itens favoritos: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Obtém uma lista de produtos de acordo com o usuário fornecido, somente admin's.
        /// </summary>
        /// <param name="user">O usuário para o qual os produtos devem ser obtidos.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação e uma lista de produtos.</returns>
        public async Task<(OperationResultModel, IEnumerable<Produtos>)> GetAllProductsForAdminAsync(ClaimsPrincipal user)
        {
            try 
            { 
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null);
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), null);
                }

                var isAdmin = await _userManager.IsInRoleAsync(currUser, "Admin");

                if (!isAdmin)
                {
                    return (new OperationResultModel(false, "Este recurso pode ser acessado apenas por administradores."), null);
                }

                var produtos = await _context.Produtos.ToListAsync();

                if (produtos == null || produtos.Count == 0)
                {
                    return (new OperationResultModel(false, "Não há produtos para ser exibido."), null);
                }

                return (new OperationResultModel(true, "Dados obtidos com sucesso."), produtos);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Recupera um produto específico para fins de edição e exclusão com base no ID fornecido.
        /// </summary>
        /// <param name="id">O ID do produto a ser recuperado.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação e o modelo de produto recuperado.</returns>
        public async Task<(OperationResultModel, EditProductModel model, string pathImage)> GetProductForEditAndDeleteAsync(int id)
        {
            try 
            { 
                if (id == 0)
                {
                    return (new OperationResultModel(false, "Nenhum dado recebido."), null, null);
                }

                var product = await RetrieveProductFromDatabaseAsync(id);

                if (product != null)
                {
                    var model = new EditProductModel
                    {
                        ID = product.ID,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Stock = product.Stock,
                        Category = product.Category,
                    };
                    return (new OperationResultModel(true, "Dados recuperados com sucesso."), model, product.PathImage);
                }

                return (new OperationResultModel(false, "Erro ao recuperar os dados."), null, null);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null, null);
            }
        }

        public async Task<(OperationResultModel, Dictionary<Produtos, Order>)> GetProductsPaid(ClaimsPrincipal user)
        {
            try
            {
                if (user == null)
                {
                    return (new OperationResultModel(false, "Nenhum usuário recebido."), new Dictionary<Produtos, Order>());
                }

                var currUser = await _userManager.GetUserAsync(user);

                if (currUser == null)
                {
                    return (new OperationResultModel(false, "Usuário não encontrado."), new Dictionary<Produtos, Order>());
                }

                var productsPaid = await RetrieveProductsPaidAsync(currUser.Id);
                var order = await GetOrberByUserIdAsync(currUser.Id);

                return (new OperationResultModel(true, "Produtos pagos recuperados com sucesso."), productsPaid);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), new Dictionary<Produtos, Order>());
            }
        }


        //---------------------------------------------------------------------------------------------------------

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
        /// Filtra os produtos por categoria.
        /// </summary>
        /// <param name="category">A categoria pela qual os produtos serão filtrados.</param>
        /// <returns>Uma coleção de objetos Produtos correspondente aos produtos filtrados pela categoria especificada ou null se nenhum produto for encontrado.</returns>
        private async Task<IEnumerable<Produtos>> FilteredProductsByCategory(string category)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var result = await connection.QueryAsync<Produtos>("SELECT * FROM Produtos WHERE Category = @Category", new { Category = category });
                return result ?? null;
            }
        }

        /// <summary>
        /// Filtra os produtos com base em uma sequência de pesquisa.
        /// </summary>
        /// <param name="searchString">A sequência de pesquisa usada para filtrar os produtos.</param>
        /// <returns>Uma coleção de objetos Produtos correspondente aos produtos filtrados ou null se nenhum produto for encontrado.</returns>
        private async Task<IEnumerable<Produtos>> FilteredProductsBySearch(string searchString)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var result = await connection.QueryAsync<Produtos>("SELECT * FROM Produtos WHERE LOWER(Name) LIKE @SearchString", new { SearchString = $"{searchString}%" });
                return result ?? null;
            }
        }

        /// <summary>
        /// Verifica se um produto foi marcado como favorito com base no ID do produto e no ID do usuário.
        /// </summary>
        /// <param name="productId">O ID do produto a ser verificado.</param>
        /// <param name="userId">O ID do usuário para o qual o produto será verificado.</param>
        /// <returns>Verdadeiro se o produto estiver marcado como favorito, caso contrário, falso.</returns>
        private async Task<bool> CheckIfProductIsFavoritedAsync(int productId, string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                string query = "SELECT TOP 1 1 FROM FavoriteProductsUser WHERE ProductId = @ProductId AND UserId = @UserId";
                var parameters = new { ProductId = productId, UserId = userId };
                var result = await connection.QueryFirstOrDefaultAsync<int?>(query, parameters);
                return result != null && result.Value > 0;
            }
        }

        /// <summary>
        /// Obtém os itens do carrinho de compras com base no ID do usuário.
        /// </summary>
        /// <param name="userId">O ID do usuário para o qual os itens do carrinho de compras serão recuperados.</param>
        /// <returns>Uma coleção de objetos ShoppingCartUser correspondente aos itens do carrinho de compras do usuário ou null se nenhum item for encontrado.</returns>
        private async Task<IEnumerable<ShoppingCartUser>> GetShoppingCartItemsAsync(string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = @"SELECT sc.ProductId, p.Name, p.Price, p.PathImage, p.Stock, COUNT(sc.ProductId) AS Quantity 
                   FROM ShoppingCartUser sc 
                   JOIN Produtos p ON sc.ProductId = p.Id 
                   WHERE sc.UserId = @UserId 
                   GROUP BY sc.ProductId, p.Name, p.Price, p.PathImage, p.Stock";

                var parameters = new { UserId = userId };

                var shoppingCartItems = await connection.QueryAsync<ShoppingCartUser>(query, parameters);

                return shoppingCartItems ?? null;
            }
        }

        /// <summary>
        /// Obtém os produtos favoritos com base no ID do usuário.
        /// </summary>
        /// <param name="userId">O ID do usuário para o qual os produtos favoritos serão recuperados.</param>
        /// <returns>Uma coleção de objetos FavoriteProducts correspondente aos produtos favoritos do usuário ou uma lista vazia se nenhum favorito for encontrado.</returns>
        private async Task<IEnumerable<FavoriteProducts>> GetFavoritesByUserIdAsync(string? userId)
        {
            if (userId == null)
            {
                return Enumerable.Empty<FavoriteProducts>();
            }
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = "SELECT * FROM FavoriteProductsUser WHERE UserId = @UserId";
                var userFavorites = await connection.QueryAsync<FavoriteProducts>(query, new { UserId = userId });

                return userFavorites ?? Enumerable.Empty<FavoriteProducts>();
            }
        }

        /// <summary>
        /// Verifica se um produto pode ser adicionado ao carrinho de compras com base no ID do produto e no ID do usuário.
        /// </summary>
        /// <param name="productId">O ID do produto a ser verificado.</param>
        /// <param name="userId">O ID do usuário para o qual o produto será verificado.</param>
        /// <returns>Verdadeiro se o produto puder ser adicionado ao carrinho de compras, caso contrário, falso.</returns>
        private async Task<bool> CheckIfProductCanBeAddedToShoppingCartAsync(int productId, string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                string productQuery = "SELECT Stock FROM Produtos WHERE ID = @ProductId";
                var productParameters = new { ProductId = productId };
                var stock = await connection.QueryFirstOrDefaultAsync<int>(productQuery, productParameters);

                string cartQuery = "SELECT COUNT(*) FROM ShoppingCartUser WHERE ProductId = @ProductId AND UserId = @UserId";
                var cartParameters = new { ProductId = productId, UserId = userId };
                var cartItemCount = await connection.QueryFirstOrDefaultAsync<int>(cartQuery, cartParameters);

                if (cartItemCount == null)
                {
                    cartItemCount = 0;
                }

                if (cartItemCount < stock)
                {
                    return true; 
                }
                return false; 
            }
        }

        /// <summary>
        /// Este método recupera os produtos favoritos para um determinado ID de usuário.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Se nenhum produto favorito for encontrado, uma coleção vazia será retornada.</returns>
        private async Task<IEnumerable<Produtos>> GetFavoriteProductsAsync(string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var favoriteProductIds = (await connection.QueryAsync<int>("SELECT ProductId FROM FavoriteProductsUser WHERE userId = @UserId", new { UserId = userId })).ToList();

                if (favoriteProductIds.Any())
                {
                    var favoriteProducts = await connection.QueryAsync<Produtos>("SELECT * FROM Produtos WHERE ID IN @Ids", new { Ids = favoriteProductIds });
                    return favoriteProducts;
                }

                return Enumerable.Empty<Produtos>();
            }
        }

        /// <summary>
        /// Deleta uma imagem do diretório raiz.
        /// </summary>
        /// <param name="imagePath">Caminho da imagem a ser excluída.</param>
        private void DeleteImageFromRoot(string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, "images", imagePath);
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }
        }

        private async Task<Dictionary<Produtos, Order>> RetrieveProductsPaidAsync(string userId)
        {
            using (var connection = new SqlConnection(_getConnection))
            {
                var orders = (await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE UserId = @UserId AND PaymentConfirmed = @confirmed",
                new { UserId = userId, confirmed = 1 })).ToList();


                var productQuantities = new Dictionary<Produtos, Order>();

                foreach (var order in orders)
                {
                    var product = await RetrieveProductFromDatabaseAsync(order.ProductId);

                    if (product != null)
                    {                           
                      productQuantities.Add(product, order);                     
                    }
                }

                return productQuantities;
            }
        }



        private async Task<IEnumerable<Order>> GetOrberByUserIdAsync(string userId)
        {
            if (userId == null)
            {
                return Enumerable.Empty<Order>();
            }
            using (var connection = new SqlConnection(_getConnection))
            {
                var query = "SELECT * FROM Orders WHERE UserId = @UserId";
                var orders = await connection.QueryAsync<Order>(query, new { UserId = userId });

                return orders ?? Enumerable.Empty<Order>();
            }
        }





        //----------------------------------------------------------------------------------------------------------


    }
}
