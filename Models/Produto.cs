namespace Projeto_Estoque.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }

        // Comentado por enquanto:
        // public ICollection<PedidoProduto>? PedidoProdutos { get; set; }
    }
}