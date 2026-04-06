using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.Controls.HDD;
using SystemMonitorControls.HDD.Plus;

namespace SystemMonitorControls.HDD
{
    public partial class CircularMD : UserControl
    {
        public CircularMD()
        {
            InitializeComponent();
        }


        private void Iniciar()
        {
            var hdd = DeviceInfoService.Storage();

            foreach (var item in hdd)
            {
                if (!item.Principal.Equals(true))
                {
                    continue;
                }

                Dispatcher.Invoke(() => txbTitulo.Text = $@"Disco ({item.Tipo})");

                Dispatcher.Invoke(() => gridTotal.ToolTip = item.Modelo);

                string lip = item.TamañoTotal.Replace("GB", "").Trim();
                double max = Convert.ToDouble(lip);
                Dispatcher.Invoke(() => ButtonProgressAssist.SetMaximum(btnMD, max));

                string set = item.EspacioUsado.Replace("GB", "").Trim();
                double value = Convert.ToDouble(set);
                Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(btnMD, value));


                double espacio = value / max * 100.0;
                Dispatcher.Invoke(() => txbHDDp.Text = espacio.ToString().Substring(0, 2) + "%");
                Dispatcher.Invoke(() => txbHDD.Text = item.EspacioUsado.Replace("GB", "") + "/" + item.EspacioLibre);

            }
        }

        private void CambioUnidad(string Unidad)
        {
            var hdd = DeviceInfoService.Storage();

            foreach (var item in hdd)
            {
                if (!Unidad.Equals(item.Unidad))
                {
                    continue;
                }

                Dispatcher.Invoke(() => txbTitulo.Text = $@"Disco ({item.Tipo})");
                Dispatcher.Invoke(() => gridTotal.ToolTip = item.Modelo);

                string lip = item.TamañoTotal.Replace("GB", "").Trim();
                double max = Convert.ToDouble(lip);
                Dispatcher.Invoke(() => ButtonProgressAssist.SetMaximum(btnMD, max));

                string set = item.EspacioUsado.Replace("GB", "").Trim();
                double value = Convert.ToDouble(set);
                Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(btnMD, value));

                double espacio = value / max * 100.0;
                Dispatcher.Invoke(() => txbHDDp.Text = espacio.ToString().Substring(0, 2) + "%");
                Dispatcher.Invoke(() => txbHDD.Text = item.EspacioUsado.Replace("GB", "") + "/" + item.EspacioLibre);

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (HDD_Engine.Seguro.Equals(false))
                    {
                        Iniciar();
                    }
                    else if (HDD_Engine.Seguro.Equals(true))
                    {
                        CambioUnidad(HDD_Engine.HDD);

                    }
                    Task.Delay(5000).Wait();
                }
            });
        }

        public static string NormalizeFileSize(double fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        private void btnMD_Click(object sender, RoutedEventArgs e)
        {
            Plus_Detallado_MD plus = new Plus_Detallado_MD();
            plus.ShowDialog();
        }
    }
}
