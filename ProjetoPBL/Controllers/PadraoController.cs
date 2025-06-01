using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using Microsoft.AspNetCore.Http;

namespace ProjetoPBL.Controllers
{
    public class PadraoController<T> : Controller where T : PadraoViewModel
    {
        protected bool ExigeAutenticacao { get; set; } = true;
        protected PadraoDAO<T> DAO { get; set; }
        protected bool GeraProximoId { get; set; }
        protected string NomeViewIndex { get; set; } = "index";
        protected string NomeViewForm { get; set; } = "form";
        
        public IActionResult Index()
        {
            try
            {
                var lista = DAO.Listagem();
                return View(NomeViewIndex, lista);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }


        /// <summary>
        /// Prepara e exibe o formulário para a criação de um novo registro do tipo <typeparamref name="T"/>.
        /// Este método é tipicamente a action GET para uma operação de criação (Create).
        /// Ele inicializa um novo modelo, define a operação como "I" (Insert/Inserção)
        /// para a view, preenche quaisquer dados necessários para a view (como IDs ou listas para dropdowns)
        /// e, em seguida, renderiza a view de formulário especificada em <see cref="NomeViewForm"/>.
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>,
        /// representando a entidade a ser criada.</typeparam>
        /// <returns>
        /// Retorna uma <see cref="ViewResult"/> que renderiza a view do formulário (<see cref="NomeViewForm"/>)
        /// com um novo modelo <typeparamref name="T"/> para preenchimento.
        /// Em caso de exceção, retorna a View "Error" com os detalhes do erro.
        /// </returns>
        public virtual IActionResult Create()
        {
            try
            {
                ViewBag.Operacao = "I";
                T model = Activator.CreateInstance<T>(); // Cria uma nova instância do modelo genérico T.
                PreencheDadosParaView("I", model);
                return View(NomeViewForm, model);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }


        /// <summary>
        /// Prepara o modelo <paramref name="model"/> com dados adicionais antes de ser enviado para a View.
        /// Este método é virtual e pode ser sobrescrito por classes filhas para adicionar
        /// lógica de preenchimento específica para suas respectivas Views.
        /// A implementação base lida com a geração automática do próximo ID para o modelo
        /// se a propriedade <see cref="GeraProximoId"/> estiver definida como <c>true</c>
        /// e a operação atual for de Inserção ("I").
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>.</typeparam>
        /// <param name="Operacao">Uma string que indica a operação atual (por exemplo, "I" para Inserção, "A" para Atualização).
        /// Este parâmetro é usado para determinar se um novo ID deve ser gerado.</param>
        /// <param name="model">O objeto ViewModel do tipo <typeparamref name="T"/> que será preparado
        /// e, posteriormente, enviado para a View.</param>
        /// <remarks>
        /// Classes derivadas frequentemente sobrescrevem este método para popular ViewBags com listas
        /// para dropdowns (como a lista de sexos no <see cref="Controllers.UsuarioController"/>)
        /// ou para definir valores padrão para campos do modelo (como a data de instalação no <see cref="Controllers.SensorController"/>).
        /// </remarks>
        protected virtual void PreencheDadosParaView(string Operacao, T model)
        {
            if (GeraProximoId && Operacao == "I")
                model.Id = DAO.ProximoId();
        }


        /// <summary>
        /// Determina a ação de redirecionamento após a operação Save ser bem-sucedida.
        /// O comportamento padrão é redirecionar para a NomeViewIndex.
        /// Pode ser sobrescrito em classes filhas para personalizar o redirecionamento.
        /// </summary>
        /// <param name="operacao">A operação realizada ("I" para Insert, "A" para Update).</param>
        /// <returns>O IActionResult para o redirecionamento.</returns>
        protected virtual IActionResult GetSaveRedirectAction(string operacao)
        {
            return RedirectToAction(NomeViewIndex);
        }


        /// <summary>
        /// Salva os dados do modelo <paramref name="model"/> no banco de dados,
        /// realizando uma operação de Inserção ("I") ou Atualização ("A").
        /// Este método primeiro valida os dados do modelo. Se a validação for bem-sucedida,
        /// ele prossegue para chamar o método Insert ou Update do DAO correspondente.
        /// Após uma operação bem-sucedida, redireciona o usuário conforme definido por
        /// <see cref="GetSaveRedirectAction"/>.
        /// Em caso de falha na validação, a view do formulário é reexibida com as mensagens de erro.
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>,
        /// representando a entidade a ser salva.</typeparam>
        /// <param name="model">O objeto ViewModel do tipo <typeparamref name="T"/> contendo os dados
        /// a serem salvos. Este modelo é tipicamente preenchido a partir de um formulário HTML.</param>
        /// <param name="Operacao">Uma string que indica o tipo de operação a ser realizada:
        /// "I" para Inserção (novo registro) ou "A" para Atualização (alteração de registro existente).</param>
        /// <returns>
        /// Retorna um <see cref="IActionResult"/> que pode ser:
        /// - Um <see cref="RedirectToActionResult"/> para a view configurada em <see cref="GetSaveRedirectAction"/>
        ///   após uma operação de salvamento bem-sucedida.
        /// - Uma <see cref="ViewResult"/> renderizando <see cref="NomeViewForm"/> com o <paramref name="model"/>
        ///   e mensagens de erro, caso a validação dos dados falhe (<c>ModelState.IsValid == false</c>).
        /// - Uma <see cref="ViewResult"/> renderizando a view "Error" com um <see cref="ErrorViewModel"/>
        ///   em caso de exceção durante o processo.
        /// </returns>
        public virtual IActionResult Save(T model, string Operacao)
        {
            try
            {
                ValidaDados(model, Operacao);
                if (ModelState.IsValid == false)
                {
                    ViewBag.Operacao = Operacao;
                    PreencheDadosParaView(Operacao, model);
                    return View(NomeViewForm, model);
                }
                else
                {
                    if (Operacao == "I")
                        DAO.Insert(model);
                    else
                        DAO.Update(model);
                    return GetSaveRedirectAction(Operacao);
                }
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }


        /// <summary>
        /// Realiza validações de dados básicas e comuns para o modelo <paramref name="model"/>,
        /// principalmente focadas na integridade do ID da entidade, antes de operações de
        /// Inserção ("I") ou Atualização ("A").
        /// Este método é virtual e pode (e geralmente deve) ser sobrescrito em classes filhas
        /// para adicionar validações específicas da entidade que o controller gerencia.
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>,
        /// cujos dados estão sendo validados.</typeparam>
        /// <param name="model">O objeto ViewModel do tipo <typeparamref name="T"/> contendo os dados
        /// a serem validados.</param>
        /// <param name="operacao">Uma string que indica o tipo de operação para a qual a validação
        /// está sendo feita: "I" para Inserção ou "A" para Atualização.</param>
        /// <remarks>
        /// A implementação base:
        /// 1. Limpa o <see cref="ModelState"/> atual para remover quaisquer erros de validação anteriores.
        /// 2. Para operações de Inserção ("I"): verifica se o ID fornecido no modelo já existe no banco de dados.
        ///    Se existir, adiciona um erro ao <see cref="ModelState"/> para o campo "Id".
        /// 3. Para operações de Atualização ("A"): verifica se o registro com o ID fornecido no modelo
        ///    realmente existe no banco de dados. Se não existir, adiciona um erro ao <see cref="ModelState"/>.
        /// 4. Verifica se o ID do modelo é menor ou igual a zero, o que é considerado inválido.
        ///    Se for, adiciona um erro ao <see cref="ModelState"/>.
        ///
        /// Classes derivadas devem chamar `base.ValidaDados(model, operacao);` se desejarem incluir
        /// estas validações base e, em seguida, adicionar suas próprias regras de validação específicas
        /// (como verificação de campos obrigatórios, formatos, regras de negócio, etc.), como visto em
        /// <see cref="Controllers.UsuarioController.ValidaDados"/> e
        /// <see cref="Controllers.SensorController.ValidaDados"/>.
        /// </remarks>
        protected virtual void ValidaDados(T model, string operacao)
        {
            ModelState.Clear();
            if (operacao == "I" && DAO.Consulta(model.Id) != null)
                ModelState.AddModelError("Id", "Código já está em uso!");
            if (operacao == "A" && DAO.Consulta(model.Id) == null)
                ModelState.AddModelError("Id", "Este registro não existe!");
            if (model.Id <= 0)
                ModelState.AddModelError("Id", "Id inválido!");
        }


        /// <summary>
        /// Prepara e exibe o formulário para edição de um registro existente do tipo <typeparamref name="T"/>,
        /// identificado pelo <paramref name="id"/> fornecido.
        /// Este método é tipicamente a action GET para uma operação de edição (Update).
        /// Ele busca o registro no banco de dados, define a operação como "A" (Alteração/Atualização)
        /// para a view, preenche quaisquer dados necessários para a view (como IDs ou listas para dropdowns)
        /// e, em seguida, renderiza a view de formulário (<see cref="NomeViewForm"/>) preenchida com os dados do registro.
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>,
        /// representando a entidade a ser editada.</typeparam>
        /// <param name="id">O identificador único do registro a ser editado.</param>
        /// <returns>
        /// Retorna uma <see cref="ViewResult"/> que renderiza a view do formulário (<see cref="NomeViewForm"/>)
        /// com o modelo <typeparamref name="T"/> preenchido com os dados do registro encontrado, se o registro existir.
        /// Retorna um <see cref="RedirectToActionResult"/> para a <see cref="NomeViewIndex"/> (geralmente a página de listagem)
        /// se o registro com o <paramref name="id"/> fornecido não for encontrado no banco de dados.
        /// Em caso de exceção durante o processo, retorna a View "Error" com os detalhes do erro.
        /// </returns>
        public virtual IActionResult Edit(int id)
        {
            try
            {
                ViewBag.Operacao = "A";
                var model = DAO.Consulta(id);
                if (model == null)
                    return RedirectToAction(NomeViewIndex);
                else
                {
                    PreencheDadosParaView("A", model);
                    return View(NomeViewForm, model);
                }
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }


        /// <summary>
        /// Exclui um registro do tipo <typeparamref name="T"/> do banco de dados,
        /// identificado pelo <paramref name="id"/> fornecido.
        /// Este método é tipicamente a action acionada após uma confirmação de exclusão.
        /// Após a exclusão bem-sucedida, redireciona o usuário para a página de listagem
        /// (<see cref="NomeViewIndex"/>).
        /// </summary>
        /// <typeparam name="T">O tipo do ViewModel que herda de <see cref="PadraoViewModel"/>,
        /// representando a entidade a ser excluída.</typeparam>
        /// <param name="id">O identificador único do registro a ser excluído.</param>
        /// <returns>
        /// Retorna um <see cref="RedirectToActionResult"/> para a <see cref="NomeViewIndex"/>
        /// (geralmente a página de listagem da entidade) após a exclusão bem-sucedida.
        /// Em caso de exceção durante o processo de exclusão (por exemplo, violação de chave estrangeira
        /// ou erro de banco de dados), retorna a View "Error" com os detalhes do erro.
        /// </returns>
        /// <remarks>
        /// Este método virtual pode ser sobrescrito em classes filhas para adicionar lógicas específicas
        /// antes ou depois da exclusão, como fez o <see cref="Controllers.SensorController.Delete(int)"/>
        /// para remover o sensor também do FIWARE.
        /// O <see cref="Controllers.AdminController"/> e o <see cref="Controllers.ChamadoController"/>
        /// implementam suas próprias lógicas de exclusão, não utilizando diretamente este método base
        /// através de uma chamada `base.Delete(id)`.
        /// </remarks>
        public virtual IActionResult Delete(int id)
        {
            try
            {
                DAO.Delete(id);
                return RedirectToAction(NomeViewIndex);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }


        /// <summary>
        /// Sobrescreve o método OnActionExecuting para implementar a lógica de verificação de autenticação
        /// antes que qualquer Action (método de controller) seja executada.
        /// Este método é um filtro de ação que é invocado automaticamente pelo ASP.NET Core
        /// como parte do ciclo de vida da requisição, antes da execução da Action do controller.
        /// </summary>
        /// <param name="context">
        /// O objeto <see cref="ActionExecutingContext"/> que contém informações sobre a Action
        /// que está prestes a ser executada, incluindo o <see cref="HttpContext"/> (e, consequentemente, a sessão).
        /// </param>
        /// <remarks>
        /// A lógica de autenticação é condicional com base na propriedade <see cref="ExigeAutenticacao"/>
        /// da classe do controller:
        /// - Se <see cref="ExigeAutenticacao"/> for <c>true</c> E o usuário não estiver logado
        ///   (verificado através de <see cref="HelperControllers.VerificaUserLogado"/>),
        ///   a execução da Action atual é interrompida e o usuário é redirecionado para a página de Login
        ///   (Action "Index" do Controller "Login").
        /// - Caso contrário (se <see cref="ExigeAutenticacao"/> for <c>false</c> ou se o usuário estiver logado),
        ///   uma variável <c>ViewBag.Logado</c> é definida como <c>true</c> (para ser usada nas Views para, por exemplo,
        ///   ajustar a interface do usuário) e a execução da Action prossegue normalmente chamando
        ///   a implementação base <c>base.OnActionExecuting(context)</c>.
        ///
        /// Este mecanismo é usado por todos os controllers que herdam de <see cref="PadraoController{T}"/>,
        /// como <see cref="Controllers.UsuarioController"/>, <see cref="Controllers.HomeController"/>,
        /// <see cref="Controllers.SobreController"/>, <see cref="Controllers.DashboardController"/>,
        /// e <see cref="Controllers.ConsultaSensorController"/>.
        /// A propriedade <c>ExigeAutenticacao</c> é definida como <c>true</c> por padrão em <see cref="PadraoController{T}"/>,
        /// mas é explicitamente definida como <c>false</c> no construtor de <see cref="Controllers.UsuarioController"/>,
        /// indicando que, por padrão, as actions do <c>UsuarioController</c> (como a de criação de usuário)
        /// não exigem que o usuário esteja logado para serem acessadas, a menos que uma action específica
        /// sobrescreva este comportamento ou a propriedade <c>ExigeAutenticacao</c> seja alterada.
        /// </remarks>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ExigeAutenticacao && !HelperControllers.VerificaUserLogado(HttpContext.Session))
                context.Result = RedirectToAction("Index", "Login");
            else
            {
                ViewBag.Logado = true;
                base.OnActionExecuting(context);
            }
        }
    }
}
