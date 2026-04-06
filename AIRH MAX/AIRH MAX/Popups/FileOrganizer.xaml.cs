using AIRH_MAX.ClassView.List;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Label = System.Windows.Controls.Label;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace AIRH_MAX.Popups
{
    public partial class FileOrganizer : Window
    {
        public FileOrganizer()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            txbDireccion.Text = BuscarCarpeta("Selecciona directorio");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txbDireccion.Text != string.Empty)
            {
                await Task.Run(() => OrganizerFile(Dispatcher.Invoke(() => txbDireccion.Text), FormatList.audioExtensions, pbMain, txbExt));
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (txbDireccion.Text != string.Empty)
            {
                await Task.Run(() => OrganizerFile(Dispatcher.Invoke(() => txbDireccion.Text), FormatList.videoExtensions, pbMain, txbExt));
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (txbDireccion.Text != string.Empty)
            {
                await Task.Run(() => OrganizerFile(Dispatcher.Invoke(() => txbDireccion.Text), FormatList.imageExtensions, pbMain, txbExt));
            }
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (txbDireccion.Text != string.Empty)
            {
                await Task.Run(() => OrganizerFile(Dispatcher.Invoke(() => txbDireccion.Text), FormatList.documentExtensions, pbMain, txbExt));
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (txbDireccion.Text != string.Empty)
            {
                foreach (var item in FormatList.Todo)
                {
                    await Task.Run(() => OrganizerFile(Dispatcher.Invoke(() => txbDireccion.Text), item, pbMain, txbExt, true));
                }
                txbExt.Dispatcher.Invoke(() => txbExt.Content = "Finalizado");
                Views.MainWindow.NotificacionEvent.MensajeBox = "Organización Finalizada";
            }
        }

        public static void OrganizerFile(string ruta, List<string> formatos, ProgressBar pb = null, Label txb = null, bool todo = false)
        {
            if (!Directory.Exists(ruta))
            {
                throw new DirectoryNotFoundException($"La ruta '{ruta}' no existe.");
            }

            foreach (var extension in formatos)
            {
                // Búsqueda nativa con .NET
                var files = Directory.GetFiles(ruta, "*" + extension, SearchOption.AllDirectories)
                                    .Select(f => new FileInfo(f))
                                    .ToList();

                if (files.Count == 0) continue;

                pb?.Dispatcher.Invoke(() => pb.Maximum = files.Count);

                double progress = 0;
                foreach (var file in files)
                {
                    // Crear carpeta con el nombre de la extensión (ej: ".MP3", ".PDF")
                    string targetDir = Path.Combine(ruta, file.Extension.TrimStart('.').ToUpper());

                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    txb?.Dispatcher.Invoke(() => txb.Content = $"Ordenando: {file.Extension}");

                    string targetPath = Path.Combine(targetDir, file.Name);

                    // Copiar y verificar antes de eliminar
                    File.Copy(file.FullName, targetPath, true);

                    if (File.Exists(targetPath))
                    {
                        File.Delete(file.FullName);
                    }

                    progress++;
                    UpdateProgressBar(pb, progress);
                }

                pb?.Dispatcher.Invoke(() => pb.Value = 0);
            }

            if (!todo)
            {
                txb?.Dispatcher.Invoke(() => txb.Content = "Finalizado");
                Views.MainWindow.NotificacionEvent.MensajeBox = "Organización Finalizada";
            }
        }

        private static void UpdateProgressBar(ProgressBar pb, double value)
        {
            if (pb != null)
            {
                pb.Dispatcher.Invoke(() => pb.Value = value, DispatcherPriority.Background);
            }
        }

        public static string BuscarCarpeta(string Titulo, bool NuevaCarpeta = false)
        {
            string Ruta = string.Empty;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = Titulo;
            fbd.ShowNewFolderButton = NuevaCarpeta;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Ruta = fbd.SelectedPath;
            }
            return Ruta;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}