using InfoSystem_v2.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SystemMonitorControls.RAM
{
    public partial class Lineal : UserControl
    {
        public Lineal()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Inicio();
            while (true)
            {
                await Task.Run(() =>
                      {
                          Monitor();
                          Task.Delay(3000).Wait();
                      });

            }
        }

        private void Monitor()
        {
            var ram = MonitorService.RAM();
            Dispatcher.Invoke(() => PB.Maximum = ram.Total);
            Dispatcher.Invoke(() => PB.Value = ram.Actual);

            if (ram.Porcentaje < 50)
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.DarkBlue));
            }
            else if (ram.Porcentaje > 51 && ram.Porcentaje < 75)
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.Yellow));
            }
            else
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.Red));
            }

            if (ram.Porcentaje != 0)
            {
                Dispatcher.Invoke(() => txbRAMp.Text = Math.Round(ram.Porcentaje, 0) + "%");
                Dispatcher.Invoke(() => txbRAM.Text = NormalizeFileSize(ram.Actual).Replace("MB", "GB") + " Usado" + Environment.NewLine + NormalizeFileSize(ram.Total).Replace("MB", "GB") + " Total");
            }
        }

        private void Inicio()
        {
            var ram = DeviceInfoService.RAM();
            foreach (var item in ram)
            {
                if (item.Tipo.ToLower().Equals("unknown"))
                {
                    continue;
                }

                Dispatcher.Invoke(() => txbRAMTitulo.Text = "RAM " + "(" + item.Tipo + ")");
                Dispatcher.Invoke(() => gridTotal.ToolTip = "Vel " + item.Velocidad + " MHz");
                break;
            }           
        }

        private static string NormalizeFileSize(double fileSize)
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
    }
}
