using AIRH_MAX.ClassView;
using AIRH_MAX.Views;
using PhoneNumbers;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using static AIRH_MAX.ClassView.ViewModel.Libreta;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Agenda : UserControl
    {
        string accion = string.Empty;
        string paisSistema = new RegionInfo(CultureInfo.CurrentCulture.Name).TwoLetterISORegionName;

        public Control_Agenda()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listaFecha.ItemsSource = DB_Lite.ObtenerTabla("Agenda").DefaultView;
            Opacity = Engrane.File_Config().Opacidad;
            CargaAgenda();
        }

        private void AlarmaUI(object sender, RoutedEventArgs e)
        {
            GridAgenda.Visibility = Visibility.Hidden;
            GridContactos.Visibility = Visibility.Hidden;
        }

        private void AgendaUI(object sender, RoutedEventArgs e)
        {
            GridAgenda.Visibility = Visibility.Visible;
            GridContactos.Visibility = Visibility.Hidden;
        }

        private void ContactosUI(object sender, RoutedEventArgs e)
        {
            GridAgenda.Visibility = Visibility.Hidden;
            GridContactos.Visibility = Visibility.Visible;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            TextBlockRuta.Text = Support.BuscarArchivo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        #region Tareas

        private void cbApagarPC_Checked(object sender, RoutedEventArgs e)
        {
            accion = "-s -t 1";
        }

        private void cbReiniciarPc_Checked(object sender, RoutedEventArgs e)
        {
            accion = "-r -t 1";
        }

        private void cbSuspender_Checked(object sender, RoutedEventArgs e)
        {
            accion = "-h";
        }

        private void cbCerrarSesion_Checked(object sender, RoutedEventArgs e)
        {
            accion = "-L";
        }

        #endregion
        #region Organizador   
        public void Reloj()
        {
            var alarmaAgenda = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            alarmaAgenda.Tick += (s, e) => TicTac_Tick();
            alarmaAgenda.Start();
        }

        private void TicTac_Tick()
        {
            var tabla = DB_Lite.ConsultarAgenda();
            ContadorTiempo(tabla);
        }

        private void ContadorTiempo(List<Evento> eventos)
        {
            foreach (var evento in eventos.ToList())
            {
                if (DateTime.TryParse(evento.Fecha, out var fecha) &&
                    DateTime.TryParse(evento.Hora, out var hora) &&
                    DateTime.Now.Date == fecha.Date &&
                    DateTime.Now.ToString("HH:mm") == hora.ToString("HH:mm"))
                {
                    DB_Lite.Eliminar("Agenda", evento.Id.ToString());
                    if (File.Exists(evento.Ruta))
                    {
                        Engrane.EXE(evento.Ruta);
                    }
                    if (!string.Empty.Equals(accion))
                    {
                        Engrane.EXE("shutdown.exe", accion);
                    }
                    MainWindow.NotificacionEvent.MensajeBox = evento.Recordar;
                    Xceed.Wpf.Toolkit.MessageBox.Show(
                        $"{evento.EventoNombre}{Environment.NewLine}{evento.Recordar}{Environment.NewLine}{evento.Fecha}{Environment.NewLine}{evento.Hora}",
                        "AV-AIRH MAX",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    listaFecha.ItemsSource = DB_Lite.ObtenerTabla("Agenda").DefaultView;
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CalGrande.SelectedDate.HasValue &&
                 !string.IsNullOrEmpty(TimePick02.SelectedTime?.ToString()) &&
                 !string.IsNullOrEmpty(txtEvento.Text) &&
                 !string.IsNullOrEmpty(txtRecordar.Text))
            {
                DB_Lite.InsertarAgenda(
                    txtEvento.Text,
                    txtRecordar.Text,
                    CalGrande.SelectedDate.Value.ToShortDateString(),
                    TimePick02.SelectedTime.Value.ToShortTimeString(),
                    TextBlockRuta.Text,
                    accion
                );

                Reloj();
                listaFecha.ItemsSource = DB_Lite.ObtenerTabla("Agenda").DefaultView;
                Engrane.ni.ShowBalloonTip(10000, "Recordatorio:", txtRecordar.Text, System.Windows.Forms.ToolTipIcon.None);
            }
            else
            {
                MainWindow.NotificacionEvent.MensajeBox = "Por favor, llene todos los campos y seleccione una fecha.";
            }
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (listaFecha.SelectedIndex != -1)
            {
                var index = (DataRowView)listaFecha.SelectedItem;
                DB_Lite.Eliminar("Agenda", index.Row.ItemArray[0].ToString());
                listaFecha.ItemsSource = DB_Lite.ObtenerTabla("Agenda").DefaultView;
            }
        }
        #endregion
        #region Contactos
        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (txtNombre01.Text != string.Empty && txtApellido01.Text != string.Empty && txtTelefono01.Text != string.Empty)
            {

                DB_Lite.InsertarContacto(txtNombre01.Text.ToLower(), txtApellido01.Text.ToLower(), ConvertirNumeroConPais(txtTelefono01.Text, paisSistema));
                CargaAgenda();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(@"Debes ingresar: 
- Numero de Contacto
- Apellido
- Numero Telefónico", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void btnBorrar_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (lbContactos.SelectedIndex != -1)
            {
                var index = (DataRowView)lbContactos.SelectedItem;
                DB_Lite.Eliminar("Contactos", index.Row.ItemArray[0].ToString());
                CargaAgenda();
            }
        }

        private void lbContactos_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lbContactos.SelectedIndex != -1)
            {
                var index = (DataRowView)lbContactos.SelectedItem;

                txtNombre01.Text = index.Row.ItemArray[1].ToString();
                txtApellido01.Text = index.Row.ItemArray[2].ToString();
                txtTelefono01.Text = index.Row.ItemArray[3].ToString();
            }
        }

        public static string ConvertirNumeroConPais(string numero, string codigoPaisISO = "VE")
        {
            // Validación básica del número
            if (string.IsNullOrEmpty(numero) || numero.Length != 11 || !numero.StartsWith("04"))
                return numero;

            // Obtener el código telefónico del país
            string codigoPais = ObtenerCodigoTelefonoPorPais(codigoPaisISO);

            // Si no se pudo obtener el código, usar Venezuela como fallback
            if (string.IsNullOrEmpty(codigoPais))
                codigoPais = "58";

            // Convertir 04124789563 a +584124789563 (o código correspondiente)
            return $"+{codigoPais}{numero.Substring(1)}";
        }

        private static string ObtenerCodigoTelefonoPorPais(string codigoPaisISO)
        {
            try
            {
                var phoneUtil = PhoneNumberUtil.GetInstance();
                int countryCode = phoneUtil.GetCountryCodeForRegion(codigoPaisISO.ToUpper());
                return countryCode.ToString();
            }
            catch
            {
                return ""; // Fallback
            }
        }

        private void CargaAgenda()
        {
            lbContactos.ItemsSource = DB_Lite.ObtenerTabla("Contactos").DefaultView;
        }
        #endregion
    }
}
