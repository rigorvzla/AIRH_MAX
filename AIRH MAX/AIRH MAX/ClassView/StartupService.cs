// Services/StartupService.cs
using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.IA;
using AIRH_MAX.ClassView.Services;
using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Popups;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace AIRH_MAX.Services
{
    public interface IStartupProgress
    {
        void UpdateProgress(int percentage, string message);
    }

    public class StartupService
    {

        bool _vozIA;
        private readonly IStartupProgress _progress;
        private string _salidaVoz = string.Empty;

        public StartupService(IStartupProgress progress)
        {
            _progress = progress;
            Engrane._piper.StatusChanged += OnStatusChanged;
            Engrane._piper.ErrorOccurred += OnErrorOccurred;
        }

        private void OnErrorOccurred(string obj)
        {
            _progress.UpdateProgress(35, obj);
        }

        private void OnStatusChanged(string obj)
        {
            _progress.UpdateProgress(35, obj);
        }

        public async Task InitializeAsync()
        {
            // 4. Archivos de configuración
            _progress.UpdateProgress(25, "Cargando: Archivos de Configuración");
            ArchivosCFG();
            //await Task.Delay(500);

            // 1. Reconocedor de Voz
            _progress.UpdateProgress(35, "Cargando: Reconocedor de Voz");
            await VoskPlataform.voskManager.StartAutomaticAsync();

            // 2. Voces instaladas
            _progress.UpdateProgress(40, "Cargando: Modulo de voz");
            await CargaVoz();
            _progress.UpdateProgress(45, "Modulo de voz: Activado");

            _progress.UpdateProgress(50, "Cargando: Servidor Android");
            await SignalServer.StartSignalRServerAsync();
            await Task.Delay(2000);
            _progress.UpdateProgress(55, "Servidor Android: Activado");

            // 3. Dispositivos de audio
            _progress.UpdateProgress(60, "Cargando: Micrófono\nDispositivos de audio");
            await Task.Delay(500);

            // 5. Modelo LLM
            _progress.UpdateProgress(75, "Cargando modelo LLM...\n(puede tardar un poco)");
            string iaMessage = await CheckIA_LocalAsync();
            _progress.UpdateProgress(75, string.IsNullOrEmpty(iaMessage) ? "Modelo LLM cargado" : iaMessage);
            //await Task.Delay(2000);

            //if (Engrane.File_Config().Experimental)
            //{
            //    // Integración experimental (comentado como en original)
            //    _progress.UpdateProgress(100, "Carga Finalizada");
            //}
            //else
            //{
            _progress.UpdateProgress(85, "Carga Finalizada");
            //await Task.Delay(500);
            _progress.UpdateProgress(90, "Carga Finalizada");
            //await Task.Delay(500);
            _progress.UpdateProgress(100, "Carga Finalizada");
            //}
        }


        private async Task CargaVoz()
        {
            if (!File.Exists(Path.Combine(RutasAbsolutas.Configuraciones, "Config.cfg")))
            {
                try
                {
                    //atento a revision
                    if (Engrane.AIRH_Voz is null)
                    {
                        _salidaVoz = await Engrane._piper.ReinitializeAsync();
                        _vozIA = true;
                    }
                    else
                    {
                        _salidaVoz = Engrane.AIRH_Voz.GetInstalledVoices().First(vo => vo.VoiceInfo.Culture.Name.Contains("es")).VoiceInfo.Name;
                        _vozIA = false;
                    }

                }
                catch (Exception)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("No hay voces instaladas, el asistente no podrá hablar.", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Error);
                    _salidaVoz = string.Empty;
                    throw;
                }
            }
            else
            {
                if (Engrane.File_Config().VozIA)
                {
                    _salidaVoz = await Engrane._piper.ReinitializeAsync(Engrane.File_Config().Voz);
                }
                else
                {
                    _salidaVoz = Engrane.AIRH_Voz.GetInstalledVoices().First(vo => vo.VoiceInfo.Culture.Name.Contains("es")).VoiceInfo.Name;
                }
            }
        }

        private async Task<string> CheckIA_LocalAsync()
        {
            if (!File.Exists(Path.Combine(RutasAbsolutas.Configuraciones, "IA.cfg")))
                return string.Empty;

            if (Engrane.File_IA().Activar)
            {
                if (Engrane.File_IA().Compania.Equals("Local"))
                {
                    return await ServerPlataform.ServerLLM_StartAsync(
                        Engrane.File_IA().Apikey,
                        Engrane.File_IA().ModeloLocal,
                        ClassView.IA.Prompt.PromptCompleto_Personal(),
                        Engrane.File_IA().EndPoint,
                        Local: true);
                }
                else
                {
                    return await ServerPlataform.ServerLLM_StartAsync(
                        Engrane.File_IA().Apikey,
                        Engrane.File_IA().ModeloOnline,
                        ClassView.IA.Prompt.PromptCompleto_Personal(),
                        Engrane.File_IA().EndPoint,
                        Local: false);
                }
            }
            return string.Empty;
        }

        private void ArchivosCFG()
        {
            if (!Directory.Exists(RutasAbsolutas.Perfil))
            {
                WizardStart wizard = new WizardStart();
                wizard.ShowDialog();
                Directory.CreateDirectory(RutasAbsolutasCFG.Perfil_CFG(Engrane.Wizard[0]));
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "user.dat"), Engrane.Wizard[0]);

                // Config.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "Config.cfg")))
                {
                    Directory.CreateDirectory(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]));
                    var config = new File_Config
                    {
                        Usuario = Engrane.Wizard[0].ToLower(),
                        Asistente = Engrane.Wizard[1].ToLower(),
                        Despedida = Engrane.Wizard[2].ToLower(),
                        Dir_Notas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.Wizard[0]), "Notas de Voz"),
                        Dir_Pantalla_Capturas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.Wizard[0]), "Capturas de pantalla"),
                        Dir_Musica = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                        Dir_Videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        Dir_Imagenes = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        Raiz = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.Wizard[0])),
                        Opacidad = 1,
                        Experimental = false,
                        Entendimiento = 1500,
                        PrimerInicio = true,
                        Minimizado = false,
                        Windows = false,
                        Telegram_Check = false,
                        ID_Telegram = "000000000",
                        Provincia = "Caracas",
                        Lenguaje = "Español",
                        Pais = "Venezuela",
                        Voz = _salidaVoz,
                        Microfono = VoskPlataform.voskManager.DefaultMicrophone,
                        Entrada_Audio = Multimedia.DeviceReproduccion(),
                        VozIA = _vozIA
                    };

                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "Config.cfg"), JsonConvert.SerializeObject(config, Formatting.Indented));
                }

                // Theme.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "Theme.cfg")))
                {
                    var theme = new File_Theme
                    {
                        PrimaryColor = "#FF02010F",
                        SecondaryColor = "#FF303055",
                        PrimaryText = "#f0f8ff",
                        SecondaryText = "#87ceeb",
                        PrimaryAcsendText = "#00FF00",
                        SecondaryAcsendText = "#FF35FDE2",
                        ProgressColor = "#FEF200"
                    };
                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "Theme.cfg"), JsonConvert.SerializeObject(theme, Formatting.Indented));
                }

                // IA.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "IA.cfg")))
                {
                    var ia = new File_IA
                    {
                        Activar = true,
                        Apikey = "APIKEY",
                        Compania = "Local",
                        ModeloLocal = ServerPlataform.GetFirstModelName(),
                        ModeloOnline = "Online",
                        EndPoint = "URL"
                    };
                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.Wizard[0]), "IA.cfg"), JsonConvert.SerializeObject(ia, Formatting.Indented));
                }

                var subcarpetas = new[] { "Notas de Voz", "Capturas de pantalla", "PerfilGamer", "ITT", "Web", "Filmador" };
                foreach (var carpeta in subcarpetas)
                    Directory.CreateDirectory(Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.Wizard[0]), carpeta));
                Directory.CreateDirectory(RutasAbsolutasCFG.Email_CFG(Engrane.Wizard[0]));

                // Archivos de correo
                var mails = new[] { "Gmail", "Yahoo", "Hotmail", "Personal" };
                foreach (var nombre in mails)
                {
                    if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Email_CFG(Engrane.Wizard[0]), $"{nombre}.cfg")))
                    {
                        var mailConfig = new Mail_Config
                        {
                            Usuario = "Usuario",
                            Pass = "1234",
                            Servidor = "Servidor",
                            Puerto = "Puerto",
                            Activar = false,
                            SSL = false,
                            POP3 = false,
                            IMAP = false
                        };
                        File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Email_CFG(Engrane.Wizard[0]), $"{nombre}.cfg"), JsonConvert.SerializeObject(mailConfig, Formatting.Indented));
                    }
                }

                SyncMultimedia();
            }
            else
            {
                // Config.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.user), "Config.cfg")))
                {
                    Directory.CreateDirectory(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario));
                    var config = new File_Config
                    {
                        Usuario = Engrane.File_Config().Usuario.ToLower(),
                        Asistente = Engrane.File_Config().Asistente.ToLower(),
                        Despedida = Engrane.File_Config().Despedida.ToLower(),
                        Dir_Notas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario), "Notas de Voz"),
                        Dir_Pantalla_Capturas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario), "Capturas de pantalla"),
                        Dir_Musica = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                        Dir_Videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        Dir_Imagenes = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        Raiz = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario)),
                        Opacidad = 1,
                        Experimental = false,
                        Entendimiento = 1500,
                        PrimerInicio = true,
                        Minimizado = false,
                        Windows = false,
                        Telegram_Check = false,
                        ID_Telegram = "00000",
                        Provincia = "Caracas",
                        Lenguaje = "Español",
                        Pais = "Venezuela",
                        Voz = _salidaVoz,
                        Microfono = VoskPlataform.voskManager.DefaultMicrophone,
                        Entrada_Audio = Multimedia.DeviceReproduccion(),
                        VozIA = _vozIA
                    };

                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "Config.cfg"), JsonConvert.SerializeObject(config, Formatting.Indented));
                }

                // Theme.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "Theme.cfg")))
                {
                    var theme = new File_Theme
                    {
                        PrimaryColor = "#FF02010F",
                        SecondaryColor = "#FF303055",
                        PrimaryText = "#f0f8ff",
                        SecondaryText = "#87ceeb",
                        PrimaryAcsendText = "#00FF00",
                        SecondaryAcsendText = "#FF35FDE2",
                        ProgressColor = "#FEF200"
                    };
                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "Theme.cfg"), JsonConvert.SerializeObject(theme, Formatting.Indented));
                }

                // IA.cfg
                if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "IA.cfg")))
                {
                    var ia = new File_IA
                    {
                        Activar = true,
                        Apikey = "APIKEY",
                        Compania = "Local",
                        ModeloLocal = ServerPlataform.GetFirstModelName(),
                        ModeloOnline = "Online",
                        EndPoint = "URL"
                    };
                    File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "IA.cfg"), JsonConvert.SerializeObject(ia, Formatting.Indented));
                }

                var subcarpetas = new[] { "Notas de Voz", "Capturas de pantalla", "PerfilGamer", "ITT", "Web", "Filmador" };
                foreach (var carpeta in subcarpetas)
                    Directory.CreateDirectory(Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario), carpeta));

                Directory.CreateDirectory(RutasAbsolutasCFG.Email_CFG(Engrane.File_Config().Usuario));

                // Archivos de correo
                var mails = new[] { "Gmail", "Yahoo", "Hotmail", "Personal" };
                foreach (var nombre in mails)
                {
                    if (!File.Exists(Path.Combine(RutasAbsolutasCFG.Email_CFG(Engrane.File_Config().Usuario), $"{nombre}.cfg")))
                    {
                        var mailConfig = new Mail_Config
                        {
                            Usuario = "Usuario",
                            Pass = "1234",
                            Servidor = "Servidor",
                            Puerto = "Puerto",
                            Activar = false,
                            SSL = false,
                            POP3 = false,
                            IMAP = false
                        };
                        File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Email_CFG(Engrane.File_Config().Usuario), $"{nombre}.cfg"), JsonConvert.SerializeObject(mailConfig, Formatting.Indented));
                    }
                }
            }
        }

        public void FinalizeStartup()
        {
            Engrane._piper.StatusChanged -= OnStatusChanged;
            Engrane._piper.ErrorOccurred -= OnErrorOccurred;

            if (Engrane.File_Config().PrimerInicio)
            {
                var config = new File_Config
                {
                    Usuario = Engrane.File_Config().Usuario.ToLower(),
                    Asistente = Engrane.File_Config().Asistente.ToLower(),
                    Despedida = Engrane.File_Config().Despedida.ToLower(),
                    Dir_Notas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario), "Notas de Voz"),
                    Dir_Pantalla_Capturas = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario), "Capturas de pantalla"),
                    Dir_Musica = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    Dir_Videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    Dir_Imagenes = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Raiz = Path.Combine(RutasAbsolutasCFG.Perfil_CFG(Engrane.File_Config().Usuario)),
                    Opacidad = 1,
                    Experimental = false,
                    Entendimiento = 1500,
                    PrimerInicio = false,
                    Minimizado = false,
                    Windows = false,
                    Telegram_Check = false,
                    ID_Telegram = "00000",
                    Provincia = "Caracas",
                    Lenguaje = "Español",
                    Pais = "Venezuela",
                    Voz = _salidaVoz,
                    Microfono = VoskPlataform.voskManager.DefaultMicrophone,
                    Entrada_Audio = Multimedia.DeviceReproduccion(),
                    VozIA = _vozIA
                };

                File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(config.Usuario), "Config.cfg"), JsonConvert.SerializeObject(config, Formatting.Indented));
                Engrane.EXE(Path.Combine(Environment.CurrentDirectory, "Manual.pdf"));
                Xceed.Wpf.Toolkit.MessageBox.Show(Engrane.novedades, $"Novedades Ver. {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SyncMultimedia()
        {
            int noValido = 0;
            DB_Lite.Eliminar("Multimedia");

            FileInfo[] filesA = Directory.GetFiles(Engrane.File_Config().Dir_Musica, "*.*", SearchOption.AllDirectories)
     .Select(filePath => new FileInfo(filePath))
     .ToArray();

            FileInfo[] filesV = Directory.GetFiles(Engrane.File_Config().Dir_Videos, "*.*", SearchOption.AllDirectories)
    .Select(filePath => new FileInfo(filePath))
    .ToArray();


            foreach (FileInfo file in filesA)
            {
                if (ClassView.List.FormatList.audioExtensions.Contains(file.Extension))
                {
                    try
                    {
                        DB_Lite.InsertarMultimedia(Support.ValidateIconName(Path.GetFileNameWithoutExtension(file.Name)), file.FullName, "Audio");
                    }
                    catch (Exception a)
                    {
                        noValido++;
                    }
                }
            }

            foreach (FileInfo file in filesV)
            {
                if (ClassView.List.FormatList.videoExtensions.Contains(file.Extension))
                {
                    try
                    {
                        DB_Lite.InsertarMultimedia(Support.ValidateIconName(Path.GetFileNameWithoutExtension(file.Name)), file.FullName, "Video");
                    }
                    catch (Exception a)
                    {
                        noValido++;
                    }
                }
            }
        }
    }
}