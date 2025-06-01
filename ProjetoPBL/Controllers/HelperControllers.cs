using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ProjetoPBL.Controllers
{
    public class HelperControllers : Controller
    {
        /// <summary>
        /// Verifica se o usuário está logado no sistema.
        /// Este método estático acessa a sessão HTTP para determinar o status de login do usuário.
        /// </summary>
        /// <param name="session">O objeto ISession que representa a sessão atual do usuário.
        /// Este objeto é usado para recuperar informações armazenadas na sessão, como o status de login.
        /// </param>
        /// <returns>
        /// Retorna true se a chave "Logado" existir na sessão e tiver um valor (indicando que o usuário está logado).
        /// Retorna false se a chave "Logado" não existir na sessão ou for nula (indicando que o usuário não está logado).
        /// </returns>
        public static Boolean VerificaUserLogado(ISession session)
        {
            string logado = session.GetString("Logado");
            if (logado == null)
                return false;
            else
                return true;
        }
    }
}
