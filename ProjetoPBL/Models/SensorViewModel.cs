using System;

namespace ProjetoPBL.Models
{
    public class SensorViewModel : PadraoViewModel
    {
        public string nomeSensor { get; set; }
        public string descricaoSensor { get; set; }
        public string localInstalacao { get; set; }
        public decimal valorInstalacao { get; set; }
        public DateTime dataInstalacao { get; set; }

    }
}
