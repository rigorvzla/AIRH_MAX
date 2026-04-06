namespace MAX.Models
{
    public class File_Config
    {
        public bool Windows { get; set; }
        public bool Minimizado { get; set; }
        public bool Experimental { get; set; }
        public string Usuario { get; set; }
        public string Asistente { get; set; }
        public string Despedida { get; set; }
        public string Voz { get; set; }
        public MicrophoneInfo Microfono { get; set; }
        public string Entrada_Audio { get; set; }
        public string Idioma { get; set; }
        public double Opacidad { get; set; }
        public int Entendimiento { get; set; }
        public bool PrimerInicio { get; set; }
        public string Dir_Pantalla_Capturas { get; set; }
        public string Dir_Notas { get; set; }
        public string Dir_Musica { get; set; }
        public string Dir_Videos { get; set; }
        public string Dir_Imagenes { get; set; }
        public string ID_Telegram { get; set; }
        public bool Telegram_Check { get; set; }
        public bool VozIA { get; set; }
        public string Lenguaje { get; set; }
        public string WebCam { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
        public string AbreviacionPais { get; set; }
        public string Raiz { get; set; }
    }

    public class MicrophoneInfo
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public int Channels { get; set; }
        public bool IsDefault { get; set; }

        public override string ToString()
        {
            var defaultMarker = IsDefault ? " (Predeterminado)" : "";
            return $"[{DeviceId}] {Name} - {Channels} canal(es){defaultMarker}";
        }
    }
}
