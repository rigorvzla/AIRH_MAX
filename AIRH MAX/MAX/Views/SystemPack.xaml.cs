using InfoSystem_v2.Services;
using System.Windows;
using System.Windows.Input;

namespace MAX.Views
{
    public partial class SystemPack : Window
    {
        public SystemPack()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // ✅ CERRAR USANDO EL SINGLETON
            HardwareMonitorEngine.Instance.MonitorClose();
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ ABRIR USANDO EL SINGLETON
            HardwareMonitorEngine.Instance.MonitorOpen();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void cbActivar_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }
    }
}
