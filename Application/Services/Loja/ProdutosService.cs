using Domain.Models;
using Domain.Interfaces.Infra.Data;
using Domain.Interfaces.Produtos;
using Domain.Entities;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Loja
{
    public class ProdutosService : IProdutosInterface
    {
        private readonly ApplicationDbContext _context;
        private readonly IImagesInterface _imageService;
        public ProdutosService(IImagesInterface imageService, ApplicationDbContext context)
        {
            _imageService = imageService;
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------
        public async Task<(OperationResultModel, IEnumerable<Produtos>)> GetProductsAsync()
        {
            var produtos = await _context.Produtos.ToListAsync();

            if (produtos == null || produtos.Count == 0)
            {
                return (new OperationResultModel(false, "Não há produtos para ser exibido."), null);
            }
            return (new OperationResultModel(true, "Dados obtidos com sucesso."), produtos);
        }



        public async Task<OperationResultModel> SaveAsync(ProductModel model)
        {
            if (model == null)
            {
                return new OperationResultModel(false, "Nenhum dado recebido.");
            }
            if (model.Image.Length > 5 * 1024 * 1024) // 5 megabytes em bytes
            {
                return new OperationResultModel(false, "A imagem não pode exceder 5 megabytes.");
            }


            var (result, imagePath) = await _imageService.UploadImageAsync(model.Image);

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

        //----------------------------------------------------------------------------------------------------------
    }
}
