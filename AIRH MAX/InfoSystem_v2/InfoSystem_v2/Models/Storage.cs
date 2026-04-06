namespace InfoSystem_v2.Models
{
    public class Storage
    {
        public string EspacioLibre { get; set; }
        public string Formato { get; set; }
        public string TamañoTotal { get; set; }
        public string EspacioUsado { get; set; }
        public bool Principal { get; set; }
        public string Unidad { get; set; }
        public string Fijado { get; set; }
        public string Tipo { get; set; }
        public string Etiqueta { get; set; }
        public string OSRoot { get; set; }
        public string Modelo { get; set; }
        public float? Temperatura { get; set; }
    }
}
