namespace Projeto_Estoque.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public decimal ValorTotal { get; set; }

        // FK para Cliente — relacionamento 1:N
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Relacionamento N:N com Produto via tabela intermediária PedidoProduto
        public ICollection<PedidoProduto>? PedidoProdutos { get; set; }

        // Relacionamento 1:1 com Expedicao — necessário para o EstoqueContext
        public Expedicao? Expedicao { get; set; }
    }
}