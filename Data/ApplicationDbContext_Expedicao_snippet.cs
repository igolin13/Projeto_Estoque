// =============================================================
// INSTRUÇÕES PARA O MEMBRO RESPONSÁVEL PELO DbContext
// =============================================================
// Adicione as linhas marcadas com ← ADICIONAR no seu ApplicationDbContext.cs
// =============================================================

using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Models;

namespace SistemaEstoque.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets dos outros módulos (já devem existir)
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoProduto> PedidoProdutos { get; set; }

        // ← ADICIONAR: DbSet da Expedição
        public DbSet<Expedicao> Expedicoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração existente dos outros módulos (manter como está)
            // Chave composta da tabela N:N
            modelBuilder.Entity<PedidoProduto>()
                .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });

            // ← ADICIONAR: Relacionamento 1:1 entre Pedido e Expedição
            modelBuilder.Entity<Expedicao>()
                .HasOne(e => e.Pedido)
                .WithOne(p => p.Expedicao)
                .HasForeignKey<Expedicao>(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Restrict); // não apaga pedido ao excluir expedição

            // ← ADICIONAR: Configurações de coluna para Expedicao
            modelBuilder.Entity<Expedicao>()
                .Property(e => e.Transportadora)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Expedicao>()
                .Property(e => e.Status)
                .HasConversion<string>() // salva o enum como texto no banco
                .HasMaxLength(30)
                .IsRequired();
        }
    }
}
