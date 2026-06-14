using System.ComponentModel.DataAnnotations;

namespace SistemaEstoque.Models
{
    public class Expedicao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A data de envio é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Envio")]
        public DateTime DataEnvio { get; set; }

        [Required(ErrorMessage = "A transportadora é obrigatória.")]
        [StringLength(100, ErrorMessage = "Transportadora deve ter no máximo 100 caracteres.")]
        [Display(Name = "Transportadora")]
        public string Transportadora { get; set; } = string.Empty;

        [Required(ErrorMessage = "O status é obrigatório.")]
        [Display(Name = "Status")]
        public StatusExpedicao Status { get; set; } = StatusExpedicao.Pendente;

        // FK para Pedido (RN1: obrigatório)
        [Required(ErrorMessage = "O pedido é obrigatório.")]
        [Display(Name = "Pedido")]
        public int PedidoId { get; set; }

        // Navigation property
        public Pedido? Pedido { get; set; }
    }
}
