namespace InfoSystem_v2.Models
{
    public class DeviceMonitor
    {
        public class CPU
        {
            public double CargaGeneral { get; set; }
            public double TemperaturaGeneral { get; set; }
            public double MHzUsado { get; set; }
            public double MHzTotal { get; set; }
            public Dictionary<string, float?> Temperaturas_Nucleos { get; set; } = new Dictionary<string, float?>();
            public Dictionary<string, float?> Hilos_Carga { get; set; } = new Dictionary<string, float?>();
        }

        public class RAM
        {
            public double Total { get; set; }
            public double Actual { get; set; }
            public double Dispobible { get; set; }
            public double Porcentaje { get; set; }
        }

        public class RED
        {
            public string TipoInterface { get; set; }
            public string Dispositivo { get; set; }
            public string Estado { get; set; }
            public string NombreRed { get; set; }
            public string MAC { get; set; }
            public float? CargaPorcentual { get; set; }
            public float? DatosDescargados { get; set; }
            public float? DatosSubidos { get; set; }
            public float? VelocidadSubida { get; set; }
            public float? VelocidadDescarga { get; set; }
            public string VelocidadDispositivo { get; set; }
            public string IP {  get; set; }
        }
    }
}
