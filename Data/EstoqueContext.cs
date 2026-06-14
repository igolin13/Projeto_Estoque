using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Models;

namespace Projeto_Estoque.Data
{
    public class EstoqueContext : DbContext
    {
        public EstoqueContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }

        // As outras linhas ficam comentadas por enquanto:
        // public DbSet<Cliente> Clientes { get; set; }
        // public DbSet<Pedido> Pedidos { get; set; }
        // public DbSet<PedidoProduto> PedidoProdutos { get; set; }
        // public DbSet<Expedicao> Expedicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Comentado também pois depende de PedidoProduto
            // modelBuilder.Entity<PedidoProduto>()
            //     .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });
        }
    }
}