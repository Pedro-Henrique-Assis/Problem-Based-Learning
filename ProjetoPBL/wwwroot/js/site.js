function aplicaFiltroConsultaAvancada() {
    var vNome = document.getElementById('nome').value;
    var vEstado = document.getElementById('estado').value;
    var vSexo = document.getElementById('sexoId').value
    var vDataInicial = document.getElementById('dataInicial').value;
    var vDataFinal = document.getElementById('dataFinal').value;
    var vLogin = document.getElementById('login').value
    $.ajax({
        url: "/Admin/ObtemDadosConsultaAvancada",
        data: { nome: vNome, estado: vEstado, sexoId: vSexo, dataInicial: vDataInicial, dataFinal: vDataFinal, login: vLogin },
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
    Swal.fire({
        title: 'Tem certeza?',
        text: `Você deseja excluir o usuário "${usuarioNome}" (ID: ${usuarioId})? Esta ação não poderá ser revertida!`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sim, excluir!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            excluirUsuarioAjax(usuarioId);
        }
    });
}

function excluirUsuarioAjax(usuarioId) {
    $.ajax({
        url: "/Admin/Delete", // A URL da sua action de exclusão
        type: "POST",
        data: { id: usuarioId },
        success: function (response) {
            var divMensagens = $('#divMensagensAdmin'); // Make sure you have this div in your ConsultaAvancada.cshtml
            divMensagens.html(''); // Limpa mensagens anteriores

            if (response.sucesso) {
                // Remove a linha da tabela
                $("#usuario-row-" + usuarioId).remove();
                Swal.fire(
                    'Excluído!',
                    response.mensagem || 'O usuário foi excluído com sucesso.',
                    'success'
                );
            } else {
                Swal.fire(
                    'Erro!',
                    response.mensagem || 'Ocorreu um erro ao tentar excluir o usuário.',
                    'error'
                );
            }
        },
        error: function (xhr, status, error) {
            console.error("Erro na exclusão: ", status, error);
            Swal.fire(
                'Erro!',
                'Ocorreu um erro na comunicação ao tentar excluir o usuário.',
                'error'
            );
        }
    });
}