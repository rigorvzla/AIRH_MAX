// AIRH_MAX/Views/ConvertMediaPack.xaml.cs
using AIRH_MAX.ClassView.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AIRH_MAX.Views
{
    public partial class ConvertMediaPack : Window
    {
        private readonly ConvertMedia _converter = new();
        private readonly string _sourcePath;
        private readonly int _mode;

        // Constructor solo almacena datos, NO hace trabajo
        public ConvertMediaPack(int mode, string sourcePath = "")
        {
            InitializeComponent();
            _mode = mode;
            _sourcePath = sourcePath;
        }

        // Método público asíncrono para iniciar la conversión
        public async Task StartConversionAsync()
        {
            var itemProgress = new Progress<string>(msg => lbItems.Items.Add(msg));
            var progress = new Progress<double>(value => pbTotal.Value = value);

            try
            {
                if (_mode == 1)
                {
                    await _converter.ConvertToMP3(_sourcePath, itemProgress, progress);
                }
                else if (_mode == 2)
                {
                    await _converter.ConvertToMP4(_sourcePath, itemProgress, progress); // Ajusta método igual que arriba
                }
                else if (_mode == 3)
                {
                    await _converter.ConvertToJPG(_sourcePath); // Este no necesita progreso
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ya no hacemos nada aquí. El trabajo lo inicia StartConversionAsync()
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower() == "ffmpeg")
                {
                    process.Kill();
                    break;
                }
            }

            Close();
        }

        private void lbItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lbItems.SelectedIndex != -1)
            {
                string ruta = lbItems.SelectedItem.ToString().Split('\n')[1];
                if (File.Exists(ruta))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = ruta;
                    startInfo.UseShellExecute = true;
                    Process.Start(startInfo);
                }
            }
        }
    }
}