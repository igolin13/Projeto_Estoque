using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Data;
using Projeto_Estoque.Models;
using Projeto_Estoque.Models.ViewModels;
namespace Projeto_Estoque.Controllers

{
    public class PedidoController : Controller
    {
        private readonly EstoqueContext _context;

        public PedidoController(EstoqueContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX - Lista todos os pedidos com cliente
        // LINQ: Include + Select (Consulta 1 do research.md)
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // [GSD - Generator] IA gerou a consulta base com Include
            // [GSD - Selector] Equipe escolheu exibir Cliente junto ao Pedido
            // [GSD - Discriminator] IA validou o uso correto de Include para evitar N+1 queries
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoProdutos)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // ============================================================
        // DETAILS - Detalhes do pedido com produtos e expedição
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoProdutos)
                    .ThenInclude(pp => pp.Produto)
                .Include(p => p.Expedicao)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // ============================================================
        // CREATE GET - Tela de criação do pedido
        // ============================================================
        public IActionResult Create()
        {
            var viewModel = new PedidoViewModel
            {
                DataPedido = DateTime.Now,
                Clientes = _context.Clientes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nome
                    }).ToList(),
                Produtos = _context.Produtos
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Nome} - R$ {p.Preco:F2}"
                    }).ToList()
            };

            return View(viewModel);
        }

        // ============================================================
        // CREATE POST - Salva o pedido e cria os PedidoProdutos
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int clienteId,
            List<int> produtoIds,
            List<int> quantidades)
        {
            if (clienteId == 0 || produtoIds == null || produtoIds.Count == 0)
            {
                TempData["Erro"] = "Selecione um cliente e ao menos um produto.";
                return RedirectToAction(nameof(Create));
            }

            // [GSD - Generator] IA sugeriu calcular o ValorTotal antes de salvar
            // [GSD - Selector] Equipe optou por buscar o preço do banco, não do form (segurança)
            // [GSD - Discriminator] IA validou que preço deve vir do banco para evitar manipulação

            // Busca preços dos produtos no banco (nunca confiar no valor do form)
            var produtos = await _context.Produtos
                .Where(p => produtoIds.Contains(p.Id))
                .ToListAsync();

            // Calcula o valor total: Σ (Preço × Quantidade)
            decimal valorTotal = 0;
            var itensPedido = new List<PedidoProduto>();

            for (int i = 0; i < produtoIds.Count; i++)
            {
                var produto = produtos.FirstOrDefault(p => p.Id == produtoIds[i]);
                if (produto == null) continue;

                int qtd = i < quantidades.Count ? quantidades[i] : 1;
                valorTotal += produto.Preco * qtd;

                itensPedido.Add(new PedidoProduto
                {
                    ProdutoId = produto.Id,
                    Quantidade = qtd
                });
            }

            var pedido = new Pedido
            {
                DataPedido = DateTime.Now,
                ClienteId = clienteId,
                ValorTotal = valorTotal,
                PedidoProdutos = itensPedido
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Pedido #{pedido.Id} criado com sucesso!";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        // ============================================================
        // EDIT GET
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProdutos)
                    .ThenInclude(pp => pp.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            var viewModel = new PedidoViewModel
            {
                Id = pedido.Id,
                DataPedido = pedido.DataPedido,
                ValorTotal = pedido.ValorTotal,
                ClienteId = pedido.ClienteId,
                Clientes = _context.Clientes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nome,
                        Selected = c.Id == pedido.ClienteId
                    }).ToList(),
                Produtos = _context.Produtos
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Nome} - R$ {p.Preco:F2}"
                    }).ToList(),
                ItensSelecionados = pedido.PedidoProdutos?.Select(pp => new ItemPedidoViewModel
                {
                    ProdutoId = pp.ProdutoId,
                    NomeProduto = pp.Produto?.Nome ?? "",
                    PrecoProduto = pp.Produto?.Preco ?? 0,
                    Quantidade = pp.Quantidade
                }).ToList() ?? new()
            };

            return View(viewModel);
        }

        // ============================================================
        // EDIT POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            int clienteId,
            List<int> produtoIds,
            List<int> quantidades)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProdutos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // Remove os itens antigos e recalcula
            _context.PedidoProdutos.RemoveRange(pedido.PedidoProdutos!);

            var produtos = await _context.Produtos
                .Where(p => produtoIds.Contains(p.Id))
                .ToListAsync();

            decimal valorTotal = 0;
            var novosItens = new List<PedidoProduto>();

            for (int i = 0; i < produtoIds.Count; i++)
            {
                var produto = produtos.FirstOrDefault(p => p.Id == produtoIds[i]);
                if (produto == null) continue;

                int qtd = i < quantidades.Count ? quantidades[i] : 1;
                valorTotal += produto.Preco * qtd;

                novosItens.Add(new PedidoProduto
                {
                    PedidoId = id,
                    ProdutoId = produto.Id,
                    Quantidade = qtd
                });
            }

            pedido.ClienteId = clienteId;
            pedido.ValorTotal = valorTotal;
            pedido.PedidoProdutos = novosItens;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Pedido #{id} atualizado com sucesso!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ============================================================
        // DELETE GET - Confirmação
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoProdutos)
                    .ThenInclude(pp => pp.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // ============================================================
        // DELETE POST - Confirmado
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoProdutos)
                .Include(p => p.Expedicao)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido != null)
            {
                if (pedido.Expedicao != null)
                    _context.Expedicoes.Remove(pedido.Expedicao);

                _context.PedidoProdutos.RemoveRange(pedido.PedidoProdutos!);
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] = $"Pedido #{id} removido com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // CONSULTAS LINQ - Tela de relatórios
        // Implementa as 3 consultas do research.md
        // ============================================================
        public async Task<IActionResult> Consultas()
        {
            // [GSD - Generator] IA gerou as 3 consultas LINQ
            // [GSD - Selector] Equipe selecionou GroupBy + Having para análise de clientes
            // [GSD - Discriminator] IA validou sintaxe e corrigiu Include faltante no GroupBy

            // Consulta 1: Pedidos com seus clientes
            var pedidosComClientes = await _context.Pedidos
                .Include(p => p.Cliente)
                .Select(p => new
                {
                    Pedido = p.Id,
                    Cliente = p.Cliente!.Nome,
                    Data = p.DataPedido,
                    Valor = p.ValorTotal
                })
                .OrderByDescending(p => p.Data)
                .ToListAsync();

            // Consulta 2: Agrupamento - total de pedidos por cliente
            var pedidosPorCliente = await _context.Pedidos
                .Include(p => p.Cliente)
                .GroupBy(p => p.Cliente!.Nome)
                .Select(g => new
                {
                    Cliente = g.Key,
                    TotalPedidos = g.Count(),
                    ValorTotal = g.Sum(p => p.ValorTotal)
                })
                .OrderByDescending(g => g.TotalPedidos)
                .ToListAsync();

            // Consulta 3: Clientes com mais de 2 pedidos (WHERE + HAVING)
            var clientesFrequentes = await _context.Pedidos
                .Include(p => p.Cliente)
                .GroupBy(p => p.Cliente!.Nome)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    Cliente = g.Key,
                    Quantidade = g.Count(),
                    ValorAcumulado = g.Sum(p => p.ValorTotal)
                })
                .ToListAsync();

            ViewBag.PedidosComClientes = pedidosComClientes;
            ViewBag.PedidosPorCliente = pedidosPorCliente;
            ViewBag.ClientesFrequentes = clientesFrequentes;

            return View();
        }
    }
}
