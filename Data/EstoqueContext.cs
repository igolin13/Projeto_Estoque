using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Models;


namespace Projeto_Estoque.Data
{
    public class EstoqueContext : DbContext
    {
        public EstoqueContext(DbContextOptions<EstoqueContext> options)
            : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoProduto> PedidoProdutos { get; set; }
        public DbSet<Expedicao> Expedicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PedidoProduto>()
                .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });

            modelBuilder.Entity<Expedicao>()
                .HasOne(e => e.Pedido)
                .WithOne(p => p.Expedicao)
                .HasForeignKey<Expedicao>(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expedicao>()
                .Property(e => e.Transportadora)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Expedicao>()
                .Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
        }
    }
}