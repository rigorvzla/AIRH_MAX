using AIRH_MAX.ClassView;
using AIRH_MAX.Views;
using System.Data;
using System.IO.Ports;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Arduino : System.Windows.Controls.UserControl
    {
        string strBufferOut;
        SerialPort ports;

        public Control_Arduino()
        {
            InitializeComponent();
        }

        private void Refresh_ports()
        {
            string[] PuertosDisponibles = SerialPort.GetPortNames();
            cb_ports.Items.Clear();

            foreach (string puertos in PuertosDisponibles)
            {
                cb_ports.Items.Add(puertos);
            }

            if (cb_ports.Items.Count > 0)
            {
                cb_ports.SelectedIndex = 0;
                Btn_conect.IsEnabled = true;
            }
            else
            {
                cb_ports.Text = string.Empty;
                strBufferOut = string.Empty;
                Btn_conect.IsEnabled = false;
                Btn_send_Data.IsEnabled = false;
            }

        }

        private void AgregarComandosBD()
        {
            if (txt_comando.Text == string.Empty && txt_accion.Text == string.Empty)
            {
                MainWindow.NotificacionEvent.MensajeBox = "Ingrese un Comando y una Acción";
            }
            else
            {
                DB_Lite.InsertarArduino(txt_comando.Text, txt_accion.Text, txt_respuesta.Text, cb_ports.Text, cbBaudRate.Text);

                txt_comando.Text = string.Empty;
                txt_accion.Text = string.Empty;
                txt_respuesta.Text = string.Empty;
            }
        }

        private void borrarDatos()
        {
            if (dgDatos.SelectedIndex != -1)
            {
                var index = (DataRowView)dgDatos.SelectedItem;
                DB_Lite.Eliminar("Arduino", index.Row.ItemArray[0].ToString());

                CargarDatos();
                Xceed.Wpf.Toolkit.MessageBox.Show("Comando eliminado", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CargarDatos()
        {
            dgDatos.ItemsSource = DB_Lite.ObtenerTabla("Arduino").DefaultView;
        }

        private void Btn_conect_Click(object sender, RoutedEventArgs e)
        {
            if (Btn_conect.Content.Equals("Conectar"))
            {
                try
                {
                    ports = new SerialPort(cb_ports.Items[0].ToString())
                    {
                        BaudRate = Convert.ToInt32(cbBaudRate.SelectedItem),
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        PortName = cb_ports.Text
                    };
                    ports.Open();
                    Btn_conect.Content = "Desconectar";
                    Btn_conect.Foreground = Brushes.Green;
                    Btn_send_Data.IsEnabled = true;
                }
                catch
                {
                    MainWindow.NotificacionEvent.Log = "Dispositivo no encontrado";
                }
            }
            else if (Btn_conect.Content.Equals("Desconectar"))
            {
                ports.Close();
                Btn_conect.Content = "Conectar";
                Btn_conect.Foreground = Brushes.Red;
                Btn_send_Data.IsEnabled = false;
            }

        }

        private void Btn_send_Data_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ports.DiscardOutBuffer();
                strBufferOut = TextBoxData.Text;
                ports.Write(strBufferOut);
            }
            catch
            {
                MainWindow.NotificacionEvent.Log = "Dispositivo no encontrado";
            }
        }

        private void Btn_add_comandos_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txt_comando.Text) && !String.IsNullOrEmpty(txt_accion.Text))
            {
                AgregarComandosBD();
                CargarDatos();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Btn_conect.Content.Equals("Desconectar"))
            {
                Btn_conect.Content = "Conectar";
                Btn_conect.Foreground = Brushes.Red;
                Btn_send_Data.IsEnabled = false;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh_ports();
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            borrarDatos();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
            Opacity = Engrane.File_Config().Opacidad;
            cbBaudRate.Items.Add("110");
            cbBaudRate.Items.Add("300");
            cbBaudRate.Items.Add("600");
            cbBaudRate.Items.Add("1200");
            cbBaudRate.Items.Add("2400");
            cbBaudRate.Items.Add("4800");
            cbBaudRate.Items.Add("9600");
            cbBaudRate.Items.Add("14400");
            cbBaudRate.Items.Add("19200");
            cbBaudRate.Items.Add("38400");
            cbBaudRate.Items.Add("57600");
            cbBaudRate.Items.Add("115200");
            cbBaudRate.Items.Add("128000");
            cbBaudRate.Items.Add("230400");
            cbBaudRate.Items.Add("256000");
            cbBaudRate.Items.Add("468000");
            cbBaudRate.Items.Add("921600");
        }


        private void txt_comando_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_comando.Clear();
        }

        private void txt_accion_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_accion.Clear();
        }

        private void txt_respuesta_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_respuesta.Clear();
        }

        private void txt_comando_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txt_comando.Text))
            {
                txt_comando.Text = "@Comando";
            }
        }

        private void txt_accion_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txt_accion.Text))
            {
                txt_accion.Text = "@Acción";
            }
        }

        private void Txt_respuesta_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txt_respuesta.Text))
            {
                txt_respuesta.Text = "@Respuesta";
            }
        }

        private void dgDatos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgDatos.SelectedIndex != -1)
            {
                var index = (DataRowView)dgDatos.SelectedItem;

                txt_comando.Text = index.Row.ItemArray[1].ToString();
                txt_accion.Text = index.Row.ItemArray[2].ToString();
                txt_respuesta.Text = index.Row.ItemArray[3].ToString();
            }
        }
    }
}