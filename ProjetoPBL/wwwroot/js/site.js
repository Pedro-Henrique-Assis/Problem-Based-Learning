function aplicaFiltroConsultaAvancada() {
    var vNome = document.getElementById('nome').value;
    var vEstado = document.getElementById('estado').value;
    var vSexo = document.getElementById('sexoId').value
    var vDataInicial = document.getElementById('dataInicial').value;
    var vDataFinal = document.getElementById('dataFinal').value;
    $.ajax({
        url: "/jogo/ObtemDadosConsultaAvancada",
        data: { Nome: vNome, Estado: vEstado, sexoId: vSexo, dataInicial: vDataInicial, dataFinal: vDataFinal },
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
