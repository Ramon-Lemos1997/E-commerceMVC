
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class EditProductModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo Descrição é obrigatório.")]
        [MaxLength(50000, ErrorMessage = "A descrição não pode ter mais de 50000 caracteres.")]
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
     
        public EditProductModel()
        {

        }
        public EditProductModel(int id, string name, string description, decimal price, int stock, string category)
        {
            ID = id;
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            Category = category;          
        }
      
    }
}
