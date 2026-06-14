namespace Projeto_Estoque.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public string? Status { get; set; }
        public decimal ValorTotal { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public ICollection<PedidoProduto>? PedidoProdutos { get; set; }
        public Expedicao? Expedicao { get; set; }
    }
}