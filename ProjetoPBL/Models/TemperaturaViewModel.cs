using System;

namespace ProjetoPBL.Models
{
    public class TemperaturaViewModel
    {
        public int Id { get; set; }
        public DateTime RecvTime { get; set; }
        public float Temperature { get; set; }
        public string SensorId { get; set; }

    }
}
