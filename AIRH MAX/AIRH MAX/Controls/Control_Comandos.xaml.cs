using AIRH_MAX.ClassView;
using AIRH_MAX.Views;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Comandos : UserControl
    {
        string Tabla = "Social";

        public Control_Comandos()
        {
            InitializeComponent();
        }



        private void BtnT_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "Textos";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = true;
        }

        private void btnS_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "Social";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = false;
        }

        private void btnC_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "Carpetas";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = false;
        }

        private void btnA_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "App";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = false;
        }

        private void btnPaginasWebs_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "Web";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = true;
        }

        private void btnDiscord_Click(object sender, RoutedEventArgs e)
        {
            Tabla = "Discord";
            MostrarGrid(Tabla);
            tbRuta.IsEnabled = true;
        }
        private void btnInternos_Click(object sender, RoutedEventArgs e)
        {
            Engrane.EXE(Environment.CurrentDirectory + "\\Manual.pdf");
            tbRuta.IsEnabled = false;
        }


        private void btn_AgregarCmd_Click(object sender, RoutedEventArgs e)
        {
            agregarComando(tbCmd.Text.ToLower(), tbRuta.Text, tbRespuesta.Text);
        }

        private void agregarComando(string Comando, string Ruta, string Respuesta)
        {
            if (tbCmd.Text == string.Empty && tbRuta.Text == string.Empty || tbCmd.Text == string.Empty && (tbRespuesta.Text == string.Empty && Tabla == "Social"))
            {
                MainWindow.NotificacionEvent.MensajeBox = "Ingrese un comando, una accion y opcionalmente una respuesta";
            }
            else if (Tabla != "Discord")
            {
                DB_Lite.InsertarComando(Tabla, Comando.Replace(" ","_"), Ruta, Respuesta);
                MostrarGrid(Tabla);

                tbCmd.Text = string.Empty;
                tbRuta.Text = string.Empty;
                tbRespuesta.Text = string.Empty;
            }
            else
            {
                DB_Lite.InsertarDiscord(Comando.Replace(" ", "_"), Ruta);
                MostrarGrid(Tabla);

                tbCmd.Text = string.Empty;
                tbRuta.Text = string.Empty;
                tbRespuesta.Text = string.Empty;
            }
        }

        private void borrarDatos()
        {
            if (dgComandos.SelectedIndex == -1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Seleccione el comando a eliminar", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var index = (DataRowView)dgComandos.SelectedItem;

            DB_Lite.Eliminar(Tabla, index.Row.ItemArray[0].ToString());
            MostrarGrid(Tabla);

            tbCmd.Text = string.Empty;
            tbRuta.Text = string.Empty;
            tbRespuesta.Text = string.Empty;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            borrarDatos();
        }

        private void btnBuscarArchivo_Click(object sender, RoutedEventArgs e)
        {
            buscarArchivos();
        }

        private void buscarArchivos()
        {
            if (Tabla == "App")
            {
                tbRuta.Text = Support.BuscarArchivo();
            }
            else if (Tabla == "Carpetas")
            {
                tbRuta.Text = Support.BuscarCarpeta();
            }
        }

        private void MostrarGrid(string Tipo)
        {
            dgComandos.ItemsSource = DB_Lite.ObtenerTabla(Tipo).DefaultView;
        }


        private void BtnModificar(object sender, RoutedEventArgs e)
        {
            if (dgComandos.SelectedIndex == -1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Seleccione un comando para modificar", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                var index = (DataRowView)dgComandos.SelectedItem;
                DB_Lite.Actualizar(Tabla, tbCmd.Text, tbRuta.Text, tbRespuesta.Text, index.Row.ItemArray[0].ToString());
                MostrarGrid(Tabla);
            }
        }

        private void DgComandos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgComandos.SelectedIndex != -1)
            {
                var index = (DataRowView)dgComandos.SelectedItem;
                if (Tabla == "Discord")
                {

                    tbCmd.Text = index.Row.ItemArray[0].ToString();
                    tbRuta.Text = index.Row.ItemArray[1].ToString();
                    tbRespuesta.Text = index.Row.ItemArray[2].ToString();
                }
                else
                {
                    tbCmd.Text = index.Row.ItemArray[1].ToString();
                    tbRuta.Text = index.Row.ItemArray[2].ToString();
                    tbRespuesta.Text = index.Row.ItemArray[3].ToString();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
            MostrarGrid("Social");
            tbRuta.IsEnabled = false;
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            var comboBoxItem = sender as ComboBoxItem;
            if (comboBoxItem != null)
            {
                string tag = comboBoxItem.Tag.ToString();

                switch (tag)
                {
                    case "Sociales":
                        btnS_Click(sender, e);
                        break;
                    case "Carpetas":
                        btnC_Click(sender, e);
                        break;
                    case "Aplicaciones":
                        btnA_Click(sender, e);
                        break;
                    case "PaginaWeb":
                        btnPaginasWebs_Click(sender, e);
                        break;
                    case "Textos":
                        BtnT_Click(sender, e);
                        break;
                    case "Discord":
                        btnDiscord_Click(sender, e);
                        break;
                }
            }
        }
    }
}