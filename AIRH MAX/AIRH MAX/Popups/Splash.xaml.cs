// Views/Splash.xaml.cs
using AIRH_MAX.Services;
using System.Windows;

namespace AIRH_MAX.Popups
{
    public partial class Splash : Window, IStartupProgress
    {
        private readonly StartupService _startupService;

        public Splash()
        {
            InitializeComponent();
            _startupService = new StartupService(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _startupService.InitializeAsync();
            AIRH_Start();
        }

        public void UpdateProgress(int percentage, string message)
        {
            // Actualizar UI desde cualquier hilo
            Dispatcher.Invoke(() =>
            {
                ARC1.EndAngle = percentage * 3.6;
                LabelPorcentaje.Text = percentage.ToString();
                LabelCarga.Text = message ?? "Cargando...";
            });
        }

        private void AIRH_Start()
        {
            _startupService.FinalizeStartup();

            var mainWindow = new Views.MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}