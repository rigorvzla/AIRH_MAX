namespace InfoSystem_v2.Models
{
    public class NET
    {
        public string TipoInterface { get; set; }
        public string Dispositivo { get; set; }
        public string Estado { get; set; }
        public string NombreRed { get; set; }
        public float? CargaPorcentual { get; set; }
        public float? DatosDescargados { get; set; }
        public float? DatosSubidos { get; set; }
        public float? VelocidadSubida { get; set; }
        public float? VelocidadDescarga { get; set; }
        public string VelocidadDispositivo { get; set; }
        public string MAC { get; set; }
        public string IP { get; set; }
    }
}
