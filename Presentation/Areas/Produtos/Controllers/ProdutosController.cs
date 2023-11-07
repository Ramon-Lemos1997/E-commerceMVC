using Domain.Models;
using Domain.Interfaces.Produtos;
using Infra.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services.Account;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using JetBrains.Annotations;

namespace Presentation.Areas.Produtos.Controllers
{
    [Area("Produtos")]
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProdutosInterface _produtosService;
        public ProdutosController(ApplicationDbContext context, IProdutosInterface produtosService)
        {
            _context = context;
            _produtosService = produtosService;
        }

        //------------------------------------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? searchString, int? page)
        {
            var (result, produtos, favorites) = await _produtosService.GetProductsAsync(User, category, searchString, page);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            ViewBag.Favorites = favorites;
            return View(produtos);           
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateProduct() => View();

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            
            var (result, produto, favorites) = await _produtosService.GetProductByIdAsync(id, User);
            if (result.Success)
            {
                ViewBag.Favorites = favorites;
                return View(produto); 
            }
           
            TempData["MessageError"] = result.Message;
            return View(produto);   
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {

            var (result, produto, pathImage) = await _produtosService.GetProductForEditAndDeleteAsync(id);
            if (result.Success)
            {
                ViewBag.Image = pathImage;
                return View(produto);              
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int id)
        {

            var (result, produto, pathImage) = await _produtosService.GetProductForEditAndDeleteAsync(id);
            if (result.Success)
            {
                ViewBag.Image = pathImage;
                return View(produto);
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShoppingCart()
        {

            var (result, cart, favorites) = await _produtosService.GetShoppingCartAsync(User);
            if (result.Success)
            {
                ViewBag.Favorites = favorites;
                return View(cart); 
            }

            TempData["MessageError"] = result.Message;
            return View(cart);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> FavoriteCard()
        {
            var (result, favorites) = await _produtosService.GetFavoriteCardAsync(User);
            if (result.Success)
            {
                return View(favorites);
            }

            TempData["MessageError"] = result.Message;
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AreaAdmin()
        {
            var (result, products) = await _produtosService.GetAllProductsForAdminAsync(User);
            if (result.Success)
            {
                return View(products);
            }

            TempData["MessageError"] = result.Message;
            return View();
        }


        //----------------------------------------------------------------------------------------------------


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _produtosService.SaveAsync(model);

                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Produto cadastrado com sucesso.";
                    return RedirectToAction(nameof(CreateProduct));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View();           
            }

           return View(model);            
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProduct(EditProductModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _produtosService.EditAsync(model);

                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Produto editado com sucesso.";
                    return RedirectToAction(nameof(EditProduct), new { id = model.ID }); 
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateImage(int productId, IFormFile image)
        {           
            var result = await _produtosService.UpdateImageAsync(productId, image);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Foto do produto atualizado com sucesso.";
                return RedirectToAction(nameof(EditProduct), new { id = productId });
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(EditProduct), new { id = productId });
        }

        [HttpPost, ActionName("DeleteProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProductConfirmed(int productId)
        {
            var result = await _produtosService.DeleteAsync(productId);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Produto excluído com sucesso.";
                return RedirectToAction(nameof(AreaAdmin));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(AreaAdmin));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites1(int productId)
        {
            var result = await _produtosService.AddProductToFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto adicionado com sucesso ao favoritos.";
                return RedirectToAction(nameof(Index));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveToFavorites1(int productId)
        {
            var result = await _produtosService.RemoveProductFromFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto removido com sucesso do favoritos.";
                return RedirectToAction(nameof(Index));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites2(int productId)
        {           
            var result = await _produtosService.AddProductToFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto adicionado com sucesso ao favoritos.";
                return RedirectToAction("Details", new { id = productId });
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction("Details", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveToFavorites2(int productId)
        {
            var result = await _produtosService.RemoveProductFromFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto removido com sucesso do favoritos.";
                return RedirectToAction("Details", new { id = productId });
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction("Details", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites3(int productId)
        {
            var result = await _produtosService.AddProductToFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto adicionado com sucesso ao favoritos.";
                return RedirectToAction(nameof(ShoppingCart));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(ShoppingCart));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveToFavorites3(int productId)
        {
            var result = await _produtosService.RemoveProductFromFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto removido com sucesso do favoritos.";
                return RedirectToAction(nameof(ShoppingCart));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(ShoppingCart));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveToFavorites4(int productId)
        {
            var result = await _produtosService.RemoveProductFromFavorites(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto removido com sucesso do favoritos.";
                return RedirectToAction(nameof(FavoriteCard));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(FavoriteCard));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToShoppingCart(int productId)
        {
            var result = await _produtosService.AddProductToShoppingCart(productId, User);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Produto adicionado com sucesso ao carrinho de compras.";
                return RedirectToAction("Details", new { id = productId });
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction("Details", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToShoppingCart1(int productId)
        {
            var result = await _produtosService.AddProductToShoppingCart(productId, User);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Produto adicionado com sucesso ao carrinho de compras.";
                return RedirectToAction(nameof(FavoriteCard));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(FavoriteCard));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveToShoppingCart(int productId)
        {
            var result = await _produtosService.RemoveProductFromShoppingCart(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto removido com sucesso do carrinho de compras.";
                return RedirectToAction(nameof(ShoppingCart));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(ShoppingCart));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BuyNow(int productId)
        {
            var result = await _produtosService.AddProductToShoppingCart(productId, User);

            if (result.Success)
            {
                //TempData["MessageSuccess"] = "Produto adicionado com sucesso ao carrinho de compras.";
                return RedirectToAction(nameof(ShoppingCart));
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction(nameof(ShoppingCart));
        }


        //----------------------------------------------------------------------------------------------------------------------
    }
}

