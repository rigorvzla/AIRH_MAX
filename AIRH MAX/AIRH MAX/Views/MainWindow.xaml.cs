using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.IA;
using AIRH_MAX.ClassView.Services;
using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.ControlUsuario;
using AIRH_MAX.Popups;
using AIRH_MAX.Properties;
using AIRH_MAX.ViewModels;
using Humanizer;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using DataFormats = System.Windows.DataFormats;

namespace AIRH_MAX.Views
{
    public partial class MainWindow : Window
    {
        Stopwatch SW = new();
        public static AvisoActivado avisoActivo = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotificacionEvent.PropertyChanged += Engrane_PropertyChanged;
            VoskPlataform.voskManager.OnTranscriptionReceived += Modulo.ProcesarComandoVoz;
            LabelIP.Text = PhysicalNetworkUsed.GetNetworkInfo();
            LabelGEO.Text = PhysicalNetworkUsed.GetGeolocationInfo();
            NotificacionEvent.Log = "Para activar a AV-AIRH MAX diga: " + Engrane.File_Config().Asistente + Environment.NewLine + "Para desactivar al AV-AIRH MAX diga: " + Engrane.File_Config().Despedida;
            Process.GetCurrentProcess().MaxWorkingSet = Process.GetCurrentProcess().MinWorkingSet;
            Opacity = Engrane.File_Config().Opacidad;
            IconoBandeja();
            LabelVersion.Content = "Ver. " + Engrane.Version;

            if (DB_Lite.ObtenerTabla("Agenda").Rows.Count > 0)
            {
                Control_Agenda _Agenda = new();
                _Agenda.Reloj();
            }
            if (SolicitudOnline.AccesoInternet())
            {
                if (Engrane.File_Config().Telegram_Check)
                {
                    _ = Telegrama.TelegramStart();
                }

                Task.Run(() =>
                {
                    if (Engrane.Mail_Config("Gmail").Activar)
                    {
                        Email.PopMailKit.CheckMail(Engrane.Mail_Config("Gmail").Usuario, Engrane.Mail_Config("Gmail").Pass, "pop.gmail.com", 995, true, " correos nuevos en gmail", "Gmail");
                    }
                    if (Engrane.Mail_Config("Hotmail").Activar)
                    {
                        Email.ImapMailKit.CheckMail(Engrane.Mail_Config("Hotmail").Usuario, Engrane.Mail_Config("Hotmail").Pass, "outlook.office365.com", 995, true, " correos nuevos en hotmail", "Hotmail");
                    }
                    if (Engrane.Mail_Config("Yahoo").Activar)
                    {
                        Email.ImapMailKit.CheckMail(Engrane.Mail_Config("Yahoo").Usuario, Engrane.Mail_Config("Yahoo").Pass, "pop.mail.yahoo.com", 995, true, " correos nuevos en yahoo", "Yahoo");
                    }
                    if (Engrane.Mail_Config("Personal").Activar)
                    {
                        if (Engrane.Mail_Config("Personal").POP3)
                        {
                            Email.PopMailKit.CheckMail(Engrane.Mail_Config("Personal").Usuario, Engrane.Mail_Config("Personal").Pass, Engrane.Mail_Config("Personal").Servidor, 995, Engrane.Mail_Config("Personal").SSL, " correos nuevos en tu correo personal", "Personal");
                        }
                        else if (Engrane.Mail_Config("Personal").IMAP)
                        {
                            Email.ImapMailKit.CheckMail(Engrane.Mail_Config("Personal").Usuario, Engrane.Mail_Config("Personal").Pass, Engrane.Mail_Config("Personal").Servidor, 993, Engrane.Mail_Config("Personal").SSL, " correos nuevos en tu correo personal", "Personal");
                        }
                    }
                });
            }

            if (Engrane.File_Config().Minimizado) Hide();
            else WindowState = WindowState.Normal;

            #region BotonesDinamicos
            if (Settings.Default.hablaCheck)
            {
                Engrane.AIRH_Voz.Pause();
            }

            HablaChecker();
            EscuchaChecker();

            Micro.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOutline;
            IA_Microfono.ToolTip = "Iniciar Grabación";
            Micro2.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOutline;
            PC_Microfono.ToolTip = "Iniciar Grabación PC";
            Film.Kind = MaterialDesignThemes.Wpf.PackIconKind.Filmstrip;
            FilmadoraPCEstado.ToolTip = "Iniciar Grabación PC";
            #endregion

            avisoActivo.Show();
            avisoActivo.Activado();

            Temporizador.StartTimer();
            FileWatcher.MonitorFile();
            USBD.USBWatcherON();
            NotificacionEvent.MensajeBox = Eventos.SaludoInicial();
            Eventos.FelizNavidad(imageBrush);
            NotificacionEvent.IA = true;
        }



        private void Window_Closed(object sender, EventArgs e)
        {
            USBD.USBWatcherOFF();
            VozAsistente.SpeakTalk(Eventos.SaludoFinal());

            if (Directory.Exists(Environment.CurrentDirectory + "\\Temp\\")) Directory.Delete(Environment.CurrentDirectory + "\\Temp\\", true);

            ServerPlataform.ServerLocal_Stop();
            VoskPlataform.voskManager.KillVosk();
            SignalServer.Shutdown();
            Multimedia.MatarProcesoForzoso();

            Engrane.ni.Visible = false;
            Engrane.ni = null;

            //detectar error de cerrado
            AIRH_MAX.ClassView.GamePass.Autokey.Dispose();
            AIRH_MAX.ClassView.GamePass.Autoclick.Dispose();

            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Notificacion
        USBDetector USBD = new();
        public static Notificaciones NotificacionEvent = new();

        private void Engrane_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MensajeBox")
            {
                _ = Task.Run(() =>
                {
                    Dispatcher.Invoke(() => TablaTextual_IA(NotificacionEvent.MensajeBox));
                    SignalServer.SendResponseToDevice(NotificacionEvent.MensajeBox);
                    VozAsistente.SpeakTalk(NotificacionEvent.MensajeBox, async: true);
                });
            }
            else if (e.PropertyName == "MensajeBoxMute")
            {
                Dispatcher.Invoke(() => TablaTextual_IA(NotificacionEvent.MensajeBoxMute));
            }
            else if (e.PropertyName == "Log")
            {
                Dispatcher.Invoke(() => TablaTextual_Log(NotificacionEvent.Log));
            }
            else if (e.PropertyName == "Pizarra")
            {
                _ = Task.Run(() =>
                {
                    Dispatcher.Invoke(() => TablaTextual_Telegram(NotificacionEvent.Pizarra));
                });
            }
            else if (e.PropertyName == "UserVoice")
            {
                Dispatcher.Invoke(() => TablaTextual_User(NotificacionEvent.UserVoice));
            }
            else if (e.PropertyName == "IA")
            {
                if (NotificacionEvent.IA)
                {
                    Engrane.MP3_Player(Environment.CurrentDirectory + "\\Sonidos\\start.mp3");
                    Dispatcher.Invoke(() => pbMain.Value = 0);
                }
                else
                {
                    Dispatcher.Invoke(() => pbMain.Value = 100);
                }

            }

        }

        #endregion
        #region Metodos_Accion

        private void EscuchaChecker()
        {
            Orejita.Kind = !Settings.Default.escuchaCheck ? MaterialDesignThemes.Wpf.PackIconKind.EarHearing : MaterialDesignThemes.Wpf.PackIconKind.EarHearingOff;
            IA_Escucha.ToolTip = !Settings.Default.escuchaCheck ? "Desactivar escucha" : "Activar Escucha";
        }

        private void HablaChecker()
        {
            Carita.Kind = !Settings.Default.hablaCheck ? MaterialDesignThemes.Wpf.PackIconKind.EmoticonOutline : MaterialDesignThemes.Wpf.PackIconKind.EmoticonHappyOutline;
            IA_Habla.ToolTip = !Settings.Default.hablaCheck ? "Desactivar Voz" : "Activar Voz";
        }

        private void MicrofonoChecker()
        {
            Micro.Kind = Engrane.audioRecorder.IsRecording ? MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOutline : MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOff;
            IA_Microfono.ToolTip = Engrane.audioRecorder.IsRecording ? "Iniciar Grabación" : "Detener Grabación PC";
        }

        private void MicrofonoPC_Checker()
        {
            Micro2.Kind = Multimedia.GrabadoraPC_State() ? MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOutline : MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOff;
            PC_Microfono.ToolTip = Multimedia.GrabadoraPC_State() ? "Iniciar Grabación" : "Detener Grabación PC";
        }

        private void FilmarPC_Checker()
        {
            Film.Kind = Multimedia.FilmarPC_State() ? MaterialDesignThemes.Wpf.PackIconKind.Filmstrip : MaterialDesignThemes.Wpf.PackIconKind.FilmstripOff;
            FilmadoraPCEstado.ToolTip = Multimedia.FilmarPC_State() ? "Iniciar Grabación" : "Detener Grabación PC";
        }

        #endregion
        #region Systray
        private void AcercaDe_Click(object sender, EventArgs e)
        {
            Xceed.Wpf.Toolkit.MessageBox.Show($@"Ver. {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}

El programa está diseñado para ser un asistente virtual accesible y fácil de usar para todas las personas, sin importar su nivel de experiencia o habilidades.
Mi objetivo es ser flexible y adaptable a cualquier usuario, brindando una experiencia de uso intuitiva que les permita aprovechar al máximo el poder de la tecnología", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Web_Click(object sender, EventArgs e)
        {
            Engrane.EXE("https://www.youtube.com/rigorvzla");
        }

        private void Manual_Click(object sender, EventArgs e)
        {
            Engrane.EXE(Environment.CurrentDirectory + "\\Manual.pdf");
        }

        private void Salir_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnMinimizar_Click(object sender, EventArgs e)
        {
            Hide();
            Engrane.ni.ShowBalloonTip(10000, "Estado:", "Minimizado", ToolTipIcon.None);
        }

        private void IconoBandeja()
        {
            ContextMenuStrip ContextMenu = new();
            ContextMenu.Items.Add("Acerca de AIRH MAX", null, new EventHandler(AcercaDe_Click));
            ContextMenu.Items.Add("Canal YouTube", null, new EventHandler(Web_Click));
            ContextMenu.Items.Add("Manual", null, new EventHandler(Manual_Click));
            ContextMenu.Items.Add("-");
            ContextMenu.Items.Add("Minimizar", null, new EventHandler(BtnMinimizar_Click));
            ContextMenu.Items.Add("Salir", null, new EventHandler(Salir_Click));

            Engrane.ni.Icon = new Icon("AIRH.ico");
            Engrane.ni.ContextMenuStrip = ContextMenu;
            Engrane.ni.Visible = true;
            Engrane.ni.Text = "AV-AIRH MAX";
            Engrane.ni.DoubleClick +=

                delegate (object sender, EventArgs args)
                {
                    Show();
                };
        }
        #endregion
        #region Botones
        private async void escuchaSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.escuchaCheck.Equals(false))
            {
                await VoskPlataform.voskManager.StopAutomaticAsync();
                Settings.Default.escuchaCheck = true;
                Settings.Default.Save();
                EscuchaChecker();
                NotificacionEvent.IA = false;
            }
            else if (Settings.Default.escuchaCheck.Equals(true))
            {
                await VoskPlataform.voskManager.StartAutomaticAsync();
                Settings.Default.escuchaCheck = false;
                Settings.Default.Save();
                EscuchaChecker();
                NotificacionEvent.IA = true;
            }
        }

        private void hablaSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.hablaCheck.Equals(false))
            {
                Engrane.AIRH_Voz.Pause();
                Settings.Default.hablaCheck = true;
                Settings.Default.Save();
                HablaChecker();
            }
            else if (Settings.Default.hablaCheck.Equals(true))
            {
                Engrane.AIRH_Voz.SpeakAsyncCancelAll();
                Engrane.AIRH_Voz.Resume();
                Settings.Default.hablaCheck = false;
                Settings.Default.Save();
                HablaChecker();
            }

        }

        private void IA_Microfono_Click(object sender, RoutedEventArgs e)
        {
            MicrofonoChecker();

            if (Engrane.audioRecorder.IsRecording.Equals(false))
            {
                SW = Stopwatch.StartNew();
                GrabadoraEstado.Text = "Grabando";
                GrabadoraEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF00FF00"));

                NotificacionEvent.Log = "Iniciando grabación";

                Multimedia.GrabadoraOn();
                Engrane.audioRecorder.IsRecording = true;
            }
            else
            {
                Multimedia.GrabadoraOff();
                SW.Stop();
                TimeSpan ts = SW.Elapsed;
                SW.Reset();
                GrabadoraEstado.Text = "Grabadora";
                GrabadoraEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Engrane.File_Theme().PrimaryText));
                NotificacionEvent.Log = "Duración de grabación: " + ts.Humanize();
            }
        }

        private async void PC_Film_Click(object sender, RoutedEventArgs e)
        {
            FilmarPC_Checker();

            if (!Multimedia.FilmarPC_State())
            {
                SW = Stopwatch.StartNew();
                FilmadoraPCEstado.Text = "Filmando PC";
                FilmadoraPCEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF00FF00"));
                NotificacionEvent.Log = "Iniciando grabación";
                await Multimedia.IniciarFilmacion(RutasAbsolutas.Filmador + "\\" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss"));
            }
            else
            {
                await Multimedia.DetenerFilmar();
                SW.Stop();
                TimeSpan ts = SW.Elapsed;
                SW.Reset();
                FilmadoraPCEstado.Text = "Filmadora PC";
                FilmadoraPCEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Engrane.File_Theme().PrimaryText));
                NotificacionEvent.Log = "Duración de grabación: " + ts.Humanize();
            }
        }

        private void PC_Microfono_Click(object sender, RoutedEventArgs e)
        {
            MicrofonoPC_Checker();

            if (!Multimedia.GrabadoraPC_State())
            {
                SW = Stopwatch.StartNew();
                GrabadoraPCEstado.Text = "Grabando PC";
                GrabadoraPCEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF00FF00"));
                NotificacionEvent.Log = "Iniciando grabación";
                Multimedia.GrabadoraOn_PC();
            }
            else
            {
                Multimedia.GrabadoraOff_PC();
                SW.Stop();
                TimeSpan ts = SW.Elapsed;
                SW.Reset();
                GrabadoraPCEstado.Text = "Grabadora PC";
                GrabadoraPCEstado.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Engrane.File_Theme().PrimaryText));
                NotificacionEvent.Log = "Duración de grabación: " + ts.Humanize();
            }
        }

        private void BtnPrincipal_Click(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Visible;
        }

        private void BtnGamer_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = null;
            UC.Content = new Control_Gamer();

        }

        private void BtnCorreo_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = null;
            UC.Content = new Control_CE();
        }

        private void BtnAgenda_Click(object sender, RoutedEventArgs e)
        {
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = null;
            UC.Content = new Control_Agenda();
        }

        private void BtnComandos(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Comandos();
        }

        private void BtnConfiguracion(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Config();
        }

        private void BtnHerramientas(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Fix();
        }

        private void BtnMenuVideo(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_MenuVideos();
        }

        private void BtnHerramientasM(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Multimedia();
        }

        private void BtnArduino(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Arduino();
        }

        private void BtnSistema(object sender, RoutedEventArgs e)
        {
            Engrane.EXE("MAX.exe", "INFO");
        }


        private void BtnMonitor_Click(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_FileMonitor();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UC.Content = null;
            GridPrincipal.Visibility = Visibility.Hidden;
            UC.Content = new Control_Comentario();
        }
        #endregion
        #region Habla/Escucha     

        private void TablaTextual_Log(string texto)
        {
            var _IA = new Chat_Item
            {
                Nombre_IA = "Log",
                Tiempo = DateTime.Now.ToShortTimeString(),
                Mensaje = texto
            };
            ListBoxLog.Items.Add(_IA);
            ListBoxLog.Items.MoveCurrentToLast();
            ListBoxLog.ScrollIntoView(ListBoxLog.Items.CurrentItem);
        }

        private void TablaTextual_IA(string texto)
        {
            var _IA = new Chat_Item
            {
                Nombre_IA = Engrane.File_Config().Asistente,
                Tiempo = DateTime.Now.ToShortTimeString(),
                Mensaje = texto,
                ImageSource = "/PNG/AIRH.png"
            };
            TextBoxVisual.Items.Add(_IA);
            TextBoxVisual.Items.MoveCurrentToLast();
            TextBoxVisual.ScrollIntoView(TextBoxVisual.Items.CurrentItem);
        }

        private void TablaTextual_Telegram(string texto)
        {
            var _IA = new Chat_Item
            {
                Nombre_IA = texto.Split("(!)")[0],
                Tiempo = DateTime.Now.ToShortTimeString(),
                Mensaje = texto.Split("(!)")[1],
                ImageSource = "/PNG/local.png"
            };
            TextBoxVisual.Items.Add(_IA);
            TextBoxVisual.Items.MoveCurrentToLast();
            TextBoxVisual.ScrollIntoView(TextBoxVisual.Items.CurrentItem);
        }


        private void TablaTextual_User(string texto)
        {
            var _IA = new Chat_Item
            {
                Nombre_IA = Engrane.File_Config().Usuario,
                Tiempo = DateTime.Now.ToShortTimeString(),
                Mensaje = texto,
                ImageSource = "/PNG/user.png"
            };
            TextBoxVisual.Items.Add(_IA);
            TextBoxVisual.Items.MoveCurrentToLast();
            TextBoxVisual.ScrollIntoView(TextBoxVisual.Items.CurrentItem);
        }
        #endregion
        #region DropItems     
        private void CbTelegram_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!SolicitudOnline.AccesoInternet())
            {
                NotificacionEvent.MensajeBox = "No hay Conexion a internet";
                return;
            }

            if (Engrane.File_Config().ID_Telegram.Equals("000000000") || Engrane.File_Config().ID_Telegram.Length != 9)
            {
                NotificacionEvent.MensajeBox = "Asigne un ID valido de telegram en configuración";
                return;
            }


            string[] s = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);

            var fileHandlers = new Dictionary<string, Action<string, CancellationTokenSource>>
            {
                { ".doc", (path, token) => Telegrama.Bot.SendDocument(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".docx", (path, token) => Telegrama.Bot.SendDocument(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".rtf", (path, token) => Telegrama.Bot.SendDocument(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".pdf", (path, token) => Telegrama.Bot.SendDocument(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".jpg", (path, token) => Telegrama.Bot.SendPhoto(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".jpeg", (path, token) => Telegrama.Bot.SendPhoto(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".png", (path, token) => Telegrama.Bot.SendPhoto(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".bmp", (path, token) => Telegrama.Bot.SendPhoto(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".gif", (path, token) => Telegrama.Bot.SendPhoto(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".mp3", (path, token) => Telegrama.Bot.SendAudio(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".mp2", (path, token) => Telegrama.Bot.SendAudio(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".wav", (path, token) => Telegrama.Bot.SendAudio(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".mp4", (path, token) => Telegrama.Bot.SendVideo(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".3gp", (path, token) => Telegrama.Bot.SendVideo(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".avi", (path, token) => Telegrama.Bot.SendVideo(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) },
                { ".mkv", (path, token) => Telegrama.Bot.SendVideo(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(path)), cancellationToken: token.Token) }
            };

            foreach (string filePath in s)
            {
                string datos = $@"Nombre: {Path.GetFileNameWithoutExtension(filePath)}
Tipo: {Path.GetExtension(filePath)}
Ruta: {Path.GetDirectoryName(filePath)}
El Bot AIRH en telegram, te avisara al llegar el archivo.";

                CancellationTokenSource cancellationTokenSource = new();
                Chat_Item ViewModel = new()
                {
                    Nombre_IA = Engrane.File_Config().Asistente,
                    Tiempo = DateTime.Now.ToShortTimeString(),
                    Mensaje = datos,
                    ImageSource = "/PNG/AIRH.png",
                    button = ControlesManuales.ButtomTemplate()
                };

                ControlesManuales.taskLookup.Add(ControlesManuales.TokenID++, cancellationTokenSource);
                TextBoxVisual.Items.Add(ViewModel);

                string extension = Path.GetExtension(filePath).ToLower();
                if (fileHandlers.ContainsKey(extension))
                {
                    fileHandlers[extension](filePath, cancellationTokenSource);
                }
                else
                {
                    Telegrama.Bot.SendDocument(Engrane.File_Config().ID_Telegram, InputFile.FromStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(filePath)), cancellationToken: cancellationTokenSource.Token);
                }
            }
        }

        private void Imagen_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.All;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void ImagenDrop_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void ImagenDrop_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);
            foreach (string filePath in s)
            {
                string entrada = Path.GetFullPath(filePath);
                string salida = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".jpg");

                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        TablaTextual_IA($"Analizando: {salida}{Environment.NewLine}");
                        Multimedia.ConvertToJpg(entrada, salida);
                        NotificacionEvent.MensajeBox = "Archivo procesado";
                        NotificacionEvent.Log = $"Guardado: {salida}";
                    });
                });
            }
        }

        private async void imagenTexto_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);

            foreach (string filePath in s)
            {
                string entrada = Path.GetFullPath(filePath);
                string salida = Path.Combine(Environment.CurrentDirectory, "Perfiles", Engrane.File_Config().Usuario, "ITT", Guid.NewGuid() + ".txt");

                // Mostrar progreso inmediatamente
                NotificacionEvent.Log = $"Analizando: {Path.GetFileName(entrada)}";

                // Ejecutar OCR de forma asíncrona
                string resultado = await Multimedia.ImageToText(entrada);
                NotificacionEvent.MensajeBoxMute = resultado;

                if (!resultado.Contains("Error"))
                {
                    NotificacionEvent.MensajeBox = "Archivo procesado";
                    NotificacionEvent.Log = $"Guardado: {salida}";
                }
            }
        }

        private async void mp3Box_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s == null || s.Length == 0)
                return;

            NotificacionEvent.MensajeBox = $"Iniciando conversión de {s.Length} elemento.";
            LabelEstado.Content = "Convirtiendo";

            try
            {
                foreach (string filePath in s)
                {
                    var convertWindow = new ConvertMediaPack(1, filePath);
                    convertWindow.Owner = this;
                    convertWindow.Show();

                    await convertWindow.StartConversionAsync();
                    convertWindow.Close();
                    NotificacionEvent.Log = $"Guardado: {Path.GetDirectoryName(filePath)}\\{Path.GetFileNameWithoutExtension(filePath)}.mp3";
                }

                NotificacionEvent.MensajeBox = "Conversión finalizada";

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error durante la conversión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NotificacionEvent.MensajeBox = "Conversión interrumpida por error";
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    LabelEstado.Content = string.Empty;
                });
            }
        }

        private async void mp4Box_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s == null || s.Length == 0)
                return;

            NotificacionEvent.MensajeBox = $"Iniciando conversión de {s.Length} elemento.";
            LabelEstado.Content = "Convirtiendo";

            try
            {
                foreach (string filePath in s)
                {
                    var convertWindow = new ConvertMediaPack(2, filePath);
                    convertWindow.Owner = this;
                    convertWindow.Show();

                    await convertWindow.StartConversionAsync();
                    convertWindow.Close();
                    NotificacionEvent.Log = $"Guardado: {Path.GetDirectoryName(filePath)}\\{Path.GetFileNameWithoutExtension(filePath)}.mp4";
                }

                NotificacionEvent.MensajeBox = "Conversión finalizada";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error durante la conversión a MP4:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NotificacionEvent.MensajeBox = "Conversión interrumpida";
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    LabelEstado.Content = string.Empty;
                });
            }
        }
        #endregion
        #region StyleUI
        private async void TextBoxEnlace_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string input = TextBoxEnlace.Text;
            if (e.Key == Key.Enter && input != string.Empty)
            {
                TablaTextual_User(input);
                TextBoxEnlace.Text = string.Empty;

                if (Engrane.File_Config().Telegram_Check == true)
                {
                    await Telegrama.Bot.SendMessage(Engrane.File_Config().ID_Telegram, input);
                    return;
                }

                if (Engrane.File_IA().Activar == false)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Active en configuración la IA");
                    return;
                }

                await Modulo.RequestIA(input);
            }
        }



        #endregion

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatClient(Engrane.File_Config().Usuario);
            chatWindow.Show();
        }
    }
}