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
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoProduto> PedidoProdutos { get; set; }
        public DbSet<Expedicao> Expedicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // GSD-SPEC: Chave composta da tabela intermediária N:N PedidoProduto
            modelBuilder.Entity<PedidoProduto>()
                .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });
        }
    }
}