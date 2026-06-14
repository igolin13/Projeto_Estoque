namespace Projeto_Estoque.Models
{
    public class PedidoProduto
    {
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}