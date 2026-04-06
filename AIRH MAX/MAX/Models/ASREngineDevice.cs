namespace MAX.Models
{
    public class ASREngineDevice
    {
        /// <summary>
        /// Identificador del dispositivo de audio.
        /// </summary>
        public int DeviceId { get; set; }
        /// <summary>
        /// Nombre del dispositivo de audio.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Número máximo de canales de salida.
        /// </summary>
        public int MaxInputChannels { get; set; }
        /// <summary>
        /// Baja latencia de entrada por defecto.
        /// </summary>
        public double DefaultLowInputLatency { get; set; }
        /// <summary>
        /// Alta latencia de entrada por defecto.
        /// </summary>
        public double DefaultHighInputLatency { get; set; }
        /// <summary>
        /// Frecuencia de muestreo por defecto.
        /// </summary>
        public double DefaultSampleRate { get; set; }
        /// <summary>
        /// MME, WASAPI, DirectSound etc.
        /// </summary>
        public string HostApi { get; set; }
        /// <summary>
        /// Indica si el dispositivo está seleccionado por defecto.
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// Indica si el dispositivo es de entrada (micrófono) o salida (altavoces).
        /// </summary>
        public bool IsInput { get; set; }
    }
}
