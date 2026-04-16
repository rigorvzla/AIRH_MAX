using AIRH_MAX.ClassView.ViewModel;
using NAudio.Wave;
using Newtonsoft.Json;
using PipeSharp;
using Shell32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;

namespace AIRH_MAX.ClassView
{
    class Engrane
    {
        public static SpeechSynthesizer AIRH_Voz = new();
        public static PiperService _piper = new();

        public static NotifyIcon ni = new NotifyIcon();
        public static BasicAudio.Recording audioRecorder = new BasicAudio.Recording();
        public static WasapiLoopbackCapture capture;
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static bool Proceso;
        public static Chat_Item Model_IA;
        public static bool escucha;
        public static List<string> Wizard = new List<string>();

        public static string user => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "user.dat"));

        public const string API_BASE_URL = "";
        public const string API_KEY = "";

        public static int PosY;
        public static int PosX;

        public static string novedades = @"Novedades:  
- Mejorado el modulo OCR
- Reparado modulo de voz
- Agregada voces IA";

        public static File_IA File_IA()
        {
            string json = File.ReadAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(user),"IA.cfg"));
            File_IA fc = JsonConvert.DeserializeObject<File_IA>(json);
            return fc;
        }

        public static File_Config File_Config() 
        {
            string json = File.ReadAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(user), "Config.cfg"));
            File_Config fc = JsonConvert.DeserializeObject<File_Config>(json);
            return fc;
        }

        public static File_Theme File_Theme()
        {
            string json = File.ReadAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(user), "Theme.cfg"));
            File_Theme fc = JsonConvert.DeserializeObject<File_Theme>(json);
            return fc;
        }

        public static Mail_Config Mail_Config(string Tipo)
        {
            string json = File.ReadAllText(Path.Combine(RutasAbsolutasCFG.Email_CFG(user), $"{Tipo}.cfg"));
            Mail_Config fc = JsonConvert.DeserializeObject<Mail_Config>(json);
            return fc;
        }

        public static async Task MP3_Player(string Ruta)
        {
            var reader = new Mp3FileReader(Ruta);
            var waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();

            await Task.Run(() =>
            {
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
            });

            reader.Dispose();
            waveOut.Dispose();
        }

        public static void EXE(string ruta, string arg="")
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = ruta;
                process.StartInfo.Arguments = arg;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception a)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => Xceed.Wpf.Toolkit.MessageBox.Show(a.Message, "AV-AIRH MAX", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            }
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static string ElementPath()
        {
            string filename;
            string sourceName = string.Empty;
            IntPtr handle = GetForegroundWindow();
            foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
            {

                if (window.HWND == (int)handle)
                {
                    filename = System.IO.Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                    if (filename.ToLowerInvariant() == "explorer")
                    {
                        FolderItems items = ((IShellFolderViewDual2)window.Document).SelectedItems();
                        foreach (FolderItem item in items)
                        {
                            sourceName = item.Path;
                        }
                    }
                }
            }
            return sourceName;
        }           
        
    }
}
