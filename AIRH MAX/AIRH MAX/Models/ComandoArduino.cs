namespace AIRH_MAX.Models
{
    public class ComandoArduino
    {
        public string Puerto { get; set; }
        public string Baudrate { get; set; }
        public string Comando { get; set; } // El comando que se enviará por serial
        public string Accion { get; set; }
        public string Respuesta { get; set; }
    }
}
