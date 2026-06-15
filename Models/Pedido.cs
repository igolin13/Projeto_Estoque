namespace Projeto_Estoque.Models;
{
    public class Pedido
    {
        public int Id { get; set; }
        public decimal ValorTotal { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
    }
}