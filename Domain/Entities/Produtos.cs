namespace Domain.Entities
{
    public class Produtos
    {       
        public int ID { get;  private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public string PathImage { get; set; }

        public void AdicionarEstoque(int quantidade)
        {
            Stock += quantidade;
        }

        public void RemoverEstoque(int quantidade)
        {
            Stock -= quantidade;
        }
    }
}
