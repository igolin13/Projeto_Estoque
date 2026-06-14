namespace Projeto_Estoque.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public DateTime DataCadastro { get; set; }

        public ICollection<Pedido>? Pedidos { get; set; }
    }
}