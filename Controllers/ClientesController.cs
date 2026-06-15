using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Data;
using Projeto_Estoque.Models;

namespace SistemaEstoque.Controllers
{
    public class ClientesController : Controller
    {
        private readonly EstoqueContext _context;

        public ClientesController(EstoqueContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Email,Telefone")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telefone")] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Indicadores
        public async Task<IActionResult> Indicadores()
        {
            // Consulta 1 - Cliente e Pedidos
            var consulta1 = await _context.Pedidos
                .Include(p => p.Cliente)
                .Select(p => new {
                    NomeCliente = p.Cliente.Nome,
                    IdPedido = p.Id,
                    ValorPedido = p.ValorTotal
                }).ToListAsync();

            // Consulta 2 - Quantidade de Pedidos por Cliente
            var consulta2 = await _context.Pedidos
                .GroupBy(p => p.Cliente.Nome)
                .Select(g => new {
                    NomeCliente = g.Key,
                    QuantidadePedidos = g.Count()
                }).ToListAsync();

            // Consulta 3 - Clientes com Mais de 3 Pedidos
            var consulta3 = await _context.Pedidos
                .Where(p => p.ValorTotal > 0)
                .GroupBy(p => p.Cliente.Nome)
                .Where(g => g.Count() > 3)
                .Select(g => new {
                    NomeCliente = g.Key,
                    TotalPedidos = g.Count()
                }).ToListAsync();

            // Enviando os dados para a View usando ViewBag para simplificar a exibição
            ViewBag.Consulta1 = consulta1;
            ViewBag.Consulta2 = consulta2;
            ViewBag.Consulta3 = consulta3;

            return View();
        }
        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }

}
