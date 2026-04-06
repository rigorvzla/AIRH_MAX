using MAX.Models;
using Newtonsoft.Json;
using PipeSharp;
using System.IO;
using System.Speech.Synthesis;

namespace MAX.ClassView
{
    internal class Engrane
    {
        public static SpeechSynthesizer AIRH_Voz = new SpeechSynthesizer();
        public static PiperService _piper = new();
        public static string user => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "user.dat"));

        public static string Configuraciones_CFG() =>
           Path.Combine(Environment.CurrentDirectory, "Perfiles", user.ToLower(), "CFG");

        public static File_Config File_Config()
        {
            string json = File.ReadAllText(Path.Combine(Configuraciones_CFG(), "Config.cfg"));
            File_Config fc = JsonConvert.DeserializeObject<File_Config>(json);
            return fc;
        }

        public static File_Theme File_Theme()
        {
            string json = File.ReadAllText(Path.Combine(Configuraciones_CFG(), "Theme.cfg"));
            File_Theme fc = JsonConvert.DeserializeObject<File_Theme>(json);
            return fc;
        }
    }
}
