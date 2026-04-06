using InfoSystem_v2.Services;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.INF
{
    public partial class Mini : UserControl
    {
        public Mini()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var so = SystemInfoService.Windows();
            txbTi.Text = "OS (Sistema)";
            lbItems.Items.Add("Mod: " + so.Modelo);
            lbItems.Items.Add("Arquitectura: " + so.Arquitectura);
            lbItems.Items.Add("Sistema: " + so.Sistema);
            lbItems.Items.Add("Compilación: " + so.Version);
            lbItems.Items.Add("Nº Compilación: " + so.NumCompilacion);
        }
    }
}
