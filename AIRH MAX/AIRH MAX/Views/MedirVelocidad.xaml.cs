// Views/MedirVelocidad.xaml.cs
using AIRH_MAX.ClassView.Services;
using System.Windows;
using System.Windows.Input;

namespace AIRH_MAX.Views
{
    public partial class MedirVelocidad : Window, ISpeedTestProgress
    {
        private readonly SpeedTestService _speedTestService;
        private readonly string _mode;

        public MedirVelocidad(string mode)
        {
            InitializeComponent();
            _mode = mode;
            _speedTestService = new SpeedTestService(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _speedTestService.RunSpeedTestAsync(_mode);
            }
            catch
            {
                // Los errores ya se muestran en UpdateMessage
            }
        }

        // Implementación de ISpeedTestProgress
        public void UpdateMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                lbItems.Items.Add(message + Environment.NewLine);
            });
        }

        public void UpdateProgress(double percentage)
        {
            Dispatcher.Invoke(() =>
            {
                pbTotal.Value = percentage;
            });
        }

        // Eventos de UI (sin cambios)
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}