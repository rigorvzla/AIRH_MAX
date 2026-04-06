namespace AIRH_MAX.ClassView.ViewModel
{
    [System.Reflection.Obfuscation(Feature = "all", Exclude = true)]
    internal class FileWatch
    {
        public string Ruta { get; set; }
        public bool Creado { get; set; }
        public bool Cambiado { get; set; }
        public bool Renombrado { get; set; }
        public bool Eliminado { get; set; }
    }
}
