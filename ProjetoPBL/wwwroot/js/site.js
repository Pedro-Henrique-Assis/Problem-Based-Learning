function aplicaFiltroConsultaAvancada() {
    var vNome = document.getElementById('nome').value;
    var vEstado = document.getElementById('estado').value;
    var vSexo = document.getElementById('sexoId').value
    var vDataInicial = document.getElementById('dataInicial').value;
    var vDataFinal = document.getElementById('dataFinal').value;
    $.ajax({
        url: "/Admin/ObtemDadosConsultaAvancada",
        data: { nome: vNome, estado: vEstado, sexoId: vSexo, dataInicial: vDataInicial, dataFinal: vDataFinal },
        success: function (dados) {
            if (dados.erro != undefined) {
                alert(dados.msg);
            }
            else {
                document.getElementById('resultadoConsulta').innerHTML = dados;
            }
        },
    });
}

function confirmarExclusaoUsuario(usuarioId, usuarioNome) {
    if (confirm(`Tem certeza que deseja excluir o usuário "${usuarioNome}" (ID: ${usuarioId})?`)) {
        excluirUsuarioAjax(usuarioId);
    }
}

function excluirUsuarioAjax(usuarioId) {
    // É importante obter o token de antifalsificação se você o estiver usando no seu controller.
    // Se sua action [HttpPost] Delete tem [ValidateAntiForgeryToken], você precisará enviar o token.
    // Uma forma é ter um input hidden no seu form principal da página ConsultaAvancada
    // ou obter dinamicamente. Para simplificar, assumirei que você pode precisar adicioná-lo.
    // Ex: var token = $('input[name="__RequestVerificationToken"]').val();
    //     data: { id: usuarioId, __RequestVerificationToken: token },

    $.ajax({
        url: "/Admin/Delete", // A URL da sua action de exclusão
        type: "POST",
        data: { id: usuarioId },
        success: function (response) {
            var divMensagens = $('#divMensagensAdmin');
            divMensagens.html(''); // Limpa mensagens anteriores

            // Se o controller retornar um JSON simples para indicar sucesso/erro:
            if (response.sucesso) {
                // Remove a linha da tabela
                $("#usuario-row-" + usuarioId).remove();

                if (response.mensagem) {
                    divMensagens.html('<div class="alert alert-success" role="alert">' + response.mensagem + '</div>');
                }
            } else {
                if (response.mensagem) {
                    divMensagens.html('<div class="alert alert-danger" role="alert">' + response.mensagem + '</div>');
                }
            }
        },
        error: function (xhr, status, error) {
            console.error("Erro na exclusão: ", status, error);
            alert("Ocorreu um erro ao tentar excluir o usuário.");
        }
    });
}
