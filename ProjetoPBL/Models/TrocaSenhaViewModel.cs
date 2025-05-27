using System.ComponentModel.DataAnnotations;

namespace ProjetoPBL.Models
{
    public class TrocaSenhaViewModel
    {
        [Required(ErrorMessage = "O campo Login é obrigatório.")]
        public string LoginUsuario { get; set; }

        [Required(ErrorMessage = "O campo Nova Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não conferem.")]
        [Required(ErrorMessage = "O campo Confirmação da Nova Senha é obrigatório.")]
        public string ConfirmacaoNovaSenha { get; set; }
    }
}
