using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.RAM
{
    public partial class LinealMD_2 : UserControl
    {
        public LinealMD_2()
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
            Dispatcher.Invoke(() => ButtonProgressAssist.SetMaximum(btnRAM, ram.Total));
            Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(btnRAM, ram.Actual));

            Dispatcher.Invoke(() => txbRAMp.Text = Math.Round(ram.Porcentaje, 0) + "%");

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

        private void btnRAM_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "taskmgr.exe";
            process.Start();
        }
    }
}
