using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Theme;
using AIRH_MAX.Views;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Vosk_STT.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Config : UserControl
    {
        private readonly DispatcherTimer tiempo = new DispatcherTimer();
        string Tipo = string.Empty;
        bool _vozIA = Engrane.File_Config().VozIA;

        public Control_Config()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargaVoces();
            CargaMicro();
            configInicial();
            cmbCamara.Text = WebCam.Name;
            cbPaises.ItemsSource = ClassView.List.Paises.PaisPair().Keys;

            if (Engrane.File_IA().Compania.Equals("Local"))
            {
                cbModelIAOnline.IsEnabled = false;
                txbAPI_IA.IsEnabled = false;
                URL_IA.IsEnabled = false;
            }
            else
            {
                cbModelIALocal.IsEnabled = false;
            }
        }

        private void configInicial()
        {
            cbLenguaje.Text = Engrane.File_Config().Lenguaje;
            txtIdTelegram.Text = Engrane.File_Config().ID_Telegram;
            cbTelegram.IsChecked = Engrane.File_Config().Telegram_Check;
            cbInicio.IsChecked = Engrane.File_Config().Minimizado;
            cbWindows.IsChecked = Engrane.File_Config().Windows;
            cbExperimental.IsChecked = Engrane.File_Config().Experimental;
            Opacity = Engrane.File_Config().Opacidad;
            txtNombreUsuario.Text = Engrane.File_Config().Usuario;
            txtAdiosAsistente.Text = Engrane.File_Config().Despedida;
            txtNombreAsistente.Text = Engrane.File_Config().Asistente;
            txtNotasVoz.Text = Engrane.File_Config().Dir_Notas;
            txtCapturas.Text = Engrane.File_Config().Dir_Pantalla_Capturas;
            txtVideos.Text = Engrane.File_Config().Dir_Videos;
            txtImagenes.Text = Engrane.File_Config().Dir_Imagenes;
            txtMp3.Text = Engrane.File_Config().Dir_Musica;
            sldOpa.Value = Engrane.File_Config().Opacidad;
            sldCon.Value = Engrane.File_Config().Entendimiento;
            vozAsistente.Text = Engrane.File_Config().Voz;
            cmbCamara.Text = Engrane.File_Config().WebCam;
            micAsistente.Text = Engrane.File_Config().Microfono.Name;
            micAsistente2.Text = Engrane.File_Config().Entrada_Audio;
            cbPaises.Text = Engrane.File_Config().Pais;
            txbEstado.Text = Engrane.File_Config().Provincia;
            cbCompanyIA.Text = Engrane.File_IA().Compania;
            txbAPI_IA.Text = Engrane.File_IA().Apikey;
            URL_IA.Text = Engrane.File_IA().EndPoint;
            cbModelIALocal.ItemsSource = ClassView.IA.ServerPlataform.GetModelNames();
            cbModelIAOnline.Text = Engrane.File_IA().ModeloOnline;
            cbModelIALocal.Text = Engrane.File_IA().ModeloLocal;
            cbIA.IsChecked = Engrane.File_IA().Activar;
            _vozIA = Engrane.File_Config().VozIA;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            guardarConfig();
        }

        private void guardarConfig()
        {
            File_Config config = new File_Config()
            {
                Idioma = cbLenguaje.Text,
                WebCam = cmbCamara.Text,
                Experimental = Convert.ToBoolean(cbExperimental.IsChecked),
                Windows = Convert.ToBoolean(cbWindows.IsChecked),
                Minimizado = Convert.ToBoolean(cbInicio.IsChecked),
                Telegram_Check = Convert.ToBoolean(cbTelegram.IsChecked),
                ID_Telegram = txtIdTelegram.Text,
                Lenguaje = cbLenguaje.Text,
                Despedida = txtAdiosAsistente.Text,
                Asistente = txtNombreAsistente.Text,
                Usuario = txtNombreUsuario.Text,
                Dir_Notas = txtNotasVoz.Text,
                Dir_Pantalla_Capturas = txtCapturas.Text,
                Dir_Musica = txtMp3.Text,
                Dir_Videos = txtVideos.Text,
                Dir_Imagenes = txtImagenes.Text,
                Opacidad = sldOpa.Value,
                Entendimiento = Convert.ToInt32(sldCon.Value),
                PrimerInicio = false,
                Provincia = txbEstado.Text.Replace(" ", "+").Trim(),
                Pais = cbPaises.Text,
                Voz = vozAsistente.Text,
                Microfono = (MicrophoneInfo)micAsistente.SelectedItem,
                Entrada_Audio = Engrane.File_Config().Entrada_Audio,
                VozIA = _vozIA                
            };

            File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "Config.cfg"), JsonConvert.SerializeObject(config, Formatting.Indented));

            File_IA iA = new File_IA()
            {
                Activar = Convert.ToBoolean(cbIA.IsChecked),
                Apikey = txbAPI_IA.Text,
                Compania = cbCompanyIA.Text,
                ModeloLocal = cbModelIALocal.Text,
                ModeloOnline = cbModelIAOnline.Text,
                EndPoint = URL_IA.Text
            };

            File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "IA.cfg"), JsonConvert.SerializeObject(iA, Formatting.Indented));

            //if (!Engrane.File_Config().PrimerInicio)
            //{
                Xceed.Wpf.Toolkit.MessageBox.Show("Se guardaron los cambios", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
        }

        private void btnCamara_Click(object sender, RoutedEventArgs e)
        {
            WebCam.TomarFoto(camCaptura);
        }

        private void cbLenguaje_DropDownClosed(object sender, EventArgs e)
        {
            if (cbLenguaje.Text == "Español")
            {
                //Settings.Default.multiLenguaje = "es-ES";
                //Settings.Default.etiquetaLenguaje = "Español";
                //Settings.Default.Save();
                //UsoComun.MensajeOK("Idioma cambiado");
            }
        }

        private void TxtNombreUsuario_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int ascii = Convert.ToInt32(Convert.ToChar(e.Text));
            if (ascii >= 65 && ascii <= 90 || ascii >= 97 && ascii <= 122 || ascii == 165)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        #region General
        private void sldCon_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int valueMs = (int)Math.Round(e.NewValue);
            if (txtEndSilenceDisplay != null)
                txtEndSilenceDisplay.Text = $" {valueMs} ms";

            if (!IsLoaded) return;

            //Engrane.aSRStartClient.SetVadTimeOutMS(valueMs);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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

            Xceed.Wpf.Toolkit.MessageBox.Show($"Sincronizacion multimedia completada \nArchivos no agregados: {noValido}", "AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void vozAsistente_DropDownClosed(object sender, EventArgs e)
        {
            SpeakOut();
        }

        private void CargaVoces()
        {
            // Voces del sistema
            foreach (var voz in Engrane.AIRH_Voz.GetInstalledVoices()
                .Where(vo => vo.VoiceInfo.Culture.Name.Contains("es")))
            {
                vozAsistente.Items.Add(voz);
            }

            // Voces IA - crear objeto anónimo con misma estructura
            var modelosPath = Path.Combine(Environment.CurrentDirectory, "tts", "models");
            if (Directory.Exists(modelosPath))
            {
                foreach (var archivo in Directory.EnumerateFiles(modelosPath, "*.onnx", SearchOption.AllDirectories))
                {
                    vozAsistente.Items.Add(new
                    {
                        VoiceInfo = new { Name = Path.GetFileNameWithoutExtension(archivo) },
                        Tipo = "Neuronal",
                        FilePath = archivo
                    });
                }
            }

            vozAsistente.DisplayMemberPath = "VoiceInfo.Name";
        }

        private void CargaMicro()
        {
            micAsistente2.Text = Engrane.File_Config().Entrada_Audio;
            var listDevice = VoskPlataform.voskManager.GetAvailableMicrophones();

            foreach (var item in listDevice)
            {
                micAsistente.DisplayMemberPath = "Name";
                micAsistente.Items.Add(item);
            }
        }

        private async void SpeakOut()
        {
            try
            {
                if (vozAsistente.SelectedItem != null)
                {
                    if (vozAsistente.SelectedItem is InstalledVoice vozSistema)
                    {
                        Engrane.AIRH_Voz.SelectVoice(vozSistema.VoiceInfo.Name);
                        VozAsistente.SpeakTest("Prueba de Voz", vozSistema);
                        _vozIA = false; 
                    }
                    else
                    {
                        dynamic selected = vozAsistente.SelectedItem;
                        string filePath = selected.FilePath;

                        if (File.Exists(filePath))
                        {
                            string model = selected.VoiceInfo.Name;
                            await Engrane._piper.ReinitializeAsync(model);
                            VozAsistente.SpeakTest("Prueba de Voz");
                            _vozIA = true;
                        }
                    }
                }
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Seleccione una voz.", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void txtNombreAsistente_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int ascii = Convert.ToInt32(Convert.ToChar(e.Text));
            if (ascii >= 65 && ascii <= 90 || ascii >= 97 && ascii <= 122 || ascii == 165)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtAdiosAsistente_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int ascii = Convert.ToInt32(Convert.ToChar(e.Text));
            if (ascii >= 65 && ascii <= 90 || ascii >= 97 && ascii <= 122 || ascii == 165)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void cbInicio_Checked(object sender, RoutedEventArgs e)
        {
            cbInicio.IsChecked = true;
        }

        private void cbInicio_Unchecked(object sender, RoutedEventArgs e)
        {
            cbInicio.IsChecked = false;
        }

        private void cbWindows_Checked(object sender, RoutedEventArgs e)
        {
            Type ShellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic Shell = Activator.CreateInstance(ShellType);
            dynamic shortcut = Shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\AIRH MAX.lnk");
            shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            shortcut.WorkingDirectory = Environment.CurrentDirectory;
            shortcut.Save();
            cbWindows.IsChecked = true;
        }

        private void cbWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\AIRH MAX.lnk"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\AIRH MAX.lnk");
            }
            cbWindows.IsChecked = false;
        }

        private void micAsistente_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (micAsistente.SelectedItem is MicrophoneInfo dispositivoSeleccionado)
            {
                Debug.WriteLine($"Dispositivo: {dispositivoSeleccionado}, Valor asociado: {dispositivoSeleccionado.Name}");

                bool success = VoskPlataform.voskManager.SelectMicrophone(dispositivoSeleccionado.DeviceId);
                if (success)
                {
                    VoskPlataform.voskManager.SetWaveFormat(16000, 16, 1);
                }
                else
                {
                    MainWindow.NotificacionEvent.Log = $"❌ Error seleccionando micrófono: {dispositivoSeleccionado.Name}";
                }
            }
            else
            {
                Debug.WriteLine("No se ha seleccionado un dispositivo válido.");
            }
        }
        #endregion

        #region Directorios
        private void BtnCaptura_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog buscarCarpeta = new FolderBrowserDialog();
            if (buscarCarpeta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtCapturas.Text = buscarCarpeta.SelectedPath;
                buscarCarpeta.Dispose();
            }
        }

        private void btnNotasVoz_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog buscarCarpeta = new FolderBrowserDialog();
            if (buscarCarpeta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtNotasVoz.Text = buscarCarpeta.SelectedPath;
                buscarCarpeta.Dispose();
            }
        }

        private void btnMp3_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog buscarCarpeta = new FolderBrowserDialog();
            if (buscarCarpeta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtMp3.Text = buscarCarpeta.SelectedPath;
                buscarCarpeta.Dispose();
            }

        }

        private void btnVideos_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog buscarCarpeta = new FolderBrowserDialog();
            if (buscarCarpeta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtVideos.Text = buscarCarpeta.SelectedPath;
                buscarCarpeta.Dispose();
            }
        }
        private void btnImagenes_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog buscarCarpeta = new FolderBrowserDialog();
            if (buscarCarpeta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtImagenes.Text = buscarCarpeta.SelectedPath;
                buscarCarpeta.Dispose();
            }
        }
        #endregion

        #region IA
        private void BtnBotTelegram_Click(object sender, RoutedEventArgs e)
        {
            Engrane.EXE("http://telegram.me/AIRH_Bot");
        }

        private void cbTelegram_Checked(object sender, RoutedEventArgs e)
        {
            if (SolicitudOnline.AccesoInternet())
            {
                cbIA.IsChecked = false;
                Telegrama.TelegramStart();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No hay acceso a internet", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbTelegram.IsChecked = false;
            }

        }

        private void txtIdTelegram_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Support.SoloNumeros(sender, e);
        }


        private void cbNeoxz_Checked(object sender, RoutedEventArgs e)
        {
            if (cbIA.IsChecked == true)
            {
                cbIA.IsChecked = true;
                cbTelegram.IsChecked = false;
            }
        }

        private void cbNeoxz_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cbIA.IsChecked == true)
            {
                cbIA.IsChecked = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Engrane.EXE(Environment.CurrentDirectory + "\\llm\\Models\\");
        }
        #endregion

        #region Notificacion

        private void CbPaises_DropDownClosed(object sender, EventArgs e)
        {
            if (cbPaises.SelectedIndex != -1)
            {
                string ab;
                ClassView.List.Paises.PaisPair().TryGetValue(cbPaises.Text, out ab);
                Engrane.File_Config().AbreviacionPais = ab;
                Engrane.File_Config().Pais = cbPaises.Text;
            }

        }

        private void RadioButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gmail.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }

        private void RadioButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gmail.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }

        private void RadioButton_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            yahoo.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }

        private void RadioButton_MouseLeave_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            yahoo.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }

        private void RadioButton_MouseEnter_2(object sender, System.Windows.Input.MouseEventArgs e)
        {
            hotmail.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }

        private void RadioButton_MouseLeave_2(object sender, System.Windows.Input.MouseEventArgs e)
        {
            hotmail.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }

        private void RadioButton_MouseEnter_3(object sender, System.Windows.Input.MouseEventArgs e)
        {
            otros.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }

        private void RadioButton_MouseLeave_3(object sender, System.Windows.Input.MouseEventArgs e)
        {
            otros.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }

        bool Activar = false;

        private void G_HotmailView(object sender, RoutedEventArgs e)
        {
            Tipo = "Hotmail";
            txtUserPersonal.Text = Engrane.Mail_Config(Tipo).Usuario;
            txtPassPersonal.Password = Engrane.Mail_Config(Tipo).Pass;
            cbPersonal.IsChecked = Engrane.Mail_Config(Tipo).Activar;
            txtServidor.IsEnabled = false;
            txtPuerto.IsEnabled = false;
            cbSSL.IsEnabled = false;
            radioIMAP.IsEnabled = false;
            radioPOP.IsEnabled = false;
        }
        private void G_GmailView(object sender, RoutedEventArgs e)
        {
            Tipo = "Gmail";
            txtUserPersonal.Text = Engrane.Mail_Config(Tipo).Usuario;
            txtPassPersonal.Password = Engrane.Mail_Config(Tipo).Pass;
            cbPersonal.IsChecked = Engrane.Mail_Config(Tipo).Activar;
            txtServidor.IsEnabled = false;
            txtPuerto.IsEnabled = false;
            cbSSL.IsEnabled = false;
            radioIMAP.IsEnabled = false;
            radioPOP.IsEnabled = false;
        }
        private void G_YahooView(object sender, RoutedEventArgs e)
        {
            Tipo = "Yahoo";
            txtUserPersonal.Text = Engrane.Mail_Config(Tipo).Usuario;
            txtPassPersonal.Password = Engrane.Mail_Config(Tipo).Pass;
            cbPersonal.IsChecked = Engrane.Mail_Config(Tipo).Activar;
            txtServidor.IsEnabled = false;
            txtPuerto.IsEnabled = false;
            cbSSL.IsEnabled = false;
            radioIMAP.IsEnabled = false;
            radioPOP.IsEnabled = false;
        }
        private void G_OtrosView(object sender, RoutedEventArgs e)
        {
            Tipo = "Personal";
            txtUserPersonal.Text = Engrane.Mail_Config(Tipo).Usuario;
            txtPassPersonal.Password = Engrane.Mail_Config(Tipo).Pass;
            cbPersonal.IsChecked = Engrane.Mail_Config(Tipo).Activar;

            txtServidor.Text = Engrane.Mail_Config(Tipo).Servidor;
            txtPuerto.Text = Engrane.Mail_Config(Tipo).Puerto;
            cbSSL.IsChecked = Engrane.Mail_Config(Tipo).SSL;
            radioIMAP.IsChecked = Engrane.Mail_Config(Tipo).IMAP;
            radioPOP.IsChecked = Engrane.Mail_Config(Tipo).POP3;

            txtServidor.IsEnabled = true;
            txtPuerto.IsEnabled = true;
            cbSSL.IsEnabled = true;
            radioIMAP.IsEnabled = true;
            radioPOP.IsEnabled = true;
        }


        private void radioPOP_Checked(object sender, RoutedEventArgs e)
        {
            radioPOP.IsChecked = true;
            radioIMAP.IsChecked = false;
        }

        private void radioIMAP_Checked(object sender, RoutedEventArgs e)
        {
            radioPOP.IsChecked = false;
            radioIMAP.IsChecked = true;
        }

        private void cbPersonal_Checked(object sender, RoutedEventArgs e)
        {
            Mail_Config mail_ = new Mail_Config()
            {
                Usuario = txtUserPersonal.Text,
                Pass = txtPassPersonal.Password,
                Servidor = txtServidor.Text,
                Puerto = txtPuerto.Text,
                Activar = true,
                SSL = Convert.ToBoolean(cbSSL.IsChecked),
                POP3 = Convert.ToBoolean(radioPOP.IsChecked),
                IMAP = Convert.ToBoolean(radioIMAP.IsChecked)

            };

            string mail = JsonConvert.SerializeObject(mail_);
            File.WriteAllText(Path.Combine(RutasAbsolutas.Email, Tipo + ".cfg"), mail);
        }
        private void cbPersonal_Unchecked(object sender, RoutedEventArgs e)
        {
            Mail_Config mail_ = new Mail_Config()
            {
                Usuario = txtUserPersonal.Text,
                Pass = txtPassPersonal.Password,
                Servidor = txtServidor.Text,
                Puerto = txtPuerto.Text,
                Activar = false,
                SSL = Convert.ToBoolean(cbSSL.IsChecked),
                POP3 = Convert.ToBoolean(radioPOP.IsChecked),
                IMAP = Convert.ToBoolean(radioIMAP.IsChecked)

            };

            string mail = JsonConvert.SerializeObject(mail_);
            File.WriteAllText(Path.Combine(RutasAbsolutas.Email, Tipo + ".cfg"), mail);
        }
        #endregion

        #region Personalización
        int color;

        string PriColor;
        string SecColor;
        string PriTex;
        string SecTex;
        string PriAcsTex;
        string SecAcsTex;
        string Prog;

        private void ColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (color == 1)
            {
                PriColor = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 2)
            {
                SecColor = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 3)
            {
                PriTex = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 4)
            {
                SecTex = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 5)
            {
                PriAcsTex = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 6)
            {
                SecAcsTex = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
            else if (color == 7)
            {
                Prog = e.NewValue.ToString(); AppTheme.SetTheme(Resources, PriColor, SecColor, PriTex, SecTex, PriAcsTex, SecAcsTex, Prog);
            }
        }


        private void RadioButton_CheckedPriCol(object sender, RoutedEventArgs e)
        {
            color = 1;
        }

        private void RadioButton_CheckedSecCol(object sender, RoutedEventArgs e)
        {
            color = 2;
        }

        private void RadioButton_CheckedPriTex(object sender, RoutedEventArgs e)
        {
            color = 3;
        }

        private void RadioButton_CheckedSecTex(object sender, RoutedEventArgs e)
        {
            color = 4;
        }

        private void RadioButton_CheckedPriAcsText(object sender, RoutedEventArgs e)
        {
            color = 5;
        }

        private void RadioButton_CheckedSecAcsTex(object sender, RoutedEventArgs e)
        {
            color = 6;
        }

        private void RadioButton_CheckedProg(object sender, RoutedEventArgs e)
        {
            color = 7;
        }
        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            AppTheme.SetTheme(Resources);
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            File_Theme theme = new()
            {
                PrimaryColor = PriColor,
                SecondaryColor = SecColor,
                PrimaryText = PriTex,
                SecondaryText = SecTex,
                PrimaryAcsendText = PriAcsTex,
                SecondaryAcsendText = SecAcsTex,
                ProgressColor = Prog
            };

            File.WriteAllText(Path.Combine(RutasAbsolutasCFG.Configuraciones_CFG(Engrane.File_Config().Usuario), "Theme.cfg"), JsonConvert.SerializeObject(theme, Formatting.Indented));
            Xceed.Wpf.Toolkit.MessageBox.Show("Reinicie el programa para que los cambios tengan efecto", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void sldOpa_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = sldOpa.Value;
        }




        #endregion

        private void cbCompanyIA_DropDownClosed(object sender, EventArgs e)
        {
            if (cbCompanyIA.Text.Equals("Local"))
            {
                cbModelIALocal.IsEnabled = true;
                cbModelIAOnline.IsEnabled = false;
                txbAPI_IA.IsEnabled = false;
                URL_IA.IsEnabled = false;
            }
            else
            {
                cbModelIALocal.IsEnabled = false;
                cbModelIAOnline.IsEnabled = true;
                txbAPI_IA.IsEnabled = true;
                URL_IA.IsEnabled = true;
            }
        }

        private void cbExperimental_Checked(object sender, RoutedEventArgs e)
        {
            cbExperimental.IsChecked = true;
            Xceed.Wpf.Toolkit.MessageBox.Show("Modo Experimental:\nQueda bajo tu responsabilidad cualquier anomalia que presente el equipo.\n- Integracion en el menú contextual de Windows con diversas herramientas. ", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private void cbExperimental_Unchecked(object sender, RoutedEventArgs e)
        {
            cbExperimental.IsChecked = false;
        }
    }
}
