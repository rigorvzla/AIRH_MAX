using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.CPU
{
    public partial class LinealMD_2 : UserControl
    {
        public LinealMD_2()
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
            Dispatcher.Invoke(() => ButtonProgressAssist.SetMaximum(btnCPU, 100));
            Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(btnCPU, cpuM.CargaGeneral));
            Dispatcher.Invoke(() => txbCPUP.Text = Math.Round(cpuM.CargaGeneral, 0).ToString() + "%");
        }

        private void Inicio()
        {
            var cpu = DeviceInfoService.CPU();
            Dispatcher.Invoke(() => txbcpu.Text = "CPU " + "(" + cpu.Nucleos + " Nucleos)");
            Dispatcher.Invoke(() => gridTotal.ToolTip = cpu.Modelo);
        }

        private void btnCPU_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "taskmgr.exe";
            process.Start();
        }
    }
}
