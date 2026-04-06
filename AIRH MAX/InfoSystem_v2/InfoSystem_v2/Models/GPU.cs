namespace InfoSystem_v2.Models
{
    public class GPU
    {
        public bool Fisico { get; set; }
        public string Nombre { get; set; }
        public string Caption { get; set; }
        public string NameAlter { get; set; }
        public string Detalle { get; set; }
        public string MemoriaIntegrada { get; set; }
        public float? Temperatura { get; set; }
        public float? ClockCore { get; set; }
        public float? ClockMemory { get; set; }
        public float? MemoriaTotal { get; set; }
        public float? MemoriaLibre { get; set; }
        public float? MemoriaUsada { get; set; }
    }
}
