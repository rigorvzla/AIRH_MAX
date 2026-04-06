using InfoSystem_v2.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SystemMonitorControls.CPU
{
    public partial class Lineal : UserControl
    {
        public Lineal()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Inicio();

            Task.Run(() =>
            {
                while (true)
                {
                    Monitor();
                    Task.Delay(3000).Wait();
                }
            });
        }

        private void Monitor()
        {
            var cpuM = MonitorService.CPU();

            if (cpuM.CargaGeneral < 50)
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.DarkBlue));
            }
            else if (cpuM.CargaGeneral > 51 && cpuM.CargaGeneral < 75)
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.Yellow));
            }
            else
            {
                Dispatcher.Invoke(() => PB.Foreground = new SolidColorBrush(Colors.Red));
            }

            Dispatcher.Invoke(() => txbCPU.Text = cpuM.MHzUsado.ToString() + " MHz Usado" + Environment.NewLine + cpuM.MHzTotal.ToString() + " MHz Total");
            Dispatcher.Invoke(() => PB.Value = cpuM.CargaGeneral);
        }

        private void Inicio()
        {
            var cpu = DeviceInfoService.CPU();        
            Dispatcher.Invoke(() => txbcpu.Text = "CPU " + "(" + cpu.Nucleos + " Nucleos)");
            Dispatcher.Invoke(() => gridTotal.ToolTip = cpu.Modelo);            
        }
    }
}
