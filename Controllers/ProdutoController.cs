using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto_Estoque.Data;
using Projeto_Estoque.Models;

namespace Projeto_Estoque.Controllers

{
    public class ProdutoController : Controller
    {
            private readonly EstoqueContext _context;

            // O construtor recebe o contexto do banco automaticamente
            // isso se chama "injeção de dependência"
            public ProdutoController(EstoqueContext context)
            {
                _context = context;
            }


            // GET: /Produto
            // Lista todos os produtos cadastrados
            public async Task<IActionResult> Index()
            {
                return View(await _context.Produtos.ToListAsync());
            }

            public async Task<IActionResult> Details(int? id)
            {
                if (id == null)
                    return NotFound();

                var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);

                if (produto == null)
                    return NotFound();

                return View(produto);
            
            }


            // GET: /Produto/Create
            // Apenas abre a tela de cadastro vazia
            public IActionResult Create()
            {
                return View();
            }

            [HttpPost] // Criar novo produto // Recebe os dados do formulário e salva no banco
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,Preco,QuantidadeEstoque")]Produto produto)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(produto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(produto);
            }


            // GET: /Produto/Edit/5
            // Abre a tela de edição com os dados já preenchidos
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                    return NotFound();

                var produto = await _context.Produtos.FindAsync(id);

                if (produto == null)
                    return NotFound(); 

                return View(produto);


            }

            // Recebe os dados alterados e atualiza no banco

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind ("Id,Nome,Descricao,Preco,QuantidadeEstoque")]Produto produto)
            {
                if (id != produto.Id)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(produto);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProdutoExists(produto.Id))
                            return NotFound();
                        else
                            throw;
                    }
                    return RedirectToAction(nameof(Index));

                }

                return View(produto);

            }


            // Mostra a tela de confirmação de exclusão
            public async Task<IActionResult> Delete (int? id)
            {
                if (id == null)
                    return NotFound();

                var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);

                if (produto == null)
                    return NotFound();

                return View(produto);

            }

            // Recebe a confirmação e exclui o produto do banco

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]

            public async Task<IActionResult> DeleteConfirmed(int id)
            { 
                var produto = await _context.Produtos.FindAsync(id);
                if (produto!= null)
                    _context.Produtos.Remove(produto);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            }

            // Verifica se um produto existe pelo Id
            private bool ProdutoExists(int id)
            {
                return _context.Produtos.Any(e => e.Id == id);
            }
        }









    }

