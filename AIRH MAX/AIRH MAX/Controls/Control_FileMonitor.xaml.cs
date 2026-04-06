using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Properties;
using Newtonsoft.Json;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_FileMonitor : UserControl
    {
        public Control_FileMonitor()
        {
            InitializeComponent();
        }

        private void guardar_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEnlace.Text == string.Empty)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Seleccione una ruta para guardar", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var file = new FileWatch();
            file.Ruta = TextBoxEnlace.Text;
            file.Eliminado = (bool)cbEliminar.IsChecked;
            file.Cambiado = (bool)cbCambiado.IsChecked;
            file.Renombrado = (bool)cbRenombrado.IsChecked;
            file.Creado = (bool)cbCreado.IsChecked;

            string json = JsonConvert.SerializeObject(file);

            if (!Settings.Default.MonitorFile.Contains(json))
            {
                Settings.Default.MonitorFile.Add(json);
                Settings.Default.Save();

                ClassView.FileWatcher.MonitorFile();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Ruta existente", "AV-AIRH MAX", MessageBoxButton.OK,MessageBoxImage.Warning);
            }

            lbRutas.Items.Clear();
            foreach (var item in Settings.Default.MonitorFile)
            {
                if (!item.Equals(string.Empty))
                {
                    FileWatch f = JsonConvert.DeserializeObject<FileWatch>(item);
                    lbRutas.Items.Add(f.Ruta);
                }
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Settings.Default.MonitorFile)
            {
                if (!item.Equals(string.Empty))
                {
                    FileWatch f = JsonConvert.DeserializeObject<FileWatch>(item);
                    lbRutas.Items.Add(f.Ruta);
                }
            }
        }

        private void eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (lbRutas.SelectedIndex != -1)
            {
                Settings.Default.MonitorFile.RemoveAt(lbRutas.SelectedIndex + 1);
                Settings.Default.Save();
                lbRutas.Items.Clear();

                foreach (var item in Settings.Default.MonitorFile)
                {
                    if (!item.Equals(string.Empty))
                    {
                        FileWatch f = JsonConvert.DeserializeObject<FileWatch>(item);
                        lbRutas.Items.Add(f.Ruta);
                    }
                }
            }
        }

        private void btnCaptura_Click(object sender, RoutedEventArgs e)
        {
            TextBoxEnlace.Text = ClassView.Support.BuscarCarpeta();
        }
    }
}
