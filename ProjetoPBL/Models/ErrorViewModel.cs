using System;

namespace ProjetoPBL.Models
{
    public class ErrorViewModel
    {
        public ErrorViewModel(string msgErro)
        {
            Erro = msgErro;
        }

        public ErrorViewModel()
        { }

        public string Erro { get; set; }

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
