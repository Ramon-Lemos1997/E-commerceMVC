using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ProductModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo Descrição é obrigatório.")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O campo Preço é obrigatório.")]
        [RegularExpression(@"^\d+(\,\d{1,2})?$", ErrorMessage = "Somente números são aceitos no campo de preço.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "O campo Quantidade em Estoque é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade em estoque deve ser maior ou igual a zero.")]
        [Display(Name = "Quantidade em estoque")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "O campo Categoria é obrigatório.")]
        [Display(Name = "Categoria")]
        public string Category { get; set; }

        [Required(ErrorMessage = "O campo Imagem é obrigatório.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "A imagem não pode exceder 2 megabytes.")]
        [Display(Name = "Imagem do produto (*Tamanho máximo 5 megabytes.)")]
        public IFormFile Image { get; set; }


        public ProductModel()
        {
            // Adicione inicializações padrão ou deixe o construtor vazio, se não for necessário fazer nada específico.
        }
        public ProductModel(int id, string name, string description, decimal price, int stock, string category, IFormFile image)
        {
            ID = id;
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            Category = category;
            Image = image;
        }

        public ProductModel(string name, string description, decimal price, int stock, string category, IFormFile image)
        {
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            Category = category;
            Image = image;
        }

       
    }
}
