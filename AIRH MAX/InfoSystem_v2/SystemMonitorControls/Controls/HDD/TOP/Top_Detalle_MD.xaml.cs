using Syroot.Windows.IO;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SystemMonitorControls.HDD.TOP
{
    public partial class Top_Detalle_MD : Window, IDisposable
    {
        #region Modelo sin RelayCommand
        public class FileModelView
        {
            public string Nombre { get; set; }
            public long Orden { get; set; }
            public string Ruta { get; set; }
            public string Peso { get; set; }
        }
        #endregion

        #region Campos y Propiedades
        private System.Timers.Timer _monitoringTimer;
        private bool _disposed = false;
        private const int UPDATE_INTERVAL = 10000; // 10 segundos
        private const int FILE_COUNT = 10; // Top 10 archivos
        #endregion

        public Top_Detalle_MD()
        {
            InitializeComponent();
        }

        #region Monitorización y Actualización
        private void StartMonitoring()
        {
            _monitoringTimer = new System.Timers.Timer(UPDATE_INTERVAL);
            _monitoringTimer.Elapsed += async (sender, e) => await LoadTopFilesAsync();
            _monitoringTimer.Start();

            // ✅ Carga inicial inmediata
            _ = LoadTopFilesAsync();
        }

        private async Task LoadTopFilesAsync()
        {
            try
            {
                var topFiles = await Task.Run(() => GetTopLargeFiles(FILE_COUNT));
                await UpdateFileListAsync(topFiles);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando archivos grandes: {ex.Message}");
            }
        }

        private async Task UpdateFileListAsync(List<FileModelView> files)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateFileListView(files);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando lista de archivos: {ex.Message}");
                }
            });
        }

        private void UpdateFileListView(List<FileModelView> files)
        {
            ListaProcesos.Items.Clear();

            foreach (var file in files)
            {
                ListaProcesos.Items.Add(file);
            }

            Debug.WriteLine($"Archivos listados: {files.Count}");
        }
        #endregion

        #region Obtención de Archivos Grandes
        private List<FileModelView> GetTopLargeFiles(int count)
        {
            var topFiles = new List<FileModelView>();

            try
            {
                string[] directorios =
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    KnownFolders.Downloads.Path
                };

                var allLargeFiles = new List<FileModelView>();

                foreach (var directorio in directorios)
                {
                    try
                    {
                        if (!Directory.Exists(directorio)) continue;

                        var directoryInfo = new DirectoryInfo(directorio);
                        var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            try
                            {
                                var fileModel = new FileModelView
                                {
                                    Nombre = file.Name,
                                    Peso = NormalizeFileSize(file.Length),
                                    Ruta = file.FullName,
                                    Orden = file.Length
                                };
                                allLargeFiles.Add(fileModel);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error procesando archivo {file.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error accediendo directorio {directorio}: {ex.Message}");
                    }
                }

                // ✅ OBTENER TOP ARCHIVOS MÁS GRANDES
                topFiles = allLargeFiles
                    .OrderByDescending(f => f.Orden)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetTopLargeFiles: {ex.Message}");
            }

            return topFiles;
        }
        #endregion

        #region Eventos de UI
        private void ListaProcesos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ListaProcesos.SelectedItem is FileModelView selectedFile)
                {
                    OpenFileLocation(selectedFile.Ruta);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir ubicación del archivo: {ex.Message}");
                MessageBox.Show("Error al abrir la ubicación del archivo", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFileLocation(string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                else
                {
                    MessageBox.Show("La ruta del archivo no existe", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error abriendo ubicación: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Eventos de Ventana
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ AGREGAR EVENTO DE DOBLE CLIC
            ListaProcesos.MouseDoubleClick += ListaProcesos_MouseDoubleClick;

            StartMonitoring();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error moviendo ventana: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Utilidades
        private static string NormalizeFileSize(double fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.#} {units[unit]}";
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _monitoringTimer?.Stop();
                _monitoringTimer?.Dispose();

                // ✅ REMOVER EVENT HANDLERS
                ListaProcesos.MouseDoubleClick -= ListaProcesos_MouseDoubleClick;
            }
        }
        #endregion

    }
}