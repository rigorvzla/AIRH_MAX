// Views/YouTubePack.xaml.cs
using AIRH_MAX.ClassView.Services;
using AIRH_MAX.Models;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace AIRH_MAX.Views
{
    public partial class YouTubePack : Window, IYouTubeDownloadProgress
    {
        private readonly YouTubeDownloadService _downloadService;
        private readonly int _valorInterno;
        private readonly string _id;
        private readonly string _rutaMedia;
        private readonly string _nameIA;

        public YouTubePack(int valor, string id, string rutaMedia, string nameIA)
        {
            InitializeComponent();
            _valorInterno = valor;
            _id = id;
            _rutaMedia = rutaMedia;
            _nameIA = nameIA;
            _downloadService = new YouTubeDownloadService(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_valorInterno)
                {
                    case 1:
                        await _downloadService.DownloadAudioAsync(_id, _rutaMedia, _nameIA, isSearch: false);
                        MainWindow.NotificacionEvent.MensajeBox = "Descarga finalizada";
                        break;
                    case 2:
                        await _downloadService.DownloadVideoAsync(_id, _rutaMedia, _nameIA, isSearch: false);
                        break;
                    case 3:
                        await _downloadService.DownloadAudioAsync(_id, _rutaMedia, _nameIA, isSearch: true);
                        MainWindow.NotificacionEvent.MensajeBox = "Descarga finalizada";
                        break;
                    case 4:
                        await _downloadService.DownloadVideoAsync(_id, _rutaMedia, _nameIA, isSearch: true);
                        MainWindow.NotificacionEvent.MensajeBox = "Descarga finalizada";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning); 
                Close();
            }
        }

        // Implementación de IYouTubeDownloadProgress
        public void UpdateDownloadProgress(double percentage)
        {
            Dispatcher.Invoke(() => pbDown.Value = percentage);
        }

        public void UpdateConversionProgress(double percentage)
        {
            Dispatcher.Invoke(() => pbConver.Value = percentage);
        }

        public void AddLogMessage(string message, BitmapImage thumbnail = null)
        {
            Dispatcher.Invoke(() =>
            {
                var viewModel = new Models.YouTubeModel
                {
                    Nombre_IA = _nameIA,
                    Tiempo = DateTime.Now.ToShortTimeString(),
                    Mensaje = message,
                    ImageSourceDescarga = thumbnail,
                    ImageSource = "/PNG/AIRH.png"
                };

                lbItems.Items.Add(viewModel);
                lbItems.Items.MoveCurrentToLast();
                lbItems.ScrollIntoView(lbItems.Items.CurrentItem);
            });
        }

        // Eventos de UI (sin cambios)
        private void Button_Click_1(object sender, RoutedEventArgs e) => Close();
        private void Button_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void lbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbItems.SelectedItem is YouTubeModel item && File.Exists(item.Mensaje))
            {
                Process.Start(new ProcessStartInfo(item.Mensaje) { UseShellExecute = true });
            }
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}