function aplicaFiltroSensor() {
    var local = document.getElementById('local').value;
    var valorMin = document.getElementById('valorMin').value;
    var valorMax = document.getElementById('valorMax').value;
    var dataInicial = document.getElementById('dataInicial').value;
    var dataFinal = document.getElementById('dataFinal').value;

    $.ajax({
        url: "/ConsultaSensor/ObtemDadosConsulta",
        data: {
            local: local,
            valorMin: valorMin,
            valorMax: valorMax,
            dataInicial: dataInicial,
            dataFinal: dataFinal
        },
        success: function (dados) {
            if (dados.erro !== undefined && dados.erro === true) {
                alert(dados.msg);
            } else {
                document.getElementById('resultadoConsulta').innerHTML = dados;
            }
        },
        error: function () {
            alert("Erro ao carregar os dados.");
        }
    });
}
