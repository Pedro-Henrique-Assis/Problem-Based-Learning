using System.Text.RegularExpressions;

namespace ProjetoPBL.DAO
{
    public static class Validador
    {
        //Modelo padrão de email:
        //'prefixo@sufixo.com'
        //'prefixo@sufixo.com.br'
        private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.(com|com\.br)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //Modelo padrão de cep:
        //'xxxxx-xxx'
        private static readonly Regex CepRegex = new Regex(
        @"^\d{5}-?\d{3}$",
        RegexOptions.Compiled);

        /// <summary>
        /// Valida se o e-mail informado corresponde ao padrão desejado.
        /// </summary>
        /// <param name="email">String de e-mail a validar.</param>
        /// <returns>True se for válido; caso contrário, false.</returns>
        public static bool ValidaEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// Valida se o CEP informado está no formato '12345-678'.
        /// </summary>
        /// <param name="cep">Cep que será verificado</param>
        /// <returns>True se for válido; caso contrário, false.</returns>
        public static bool ValidaCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return false;

            return CepRegex.IsMatch(cep);
        }
    }
}
