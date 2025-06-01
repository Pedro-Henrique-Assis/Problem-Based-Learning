using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ProjetoPBL.Controllers
{
    public class UsuarioController : PadraoController<UsuarioViewModel>
    {
        public UsuarioController() 
        {
            DAO = new UsuarioDAO();
            GeraProximoId = true;
            ExigeAutenticacao = false;
        }


        /// <summary>
        /// Action para exibir a página de troca de senha.
        /// Este método é acessado via GET e não requer que o usuário esteja autenticado
        /// para visualizar o formulário de troca de senha.
        /// Ele verifica o status de login atual para possivelmente ajustar a interface do usuário
        /// (por exemplo, exibir um layout diferente se o usuário já estiver logado) e
        /// apresenta um formulário limpo para a troca de senha.
        /// </summary>
        /// <returns>
        /// Retorna uma <see cref="ViewResult"/> que renderiza a view "TrocaSenha"
        /// (ou uma view com nome correspondente) com um novo <see cref="TrocaSenhaViewModel"/>
        /// para o preenchimento dos dados de troca de senha.
        /// </returns>
        [HttpGet]
        public IActionResult TrocaSenha()
        {
            // A troca de senha não exige autenticação para ser acessada
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View(new TrocaSenhaViewModel());
        }


        /// <summary>
        /// Processa a submissão do formulário de troca de senha.
        /// Este método é acionado via POST e é protegido contra CSRF com <see cref="ValidateAntiForgeryTokenAttribute"/>.
        /// Ele valida os dados recebidos, verifica se o usuário existe, se a nova senha é diferente da antiga
        /// e, em caso de sucesso, atualiza a senha do usuário no banco de dados e redireciona para a página de login.
        /// </summary>
        /// <param name="model">Um objeto <see cref="TrocaSenhaViewModel"/> contendo o login do usuário,
        /// a nova senha e a confirmação da nova senha, submetidos através do formulário.</param>
        /// <returns>
        /// - <see cref="ViewResult"/> para a view "TrocaSenha" com o <paramref name="model"/> e mensagens de erro
        ///   no <see cref="ModelState"/> ou <see cref="ViewBag"/>, caso ocorram erros de validação ou
        ///   durante o processo de atualização.
        /// - <see cref="RedirectToActionResult"/> para a Action "Index" do Controller "Login" após a
        ///   alteração bem-sucedida da senha, com uma mensagem de sucesso em <see cref="TempData"/>.
        /// </returns>
        /// <remarks>
        /// O método primeiro verifica o status de login para fins de layout (via <see cref="ViewBag.Logado"/>).
        /// Em seguida, valida o <see cref="ModelState"/>. Se inválido (por exemplo, senhas não conferem ou campos obrigatórios vazios,
        /// conforme definido em <see cref="TrocaSenhaViewModel"/>), retorna ao formulário.
        /// Procura pelo usuário com base no login fornecido. Se não encontrado, adiciona um erro.
        /// Verifica se a nova senha é igual à senha atual do usuário. Se for, adiciona um erro.
        /// Se todas as verificações passarem, atualiza a senha do usuário no objeto `usuarioParaAtualizar`
        /// e chama `DAO.Update()` para persistir a alteração. A `PadraoDAO.Update` e a stored procedure `spUpdate_usuarios`
        /// esperam o modelo completo, e o objeto `usuarioParaAtualizar` garante que apenas a senha seja efetivamente alterada,
        /// mantendo os demais dados do usuário.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarNovaSenha(TrocaSenhaViewModel model)
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                return View("TrocaSenha", model);
            }

            var usuarioDAO = (UsuarioDAO)DAO; // Cast para ter acesso aos métodos específicos
            var listaUsuarios = usuarioDAO.Listagem();
            UsuarioViewModel usuarioParaAtualizar = listaUsuarios.FirstOrDefault(u => u.LoginUsuario == model.LoginUsuario);

            if (usuarioParaAtualizar == null)
            {
                ModelState.AddModelError("LoginUsuario", "Usuário não encontrado.");
                return View("TrocaSenha", model);
            }

            if (usuarioParaAtualizar.Senha == model.NovaSenha)
            {
                ModelState.AddModelError("NovaSenha", "A nova senha não pode ser igual à senha antiga.");
                return View("TrocaSenha", model);
            }

            // Atualiza apenas a senha do usuário
            usuarioParaAtualizar.Senha = model.NovaSenha;

            try
            {
                // Como o método Update do PadraoDAO espera o modelo completo,
                // e a stored procedure spUpdate_usuarios atualiza todos os campos,
                // é necessário garantir que os outros campos não sejam alterados indevidamente.
                // O DAO.Update já recebe o usuarioParaAtualizar que contém todos os dados
                // originais, exceto a senha que foi modificada.
                DAO.Update(usuarioParaAtualizar);

                TempData["MensagemSucesso"] = "Senha alterada com sucesso! Você já pode fazer login com a nova senha.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Ocorreu um erro ao tentar atualizar a senha.";
                return View("TrocaSenha", model);
            }
        }

        /// <summary>
        /// Sobrescreve o método de redirecionamento para o UsuarioController.
        /// Se a operação for de Inserção ("I"), redireciona para a tela de Login.
        /// Caso contrário, mantém o comportamento padrão da PadraoController.
        /// </summary>
        /// <param name="operacao">A operação realizada ("I" para Insert, "A" para Update).</param>
        /// <returns>O IActionResult para o redirecionamento.</returns>
        protected override IActionResult GetSaveRedirectAction(string operacao)
        {
            if (operacao == "I")
            {
                // Redireciona para Login/Index especificamente na inserção de usuário
                return RedirectToAction("Index", "Login");
            }
            else if (operacao == "A")
            {
                TempData["MensagemSucessoAdmin"] = "Usuário atualizado com sucesso!";
                return RedirectToAction("ConsultaAvancada", "Admin");
            }
            else
            {
                return base.GetSaveRedirectAction(operacao);
            }
        }


        /// <summary>
        /// Realiza validações específicas para os dados do <see cref="UsuarioViewModel"/>,
        /// estendendo as validações da classe base <see cref="PadraoController{T}.ValidaDados"/>.
        /// Este método verifica a unicidade do login, formato de e-mail e CEP, data de nascimento,
        /// preenchimento de campos obrigatórios, tamanho da imagem e regras de senha.
        /// Além disso, trata a lógica de manutenção da senha e da imagem durante operações de atualização ("A"),
        /// e a lógica de permissão para alteração do status de administrador (IsAdmin).
        /// </summary>
        /// <param name="usuario">O objeto <see cref="UsuarioViewModel"/> contendo os dados do usuário a serem validados.</param>
        /// <param name="operacao">Uma string que indica o tipo de operação: "I" para Inserção ou "A" para Atualização.</param>
        /// <remarks>
        /// As validações incluem:
        /// - Unicidade do <see cref="UsuarioViewModel.LoginUsuario"/> (apenas se diferente do ID atual em caso de atualização).
        /// - Validações de ID da classe base.
        /// - Nome: Obrigatório, entre 2 e 200 caracteres, contendo apenas letras, espaços e apóstrofos.
        /// - Email: Formato válido (usando <see cref="DAO.Validador.ValidaEmail"/>).
        /// - DataNascimento: Obrigatória e não pode ser no futuro.
        /// - CEP: Formato válido (usando <see cref="DAO.Validador.ValidaCep"/>).
        /// - Logradouro, Cidade, Estado: Obrigatórios.
        /// - Numero: Deve ser um número válido (maior que zero).
        /// - SexoId: Deve ser um valor válido (maior que zero).
        /// - Imagem: Se fornecida, tamanho limitado a 2 MB.
        /// - Senha: Obrigatória apenas na inserção ("I").
        ///
        /// Para operações de Atualização ("A") e se o <see cref="ModelState"/> for válido até o momento:
        /// - Senha: Se não fornecida no formulário, a senha atual do usuário é mantida.
        /// - Status de Administrador (<see cref="UsuarioViewModel.IsAdmin"/>):
        ///     - Se o editor logado for administrador:
        ///         - Um administrador não pode remover seu próprio status de administrador.
        ///         - Não é possível remover o status de administrador do último administrador do sistema.
        ///     - Se o editor logado não for administrador, o status <c>IsAdmin</c> do usuário sendo editado não pode ser alterado.
        /// - Imagem:
        ///     - Se a opção <see cref="UsuarioViewModel.RemoverImagemAtual"/> estiver marcada, a imagem é definida como nula.
        ///     - Se uma nova <see cref="UsuarioViewModel.Imagem"/> for enviada, ela é convertida para bytes.
        ///     - Se nenhuma nova imagem for enviada e a remoção não for solicitada, a imagem atual é mantida.
        ///
        /// Este método utiliza <see cref="ModelState.AddModelError"/> para registrar quaisquer erros de validação.
        /// </remarks>
        protected override void ValidaDados(UsuarioViewModel usuario, string operacao)
        {
            bool usuarioJaExiste = ((UsuarioDAO)DAO).Listagem().Any(u => u.LoginUsuario == usuario.LoginUsuario && u.Id != usuario.Id);

            //Mantém a validação de Id presente na PadraoController
            base.ValidaDados(usuario, operacao);

            if (string.IsNullOrEmpty(usuario.Nome))
                ModelState.AddModelError("Nome", "Preencha o nome.");
            else
            {
                //Expressão regular para validação extra do nome
                var nomeRegex = new Regex(@"^[a-zA-ZÀ-ú\s']{2,200}$");
                if (!nomeRegex.IsMatch(usuario.Nome))
                {
                    ModelState.AddModelError("Nome", "O Nome deve conter apenas letras, espaços e apóstrofos (')");
                }
                else if (usuario.Nome.Trim().Length < 2)
                {
                    ModelState.AddModelError("Nome", "O Nome deve ter pelo menos 2 caracteres.");
                }
            }
            if (!Validador.ValidaEmail(usuario.Email))
                ModelState.AddModelError("Email", "O formato de email está incorreto. Modelo: prefixo@sufixo.com(.br)");

            if (usuario.DataNascimento > DateTime.Now)
                ModelState.AddModelError("DataNascimento", "Data inválida!");
            else if (usuario.DataNascimento == DateTime.MinValue) // Verifica se a data foi deixada vazia
                ModelState.AddModelError("DataNascimento", "A data de nascimento é obrigatória.");

            if (!Validador.ValidaCep(usuario.Cep))
            ModelState.AddModelError("Cep", "O formato do cep está incorreto. Modelo: xxxxx-xxx ou xxxxxxxx");
            if (string.IsNullOrEmpty(usuario.Logradouro))
                ModelState.AddModelError("Logradouro", "Preencha o logradouro.");
            if (usuario.Numero <= 0)
                ModelState.AddModelError("Numero", "Preencha um número válido.");
            if (string.IsNullOrEmpty(usuario.Cidade))
                ModelState.AddModelError("Cidade", "Preencha a cidade.");
            if (string.IsNullOrEmpty(usuario.Estado))
                ModelState.AddModelError("Estado", "Preencha o estado.");
            if (usuario.SexoId <= 0)
                ModelState.AddModelError("SexoId", "Escolha um sexo válido.");
            if (usuario.Imagem != null && usuario.Imagem.Length / 1024 / 1024 >= 2.048)
                ModelState.AddModelError("Imagem", "Imagem limitada a 2 mb.");

            if (usuarioJaExiste)
                ModelState.AddModelError("LoginUsuario", "Usuário já cadastrado");

            // Na inserção, a senha é obrigatória
            if (operacao == "I" && string.IsNullOrEmpty(usuario.Senha))
                ModelState.AddModelError("Senha", "A senha é obrigatória.");

            if (ModelState.IsValid)
            {
                // Se for uma alteração, busca os dados atuais para manter
                // a imagem e a senha, caso não sejam alteradas.
                if (operacao == "A")
                {
                    // Evita consulta dupla ao banco se já tiver os dados.
                    UsuarioViewModel usuarioAtual = DAO.Consulta(usuario.Id);
                    if (usuarioAtual != null)
                    {
                        // Lógica da Senha: Se o campo veio vazio, mantém a senha antiga.
                        if (string.IsNullOrEmpty(usuario.Senha))
                        {
                            usuario.Senha = usuarioAtual.Senha;
                        }

                        string loggedInUserIdStr = HttpContext.Session.GetString("IdUsuario");
                        string loggedInUserIsAdminSessionStr = HttpContext.Session.GetString("IsAdmin");
                        bool editorIsAdmin = loggedInUserIsAdminSessionStr == "True";
                        int.TryParse(loggedInUserIdStr, out int loggedInUserId);
                        bool isAdminFieldPresentInForm = Request.Form.ContainsKey(nameof(UsuarioViewModel.IsAdmin));

                        if (editorIsAdmin)
                        {
                            if (usuario.Id == loggedInUserId)
                            {
                                usuario.IsAdmin = usuarioAtual.IsAdmin;

                                if (usuarioAtual.IsAdmin && !usuario.IsAdmin && Request.Form.ContainsKey(nameof(UsuarioViewModel.IsAdmin)))
                                {
                                    usuario.IsAdmin = true;
                                    ModelState.AddModelError("IsAdmin", "Você não pode remover seu próprio status de administrador."); // Opcional
                                }
                            }
                            else
                            {
                                // Admin está editando outro usuário.
                                if (usuarioAtual.IsAdmin && !usuario.IsAdmin)
                                {
                                    var todosAdmins = ((UsuarioDAO)DAO).Listagem().Where(u => u.IsAdmin).ToList();
                                    // Se o usuário atual é o único admin na lista (ou o único que está sendo desmarcado)
                                    if (todosAdmins.Count(a => a.Id != usuario.Id || a.IsAdmin) <= 0 && todosAdmins.Any(a => a.Id == usuario.Id))
                                    {
                                        ModelState.AddModelError("IsAdmin", "Não é possível remover o status de administrador do último administrador do sistema.");
                                        usuario.IsAdmin = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Se quem edita não é admin, o status IsAdmin não pode ser alterado.
                            usuario.IsAdmin = usuarioAtual.IsAdmin;
                        }

                        // Lógica da Imagem: Se uma nova imagem não foi enviada E não foi solicitado para remover,
                        // mantém a imagem antiga.
                        if (usuario.Imagem == null && !usuario.RemoverImagemAtual)
                        {
                            usuario.ImagemEmByte = usuarioAtual.ImagemEmByte;
                        }
                    }
                }

                if (usuario.RemoverImagemAtual)
                {
                    usuario.ImagemEmByte = null;
                }
                else if (usuario.Imagem != null) // Nova imagem foi enviada
                {
                    usuario.ImagemEmByte = ConvertImageToByte(usuario.Imagem);
                }
            }
        }


        /// <summary>
        /// Prepara uma lista de gêneros (sexos) para ser usada em um controle de dropdown (combo box) na View.
        /// Este método privado busca todos os registros de sexo do banco de dados através do <see cref="SexoDAO"/>,
        /// formata-os como uma lista de <see cref="SelectListItem"/> e os armazena na <see cref="ViewBag"/>
        /// sob a chave "Sexos" para uso no formulário de usuário.
        /// </summary>
        /// <remarks>
        /// O processo envolve:
        /// 1. Instanciar <see cref="SexoDAO"/> para acessar os dados.
        /// 2. Chamar o método <see cref="DAO.PadraoDAO{T}.Listagem"/> para obter a lista de <see cref="SexoViewModel"/>.
        /// 3. Criar uma <see cref="List{T}"/> de <see cref="SelectListItem"/>.
        /// 4. Adicionar um item padrão/placeholder à lista com o texto "Selecione seu gênero..." e valor "0".
        /// 5. Iterar sobre os sexos retornados pelo DAO, convertendo cada um em um <see cref="SelectListItem"/>
        ///    (usando <see cref="SexoViewModel.Nome"/> para o texto e <see cref="SexoViewModel.Id"/> para o valor).
        /// 6. Atribuir a lista final à <c>ViewBag.Sexos</c>.
        /// Este método é chamado, por exemplo, em <see cref="UsuarioController.PreencheDadosParaView"/> para
        /// garantir que o dropdown de sexo esteja disponível no formulário de criação/edição de usuário.
        /// </remarks>
        private void PreparaListaSexosParaCombo()
        {
            var sexoDAO = new SexoDAO();
            var sexos = sexoDAO.Listagem();
            List<SelectListItem> listaSexos = new List<SelectListItem>();
            listaSexos.Add(new SelectListItem("Selecione seu gênero...", "0"));
            foreach (var sexo in sexos)
            {
                SelectListItem item = new SelectListItem(sexo.Nome, sexo.Id.ToString());
                listaSexos.Add(item);
            }
            ViewBag.Sexos = listaSexos;
        }


        /// <summary>
        /// Prepara os dados necessários para a view de formulário do usuário,
        /// estendendo a lógica da classe base <see cref="PadraoController{T}"/>.
        /// Este método é chamado antes de exibir a view de criação ("I") ou edição ("A") de um usuário.
        /// </summary>
        /// <param name="Operacao">Uma string que indica a operação atual: "I" para Inserção ou "A" para Atualização.</param>
        /// <param name="model">O objeto <see cref="UsuarioViewModel"/> que será preparado
        /// e, posteriormente, enviado para a View.</param>
        /// <remarks>
        /// A lógica deste método é a seguinte:
        /// 1. Chama a implementação base <c>base.PreencheDadosParaView(Operacao, model)</c> para executar
        ///    a lógica padrão, como a geração de um novo ID se <c>GeraProximoId</c> for verdadeiro.
        /// 2. Se a operação for de Inserção ("I"), define a <see cref="UsuarioViewModel.DataNascimento"/>
        ///    do modelo como a data e hora atuais como um valor padrão inicial.
        /// 3. Chama o método auxiliar <see cref="PreparaListaSexosParaCombo"/> para buscar a lista de sexos
        ///    do banco de dados e prepará-la para ser usada em um dropdown na view, armazenando-a
        ///    em <c>ViewBag.Sexos</c>.
        /// </remarks>
        protected override void PreencheDadosParaView(string Operacao, UsuarioViewModel model)
        {
            base.PreencheDadosParaView(Operacao, model);
            if (Operacao == "I")
                model.DataNascimento = DateTime.Now;

            PreparaListaSexosParaCombo();
        }


        /// <summary>
        /// Converte um arquivo de imagem (<see cref="IFormFile"/>) recebido de um formulário
        /// em um array de bytes (<c>byte[]</c>).
        /// Este método é um utilitário para processar uploads de arquivos, permitindo que o
        /// conteúdo do arquivo seja facilmente armazenado em um banco de dados (por exemplo,
        /// em um campo do tipo <c>varbinary</c> ou <c>image</c>).
        /// </summary>
        /// <param name="file">O arquivo (<see cref="IFormFile"/>) enviado através da requisição HTTP.
        /// Representa a imagem a ser convertida.</param>
        /// <returns>
        /// Um array de bytes (<c>byte[]</c>) contendo os dados do arquivo se o <paramref name="file"/>
        /// não for nulo.
        /// Retorna <c>null</c> se o <paramref name="file"/> de entrada for nulo (ou seja, se nenhum
        /// arquivo foi enviado).
        /// </returns>
        /// <remarks>
        /// Este método é chamado em <see cref="UsuarioController.ValidaDados"/> para processar a
        /// imagem de perfil do usuário antes de salvá-la no banco de dados.
        /// </remarks>
        public byte[] ConvertImageToByte(IFormFile file)
        {
            if (file != null)
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    return ms.ToArray();
                }
            else
                return null;
        }


        /// <summary>
        /// Sobrescreve o método Edit da classe base para preparar e exibir o formulário de edição de um usuário,
        /// adicionando lógicas específicas de permissão e contexto.
        /// </summary>
        /// <param name="id">O identificador único do usuário a ser editado.</param>
        /// <returns>
        /// - <see cref="ViewResult"/> que renderiza a view do formulário (<see cref="PadraoController{T}.NomeViewForm"/>)
        ///   com o modelo <see cref="UsuarioViewModel"/> preenchido e com dados adicionais na <see cref="ViewBag"/>,
        ///   se o usuário for encontrado.
        /// - <see cref="RedirectToActionResult"/> para a <see cref="PadraoController{T}.NomeViewIndex"/> (a página de listagem)
        ///   se o usuário com o <paramref name="id"/> fornecido não for encontrado.
        /// </returns>
        /// <remarks>
        /// Este método estende a funcionalidade base <see cref="PadraoController{T}.Edit(int)"/> da seguinte forma:
        /// 1. Busca os dados do usuário a ser editado. Se não encontrado, redireciona para a listagem.
        /// 2. Obtém o ID e o status de administrador do usuário atualmente logado a partir da sessão.
        /// 3. Define <c>ViewBag.UsuarioLogadoEAdmin</c> como <c>true</c> ou <c>false</c>. A view pode usar isso para
        ///    renderizar condicionalmente campos que apenas administradores podem ver/editar (como o status 'IsAdmin' de outro usuário).
        /// 4. Define <c>ViewBag.EditandoProprioPerfil</c> como <c>true</c> se o ID do usuário logado for o mesmo do usuário sendo editado.
        ///    Isso pode ser usado na view para aplicar regras específicas, como impedir que um usuário altere seu próprio status de admin.
        /// 5. Captura o parâmetro 'source' da query string da URL (ex: "?source=consultaAvancada"). Se presente, armazena em <c>ViewBag.Source</c>.
        ///    Isso é usado no método <see cref="GetSaveRedirectAction"/> para redirecionar o usuário de volta para a página de origem após salvar.
        /// 6. Chama <see cref="PreencheDadosParaView"/> para popular dados necessários, como a lista de sexos para o dropdown.
        /// </remarks>
        public override IActionResult Edit(int id)
        {
            ViewBag.Operacao = "A";
            var model = DAO.Consulta(id);

            if (model == null)
                return RedirectToAction(NomeViewIndex);

            string loggedInUserIdStr = HttpContext.Session.GetString("IdUsuario");
            int.TryParse(loggedInUserIdStr, out int loggedInUserId);
            string loggedInUserIsAdminSessionStr = HttpContext.Session.GetString("IsAdmin");
            bool editorLogadoEAdmin = loggedInUserIsAdminSessionStr == "True";

            ViewBag.UsuarioLogadoEAdmin = editorLogadoEAdmin;
            ViewBag.EditandoProprioPerfil = (model.Id == loggedInUserId);

            // Captura o parâmetro 'source' da query string
            if (Request.Query.ContainsKey("source"))
            {
                ViewBag.Source = Request.Query["source"].ToString();
            }

            PreencheDadosParaView("A", model);
            return View(NomeViewForm, model);
        }


        /// <summary>
        /// Sobrescreve o método Save da classe base para lidar com a persistência (Inserção ou Atualização)
        /// de um <see cref="UsuarioViewModel"/>, adicionando um fluxo de redirecionamento dinâmico e
        /// um tratamento de erro mais robusto.
        /// </summary>
        /// <param name="model">O objeto <see cref="UsuarioViewModel"/> contendo os dados do formulário a serem salvos.</param>
        /// <param name="Operacao">Uma string que indica o tipo de operação: "I" para Inserção ou "A" para Atualização.</param>
        /// <returns>
        /// - <see cref="RedirectToActionResult"/> para um destino determinado por <see cref="GetSaveRedirectAction"/>
        ///   em caso de sucesso na operação de salvamento.
        /// - <see cref="ViewResult"/> renderizando o formulário (<see cref="PadraoController{T}.NomeViewForm"/>) com mensagens de erro
        ///   no <see cref="ModelState"/>, caso a validação falhe ou ocorra uma exceção durante o salvamento.
        /// </returns>
        /// <remarks>
        /// Este método é protegido contra ataques CSRF com o atributo <see cref="ValidateAntiForgeryTokenAttribute"/>.
        /// A lógica estendida em relação ao método base <see cref="PadraoController{T}.Save"/> inclui:
        /// 1. Recuperação do parâmetro 'Source' do formulário. Este valor, originado na Action <see cref="Edit(int)"/>,
        ///    é usado para determinar a página para a qual o usuário será redirecionado após o salvamento.
        /// 2. Uma verificação de segurança adicional para garantir que um novo ID seja gerado em operações de Inserção ("I"),
        ///    caso a geração automática esteja habilitada e o ID ainda seja 0.
        /// 3. Chamada ao método <see cref="ValidaDados(UsuarioViewModel, string)"/>, que contém as regras de negócio
        ///    específicas para a validação de um usuário.
        /// 4. Se a validação falhar, o formulário é reexibido com os dados inseridos e os erros, preservando o 'Source' na <see cref="ViewBag"/>.
        /// 5. Se a validação for bem-sucedida, o método <see cref="GetSaveRedirectAction(string, string)"/> é chamado
        ///    com o 'Source' para determinar o redirecionamento correto.
        /// 6. O bloco `catch` foi aprimorado para, em vez de redirecionar para uma página de erro genérica,
        ///    reexibir o formulário com os dados do usuário e uma mensagem de erro clara no <see cref="ModelState"/>,
        ///    proporcionando uma experiência de usuário melhor.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override IActionResult Save(UsuarioViewModel model, string Operacao)
        {
            string source = Request.Form["Source"];

            try
            {
                if (Operacao == "I" && GeraProximoId && model.Id == 0) // Segurança extra caso o ID não tenha sido gerado
                {
                    model.Id = DAO.ProximoId();
                }

                ValidaDados(model, Operacao);
                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = Operacao;
                    ViewBag.Source = source;
                    PreencheDadosParaView(Operacao, model);
                    return View(NomeViewForm, model);
                }
                else
                {
                    if (Operacao == "I")
                        DAO.Insert(model);
                    else
                        DAO.Update(model);

                    return GetSaveRedirectAction(Operacao, source);
                }
            }
            catch (Exception erro)
            {
                ViewBag.Operacao = Operacao;
                ViewBag.Source = source;
                PreencheDadosParaView(Operacao, model);
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as alterações: " + erro.Message);
                return View(NomeViewForm, model);
            }
        }


        /// <summary>
        /// Determina a ação de redirecionamento apropriada após uma operação de salvamento
        /// (Inserção ou Atualização) bem-sucedida, com base no tipo de operação e na página de origem.
        /// Este método é específico para o <see cref="UsuarioController"/> e lida com os diferentes fluxos
        /// de navegação do usuário (novo cadastro, edição de perfil, edição por administrador).
        /// </summary>
        /// <param name="operacao">Uma string que indica o tipo de operação: "I" para Inserção ou "A" para Atualização.</param>
        /// <param name="source">Uma string opcional que indica a origem da requisição (ex: "profile").
        /// Este valor é usado para determinar o destino do redirecionamento em operações de atualização.</param>
        /// <returns>
        /// Um <see cref="RedirectToActionResult"/> para a página apropriada.
        /// </returns>
        /// <remarks>
        /// A lógica de redirecionamento é a seguinte:
        /// - Se a <paramref name="operacao"/> for Inserção ("I"), o usuário é sempre redirecionado para a página de Login
        ///   (<c>RedirectToAction("Index", "Login")</c>). Isso é típico para um formulário de cadastro público.
        /// - Se a <paramref name="operacao"/> for Atualização ("A"):
        ///   - Se o <paramref name="source"/> for "profile" (case-insensitive), significa que o usuário estava
        ///     editando seu próprio perfil. Ele é redirecionado para a Home (<c>RedirectToAction("Index", "Home")</c>)
        ///     com uma mensagem de sucesso em <c>TempData["MensagemSucesso"]</c>.
        ///   - Caso contrário (se o <paramref name="source"/> for diferente de "profile" ou nulo), assume-se que um administrador
        ///     estava editando um usuário. O administrador é redirecionado de volta para a página de consulta avançada
        ///     (<c>RedirectToAction("ConsultaAvancada", "Admin")</c>) com uma mensagem de sucesso específica em
        ///     <c>TempData["MensagemSucessoAdmin"]</c>.
        /// - Se a <paramref name="operacao"/> não for nem "I" nem "A", o método retorna para a página de listagem padrão
        ///   definida em <c>NomeViewIndex</c> (comportamento de fallback).
        /// </remarks>
        protected IActionResult GetSaveRedirectAction(string operacao, string source = null)
        {
            if (operacao == "I")
            {
                return RedirectToAction("Index", "Login");
            }
            else if (operacao == "A")
            {
                if ("profile".Equals(source, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["MensagemSucessoAdmin"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction("ConsultaAvancada", "Admin");
                }
            }
            return RedirectToAction(NomeViewIndex);
        }
    }
}
