using System.ComponentModel.DataAnnotations;

namespace Projeto_Estoque.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome do Cliente")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um endereço de e-mail válido.")]
        [StringLength(100, ErrorMessage = "O E-mail não pode exceder 100 caracteres.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório.")]
        [Phone(ErrorMessage = "Insira um formato de telefone válido.")]
        [StringLength(20, ErrorMessage = "O Telefone não pode exceder 20 caracteres.")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        // Relacionamento 1:N - Um cliente pode ter muitos pedidos
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}