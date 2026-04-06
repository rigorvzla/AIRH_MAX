using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.GamePass;
using AIRH_MAX.Popups;
using AIRH_MAX.Properties;
using AIRH_MAX.Views;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Engrane = AIRH_MAX.ClassView.Engrane;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Gamer : UserControl
    {
        SortedDictionary<string, int> prDict;

        public Control_Gamer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
            MostrarGrid();
            btnDesactivar.IsEnabled = false;
            btnDetenerClick.IsEnabled = false;

            Autoclick.OnAutoclickCompletado += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    btnStartClick.IsEnabled = true;
                    btnDetenerClick.IsEnabled = false;
                    Xceed.Wpf.Toolkit.MessageBox.Show("AutoClick completado", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            };
        }

        #region UI
        private void MostrarGrid()
        {
            dgComandos.ItemsSource = DB_Lite.ObtenerTabla("Gamer").DefaultView;
            dgComandosAuto.ItemsSource = DB_Lite.ObtenerTabla("Autokey").DefaultView;
        }

        private void btnGuardar1_Click2(object sender, RoutedEventArgs e)
        {
            string directoryPath = Path.Combine(RutasAbsolutas.GameFolder, "Autokey");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.InitialDirectory = directoryPath;
                sfd.Title = "Guardar Perfil";
                sfd.Filter = "Perfil (.csv)|*.csv";

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DB_Lite.ExportTableToCSV("Autokey", sfd.FileName);
                    Xceed.Wpf.Toolkit.MessageBox.Show(
                        "Configuración guardada",
                        "AV-AIRH MAX",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }

        private void btnCargar_Click2(object sender, RoutedEventArgs e)
        {
            string directoryPath = Path.Combine(RutasAbsolutas.GameFolder, "Autokey");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Title = "Cargar Perfil";
                ofd.Multiselect = false;
                ofd.InitialDirectory = directoryPath;
                ofd.Filter = "Perfil (.csv)|*.csv";

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DB_Lite.ImportCSVToTable("Autokey", ofd.FileName);
                    MostrarGrid();
                }
            }
        }

        private void btnRestaurar_Click2(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Se eliminaran todos los comandos ¿Deseas limpiar el perfil actual?", "AV-AIRH MAX", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DB_Lite.Eliminar("Autokey");
                MostrarGrid();
            }
        }

        private void chkSecuencia_Checked(object sender, RoutedEventArgs e)
        {
            Autokey.secuen = true;
            Xceed.Wpf.Toolkit.MessageBox.Show("La secuencia se ejecutara en el orden mostrado de la lista de comandos", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDesactivar_Click(object sender, RoutedEventArgs e)
        {
            if (chkSecuencia.IsChecked.Equals(false))
            {
                ClassView.GamePass.Autokey.DetenerAutokey();
                ClassView.GamePass.Engrane.ResetContador();
                btnComenzar.IsEnabled = true;
                btnDesactivar.IsEnabled = false;
            }
            else
            {
                ClassView.GamePass.Autokey.DetenerAutokey();
                chkSecuencia.IsChecked = false;
                btnComenzar.IsEnabled = true;
                btnDesactivar.IsEnabled = false;
            }
        }

        private void btnComenzar_Click(object sender, RoutedEventArgs e)
        {
            if (chkSecuencia.IsChecked.Equals(false))
            {
                StartAutokey();
            }
            else
            {
                StartSecuencia();
            }
        }
        #endregion

        #region Estetica
        private void btnGuardar1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnGuardar1.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }
        private void btnGuardar1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnGuardar1.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }
        private void btnRestaurar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnRestaurar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }
        private void btnRestaurar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnRestaurar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }
        private void btnCargar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCargar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 32, 47));
        }
        private void btnCargar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnCargar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(2, 1, 15));
        }
        #endregion

        #region Teclado   

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            txbTeclaPresionada.Clear();
            if (e.Key.ToString() == "System")
            {
                txbTeclaAsignada.Text = e.SystemKey.ToString();
                return;
            }
            txbTeclaAsignada.Text = e.Key.ToString();
        }

        private void btnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Se eliminaran todos los comandos ¿Deseas limpiar el perfil actual?", "AV-AIRH MAX", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DB_Lite.Eliminar("Gamer");
                MostrarGrid();
            }
        }

        private void txbTiempoPresion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int key = (int)e.Key;

            e.Handled = !(key >= 34 && key <= 43 ||
                          key >= 74 && key <= 83 ||
                          key == 2);
        }

        private void LbJuegos_MouseEnter(object sender, MouseEventArgs e)
        {
            prDict = new SortedDictionary<string, int>(Process.GetProcesses().Where(pro => !string.IsNullOrEmpty(pro.MainWindowTitle) && pro.ProcessName != "AIRH MAX").ToDictionary((pr) => string.Format("{0} [{1}]", pr.ProcessName, pr.Id), (pr) => pr.Id), StringComparer.Ordinal);
            lbJuegos.ItemsSource = prDict.Keys.ToList();
        }

        private void lbJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbJuegos.SelectedIndex != -1)
                {
                    Settings.Default.idProcess = prDict[lbJuegos.SelectedItem.ToString()];
                    Settings.Default.Save();
                }
            }
            catch
            {
            }
        }

        private void btnGuardar1_Click(object sender, RoutedEventArgs e)
        {
            string userDirectory = Path.Combine(Environment.CurrentDirectory, Engrane.File_Config().Usuario, "Perfiles", "Teclado");

            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);

            using (var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.InitialDirectory = userDirectory;
                sfd.Title = "Guardar Perfil";
                sfd.Filter = "Perfil (.csv)|*.csv";

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DB_Lite.ExportTableToCSV("Gamer", sfd.FileName);
                    Xceed.Wpf.Toolkit.MessageBox.Show(
                        "Configuración guardada",
                        "AV-AIRH MAX",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }

        private void btnCargar_Click(object sender, RoutedEventArgs e)
        {
            string userDirectory = Path.Combine(Environment.CurrentDirectory, Engrane.File_Config().Usuario, "Perfiles", "Teclado");

            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);

            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Title = "Cargar Perfil";
                ofd.Multiselect = false;
                ofd.InitialDirectory = userDirectory;
                ofd.Filter = "Perfil (.csv)|*.csv";

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DB_Lite.ImportCSVToTable("Gamer", ofd.FileName);
                    MostrarGrid();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txbComando.Text != string.Empty && txbTeclaAsignada.Text != string.Empty)
            {
                if (DB_Lite.ConsultaGamer(txbTeclaAsignada.Text, "Gamer"))
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Esta tecla esta ya asignada, ¿deseas modificar el comando?", "AV-AIRH MAX", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DB_Lite.ActualizarGamer(txbComando.Text.ToLower(), DB_Lite.GetIdFromTable("Gamer", txbTeclaAsignada.Text), txbTiempoPresion.Text);
                        MostrarGrid();
                    }
                }
                else
                {
                    DB_Lite.InsertarGamer(txbTeclaAsignada.Text, txbComando.Text.ToLower(), txbTiempoPresion.Text);
                    MostrarGrid();
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Asigne una letra y un comando para guardar", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (dgComandos.SelectedIndex != -1)
            {
                var index = (DataRowView)dgComandos.SelectedItem;

                DB_Lite.Eliminar("Gamer", index.Row.ItemArray[0].ToString());
                MostrarGrid();
            }
        }
        #endregion

        #region AutoKey

        private void StartAutokey()
        {
            if (lbJuegosAuto.SelectedItem != null)
            {
                ClassView.GamePass.Autokey.AutokeyTimer();
                btnComenzar.IsEnabled = false;
                btnDesactivar.IsEnabled = true;
            }
            else
            {
                MainWindow.NotificacionEvent.MensajeBox = "Selecciona un proceso primero, para enviar la acción";
                prDict = new SortedDictionary<string, int>(Process.GetProcesses().ToDictionary((pr) => string.Format("{0} [{1}]", pr.ProcessName, pr.Id), (pr) => pr.Id), StringComparer.Ordinal);
                lbJuegosAuto.ItemsSource = prDict.Keys.ToList();
            }
        }

        private void TextBox_PreviewKeyUp1(object sender, KeyEventArgs e)
        {
            txbTeclaPresionadaAuto.Clear();
            if (e.Key.ToString() == "System")
            {
                txbTeclaAsignadaAuto.Text = e.SystemKey.ToString();
                return;
            }
            txbTeclaAsignadaAuto.Text = e.Key.ToString();
        }

        private void LbJuegos_MouseEnter2(object sender, MouseEventArgs e)
        {
            prDict = new SortedDictionary<string, int>(Process.GetProcesses().Where(pro => !string.IsNullOrEmpty(pro.MainWindowTitle) && pro.ProcessName != "AIRH").ToDictionary((pr) => string.Format("{0} [{1}]", pr.ProcessName, pr.Id), (pr) => pr.Id), StringComparer.Ordinal);
            lbJuegosAuto.ItemsSource = prDict.Keys.ToList();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (txbTeclaAsignadaAuto.Text != string.Empty && txbTiempoAccion.Text != string.Empty)
            {
                if (DB_Lite.ConsultaGamer(txbTeclaAsignadaAuto.Text, "Autokey"))
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Esta tecla esta ya asignada, ¿deseas modificar el comando?", "AV-AIRH MAX", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DB_Lite.ActualizarGamerAuto(txbTeclaAsignadaAuto.Text.ToLower(), DB_Lite.GetIdFromTable("Autokey", txbTeclaAsignadaAuto.Text), txbTiempoAccion.Text);
                        MostrarGrid();
                    }
                }
                else
                {
                    DB_Lite.InsertarGamerAuto(txbTeclaAsignadaAuto.Text, txbTiempoAccion.Text);
                    MostrarGrid();
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Asigne una letra y un comando para guardar", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (dgComandosAuto.SelectedIndex != -1)
            {
                var index = (DataRowView)dgComandosAuto.SelectedItem;

                DB_Lite.Eliminar("Autokey", index.Row.ItemArray[0].ToString());
                MostrarGrid();
            }
        }

        private void lbJuegos_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Settings.Default.idProcess2 = prDict[lbJuegosAuto.SelectedItem.ToString()];
                Settings.Default.Save();
            }
            catch
            {
            }
        }
        #endregion

        #region Secuencia
        private void StartSecuencia()
        {
            if (lbJuegosAuto.SelectedItem != null)
            {
                ClassView.GamePass.Secuencia.StartSecuencia();
                btnComenzar.IsEnabled = false;
                btnDesactivar.IsEnabled = true;
            }
            else
            {
                MainWindow.NotificacionEvent.MensajeBox = "Selecciona un proceso primero, para enviar la acción";
                prDict = new SortedDictionary<string, int>(Process.GetProcesses().ToDictionary((pr) => string.Format("{0} [{1}]", pr.ProcessName, pr.Id), (pr) => pr.Id), StringComparer.Ordinal);
                lbJuegosAuto.ItemsSource = prDict.Keys.ToList();
            }
        }
        private void chkSecuencia_Unchecked(object sender, RoutedEventArgs e)
        {
            Autokey.secuen = false;
        }
        #endregion

        #region AutoClick

        private void txbRepetir_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int key = (int)e.Key;
            e.Handled = !(key >= 34 && key <= 43 ||
                          key >= 74 && key <= 83 ||
                          key == 2) || key == 110;
        }

        private void txbPosicionClick_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;

            ClickerPosition cp = new ClickerPosition();
            cp.ShowDialog();

            parentWindow.WindowState = WindowState.Normal;
            txbPosicionClick.Content = "X:" + Engrane.PosX + " Y:" + Engrane.PosY;

            // Actualizar posición en la clase Autoclick
            Autoclick.PosX = Engrane.PosX;
            Autoclick.PosY = Engrane.PosY;
        }

        private void btnStartClick_Click(object sender, RoutedEventArgs e)
        {
            ActivarAutoclick();
        }

        public void ActivarAutoclick()
        {
            try
            {
                // Configurar Autoclick desde la UI
                Autoclick.PosX = Engrane.PosX;
                Autoclick.PosY = Engrane.PosY;
                Autoclick.TipoClick = cbTipoClick.Text;
                Autoclick.BotonMouse = cbBotonMouse.Text;

                // Configurar intervalo
                if (int.TryParse(txbIntervalo.Text, out int intervalo))
                {
                    Autoclick.ConfigurarIntervalo(intervalo, cbIntervalo.Text);
                }

                if (int.TryParse(txbRepetir.Text, out int repeticiones))
                {
                    Autoclick.IniciarAutoclick(repeticiones);
                    btnStartClick.IsEnabled = false;
                    btnDetenerClick.IsEnabled = true;
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Número de repeticiones no válido", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDetenerClick_Click(object sender, RoutedEventArgs e)
        {
            Autoclick.DetenerAutoclick();
            btnStartClick.IsEnabled = true;
            btnDetenerClick.IsEnabled = false;
            Xceed.Wpf.Toolkit.MessageBox.Show("AutoClick detenido", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
        }
               

        #endregion

    }
}