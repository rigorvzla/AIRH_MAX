namespace InfoSystem_v2.Models
{
    public class CPU
    {
        public string Modelo { get; set; }
        public Dictionary<string, float?> Temperatura { get; set; } = new Dictionary<string, float?>();
        public Dictionary<string, float?> Hilos_Carga { get; set; } = new Dictionary<string, float?>();
        public float? CargaGeneral { get; set; }
        public string Velocidad_Maxima { get; set; }
        public string Velocidad_Actual { get; set; }
        public string Familia { get; set; }
        public string Socket { get; set; }
        public string Nucleos { get; set; }
        public string Hilos { get; set; }
    }
}
