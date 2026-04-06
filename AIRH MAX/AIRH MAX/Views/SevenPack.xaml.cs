using AIRH_MAX.ClassView.Services;
using System.IO;
using System.Windows;

namespace AIRH_MAX.Views
{
    public partial class SevenPack : Window, ICompressionProgress
    {
        private readonly CompressionService _compressionService;
        private readonly int _valorInterno;
        private readonly string _sourceName;
        string pathObtain = ClassView.Engrane.ElementPath();

        public SevenPack(int valor)
        {
            InitializeComponent();
            _valorInterno = valor;
            _sourceName = pathObtain;
            _compressionService = new CompressionService(this);
        } 

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (_valorInterno)
                {
                    case 1:
                        if (Directory.Exists(_sourceName))
                        {
                            await _compressionService.CompressDirectoryAsync(_sourceName);
                        }
                        else if (File.Exists(_sourceName))
                        {
                            await _compressionService.CompressFileAsync(_sourceName);
                        }
                        break;
                    case 3:
                        var pass = Microsoft.VisualBasic.Interaction.InputBox("Ingrese contraseña en caso de ser necesario", "AV-AIRH MAX", "");
                        await _compressionService.ExtractArchiveAsync(_sourceName, pass);
                        break;
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }

        // Implementación de ICompressionProgress
        public void UpdateMessage(string message)
        {
            Dispatcher.Invoke(() => lbItems.Items.Add($"{message}{Environment.NewLine}"));
        }

        public void UpdateFileProgress(double percentage)
        {
            Dispatcher.Invoke(() => pbUnidad.Value = percentage);
        }

        public void UpdateTotalProgress(double percentage)
        {
            Dispatcher.Invoke(() => pbTotal.Value = percentage);
        }

        // Eventos de UI (sin cambios)
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