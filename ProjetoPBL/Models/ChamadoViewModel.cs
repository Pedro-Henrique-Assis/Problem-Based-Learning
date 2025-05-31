using System;
using System.Reflection.PortableExecutable;

namespace ProjetoPBL.Models
{
    public class ChamadoViewModel : PadraoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Status {  get; set; }
        public DateTime DataAbertura { get; set; }
        public int UsuarioId { get; set; }
        public string Resposta { get; set; }

    }
}
