using System;

namespace ProjetoPBL.Models
{

    /// <summary>
    /// ViewModel com os dados principais de um sensor.
    /// Utilizada nas operações de cadastro e consulta.
    /// </summary>
    
    public class SensorViewModel : PadraoViewModel
    {
        public string nomeSensor { get; set; }
        public string descricaoSensor { get; set; }
        public string localInstalacao { get; set; }
        public decimal valorInstalacao { get; set; }
        public DateTime dataInstalacao { get; set; }

    }
}


