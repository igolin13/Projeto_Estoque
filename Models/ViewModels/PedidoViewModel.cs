using Microsoft.AspNetCore.Mvc.Rendering;

namespace Projeto_Estoque.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado na tela de criação e edição de pedidos.
    /// Carrega as listas de clientes e produtos disponíveis para seleção.
    /// </summary>
    public class PedidoViewModel
    {
        public int Id { get; set; }

        public DateTime DataPedido { get; set; } = DateTime.Now;

        public decimal ValorTotal { get; set; }

        public int ClienteId { get; set; }

        // Listas para os dropdowns
        public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Produtos { get; set; } = new List<SelectListItem>();

        // Itens selecionados no pedido
        public List<ItemPedidoViewModel> ItensSelecionados { get; set; } = new();
    }

    public class ItemPedidoViewModel
    {
        public int ProdutoId { get; set; }

        public string NomeProduto { get; set; } = string.Empty;

        public decimal PrecoProduto { get; set; }

        public int Quantidade { get; set; }

        public decimal SubTotal => PrecoProduto * Quantidade;
    }
}
