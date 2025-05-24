namespace ProjetoPBL.Models
{
    /// <summary>
    /// Representa uma leitura de temperatura do sensor
    /// Utiliza struct por ser uma estrutura leve, imutável e usada apenas para leitura
    /// </summary>
    public readonly struct LeituraViewModel
    {
        /// <summary>
        /// Temperatura lida pelo sensor
        /// </summary>
        public float Temperatura { get; }

        /// <summary>
        /// Data e hora da leitura
        /// </summary>
        public DateTime Data { get; }

        /// <summary>
        /// Construtor que define a leitura de temperatura e sua data
        /// </summary>
        /// <param name="temperatura">Valor lido pelo sensor</param>
        /// <param name="data">Momento da leitura</param>
        public LeituraViewModel(float temperatura, DateTime data)
        {
            Temperatura = temperatura;
            Data = data;
        }
    }
}
