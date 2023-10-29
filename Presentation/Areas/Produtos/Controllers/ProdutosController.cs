using Domain.Models;
using Domain.Interfaces.Produtos;
using Infra.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services.Account;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
            var (result, produtos) = await _produtosService.GetProductsAsync(category, searchString, page);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(produtos);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult SaveProduct() => View();

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            
            var (result, produto) = await _produtosService.GetProductByIdAsync(id);
            if (result.Success)
            {
                return View(produto); 
            }

            TempData["MessageError"] = result.Message;
            return View(nameof(Details));   
        }

        [HttpGet]
        public async Task<IActionResult> ShoppingCart()
        {

            var (result, cart) = await _produtosService.GetShoppingCartAsync(User);
            if (result.Success)
            {
                return View(cart); 
            }

            TempData["MessageError"] = result.Message;
            return View(nameof(Details));
        }


        //----------------------------------------------------------------------------------------------------


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveProduct(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _produtosService.SaveAsync(model);

                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Produto cadastrado com sucesso.";
                    return RedirectToAction(nameof(SaveProduct));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View();           
            }

           return View(model);            
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites(int productId)
        {           
            var result = await _produtosService.AddProductToFavorites(productId, User);

            if (result.Success)
            {
                TempData["MessageSuccess"] = "Produto adicionado com sucesso ao carrinho de compras.";
                return RedirectToAction("Details", new { id = productId });
            }

            TempData["MessageError"] = result.Message;
            return RedirectToAction("Details", new { id = productId });
        }


        //----------------------------------------------------------------------------------------------------------------------
    }
}

