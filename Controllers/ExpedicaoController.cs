using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Data;
using Projeto_Estoque.Models;

namespace Projeto_Estoque.Controllers
{
    public class ExpedicaoController : Controller
    {
        private readonly EstoqueContext _context;

        public ExpedicaoController(EstoqueContext context)
        {
            _context = context;
        }

        // =====================================================================
        // INDEX — Lista todas as expedições
        // LINQ Q1: Include Pedido + ThenInclude Cliente
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            var expedicoes = await _context.Expedicoes
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                .OrderByDescending(e => e.DataEnvio)
                .ToListAsync();

            return View(expedicoes);
        }

        // =====================================================================
        // DETAILS — Detalhes completos da expedição
        // =====================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var expedicao = await _context.Expedicoes
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.PedidoProdutos)
                        .ThenInclude(pp => pp.Produto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expedicao == null) return NotFound();

            return View(expedicao);
        }

        // =====================================================================
        // CREATE GET — Formulário de criação
        // =====================================================================
        public async Task<IActionResult> Create()
        {
            var pedidosSemExpedicao = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Expedicao == null && p.Cliente != null)
                .ToListAsync();

            ViewData["PedidoId"] = new SelectList(
                pedidosSemExpedicao,
                "Id",
                "Id"
            );

            ViewData["PedidoSelectList"] = pedidosSemExpedicao
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"Pedido #{p.Id} — {p.Cliente?.Nome}"
                }).ToList();

            ViewData["StatusList"] = new SelectList(
                Enum.GetValues<StatusExpedicao>().Select(s => new
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }),
                "Value", "Text"
            );

            return View();
        }

        // =====================================================================
        // CREATE POST — Salva nova expedição com validações
        // RN1: PedidoId obrigatório
        // RN2: DataEnvio >= DataPedido
        // RN4: Pedido deve ter Cliente
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DataEnvio,Transportadora,Status,PedidoId")] Expedicao expedicao)
        {
            if (ModelState.IsValid)
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.Id == expedicao.PedidoId);

                if (pedido == null)
                {
                    ModelState.AddModelError("PedidoId", "Pedido não encontrado.");
                    await PopularViewDataCreate();
                    return View(expedicao);
                }

                if (pedido.Cliente == null)
                {
                    ModelState.AddModelError("PedidoId", "O pedido selecionado não possui cliente associado.");
                    await PopularViewDataCreate();
                    return View(expedicao);
                }

                if (expedicao.DataEnvio.Date < pedido.DataPedido.Date)
                {
                    ModelState.AddModelError("DataEnvio",
                        $"A data de envio não pode ser anterior à data do pedido ({pedido.DataPedido:dd/MM/yyyy}).");
                    await PopularViewDataCreate();
                    return View(expedicao);
                }

                _context.Add(expedicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopularViewDataCreate();
            return View(expedicao);
        }

        // =====================================================================
        // EDIT GET — Formulário de edição
        // RN5: Se status for Entregue, redireciona para Details (somente leitura)
        // =====================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expedicao = await _context.Expedicoes
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expedicao == null) return NotFound();

            // RN5 — Expedição já entregue: bloqueia edição e redireciona para visualização
            if (expedicao.Status == StatusExpedicao.Entregue)
            {
                TempData["MensagemAviso"] = "Esta expedição já foi entregue e não pode ser editada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["StatusList"] = new SelectList(
                Enum.GetValues<StatusExpedicao>().Select(s => new
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }),
                "Value", "Text",
                expedicao.Status.ToString()
            );

            return View(expedicao);
        }

        // =====================================================================
        // EDIT POST — Atualiza expedição com validações
        // RN2: DataEnvio >= DataPedido
        // RN3: Status Entregue não pode regredir
        // RN5: Status Entregue bloqueia qualquer alteração
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,DataEnvio,Transportadora,Status,PedidoId")] Expedicao expedicao)
        {
            if (id != expedicao.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var expedicaoAtual = await _context.Expedicoes
                    .AsNoTracking()
                    .Include(e => e.Pedido)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expedicaoAtual == null) return NotFound();

                // RN5 — Bloqueia qualquer alteração se já estiver Entregue
                if (expedicaoAtual.Status == StatusExpedicao.Entregue)
                {
                    TempData["MensagemAviso"] = "Esta expedição já foi entregue e não pode ser editada.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // RN3 — Não pode regredir de Entregue
                var statusesRegressivos = new[]
                {
                    StatusExpedicao.Pendente,
                    StatusExpedicao.EmSeparacao,
                    StatusExpedicao.ProntoParaEnvio
                };

                if (expedicaoAtual.Status == StatusExpedicao.Entregue &&
                    statusesRegressivos.Contains(expedicao.Status))
                {
                    ModelState.AddModelError("Status",
                        "Pedido já entregue não pode voltar para status anterior.");
                    await PopularViewDataEdit(expedicao);
                    return View(expedicao);
                }

                // RN2 — DataEnvio >= DataPedido
                var pedido = expedicaoAtual.Pedido
                    ?? await _context.Pedidos.FindAsync(expedicao.PedidoId);

                if (pedido != null && expedicao.DataEnvio.Date < pedido.DataPedido.Date)
                {
                    ModelState.AddModelError("DataEnvio",
                        $"A data de envio não pode ser anterior à data do pedido ({pedido.DataPedido:dd/MM/yyyy}).");
                    await PopularViewDataEdit(expedicao);
                    return View(expedicao);
                }

                try
                {
                    _context.Update(expedicao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpedicaoExists(expedicao.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopularViewDataEdit(expedicao);
            return View(expedicao);
        }

        // =====================================================================
        // DELETE GET — Confirmação de exclusão
        // =====================================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expedicao = await _context.Expedicoes
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expedicao == null) return NotFound();

            return View(expedicao);
        }

        // =====================================================================
        // DELETE POST — Remove expedição do banco
        // =====================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expedicao = await _context.Expedicoes.FindAsync(id);
            if (expedicao != null)
            {
                _context.Expedicoes.Remove(expedicao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // RELATORIOS — Consultas LINQ Q2 e Q3
        // =====================================================================
        public async Task<IActionResult> Relatorios()
        {
            var porTransportadora = await _context.Expedicoes
                .GroupBy(e => e.Transportadora)
                .Select(g => new
                {
                    Transportadora = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            int limiteEnvios = 1;
            var transportadorasAtivas = await _context.Expedicoes
                .GroupBy(e => e.Transportadora)
                .Where(g => g.Count() > limiteEnvios)
                .Select(g => new
                {
                    Transportadora = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            ViewData["PorTransportadora"] = porTransportadora;
            ViewData["TransportadorasAtivas"] = transportadorasAtivas;
            ViewData["LimiteEnvios"] = limiteEnvios;

            return View();
        }

        // =====================================================================
        // HELPERS PRIVADOS
        // =====================================================================
        private bool ExpedicaoExists(int id)
        {
            return _context.Expedicoes.Any(e => e.Id == id);
        }

        private async Task PopularViewDataCreate()
        {
            var pedidosSemExpedicao = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Expedicao == null && p.Cliente != null)
                .ToListAsync();

            ViewData["PedidoSelectList"] = pedidosSemExpedicao
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"Pedido #{p.Id} — {p.Cliente?.Nome}"
                }).ToList();

            ViewData["StatusList"] = new SelectList(
                Enum.GetValues<StatusExpedicao>().Select(s => new
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }),
                "Value", "Text"
            );
        }

        private async Task PopularViewDataEdit(Expedicao expedicao)
        {
            ViewData["StatusList"] = new SelectList(
                Enum.GetValues<StatusExpedicao>().Select(s => new
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }),
                "Value", "Text",
                expedicao.Status.ToString()
            );
        }
    }
}