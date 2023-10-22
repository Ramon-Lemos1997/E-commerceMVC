using Domain.Models;
using Domain.Interfaces.Produtos;
using Infra.Data.Context;
using Microsoft.AspNetCore.Mvc;


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
        public async Task<IActionResult> Index()
        {
            var (result, produtos) = await _produtosService.GetProductsAsync();

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            return View(produtos);
        }

        public IActionResult Save() => View();


        //----------------------------------------------------------------------------------------------------


        [HttpPost]
        public async Task<IActionResult> Save(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _produtosService.SaveAsync(model);

                if (result.Success)
                {
                    TempData["MessageSuccess"] = "Produto cadastrado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View();           
            }

           return View(model);            
        }
    

       
       //----------------------------------------------------------------------------------------------------------------------
    }
}

