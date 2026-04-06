using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.Controls.HDD;
using SystemMonitorControls.HDD.Alarma;
using SystemMonitorControls.HDD.Plus;
using SystemMonitorControls.HDD.TOP;

namespace SystemMonitorControls.HDD
{
    public partial class DetalladoMD : UserControl, IDisposable
    {
        #region Campos y Propiedades
        private string _rutaUnidad = string.Empty;
        private CancellationTokenSource _monitoringTokenSource;
        private bool _disposed = false;
        private bool _isMonitoring = false;
        #endregion

        public DetalladoMD()
        {
            InitializeComponent();
            _monitoringTokenSource = new CancellationTokenSource();
        }

        #region Inicialización y Monitorización
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StartMonitoring();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UserControl_Loaded: {ex.Message}");
            }
        }

        private void StartMonitoring()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            Task.Run(async () =>
            {
                while (!_monitoringTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await UpdateStorageDataAsync();
                        await Task.Delay(5000, _monitoringTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en monitorización HDD: {ex.Message}");
                        await Task.Delay(10000); // ✅ ESPERAR MÁS EN CASO DE ERROR
                    }
                }
            }, _monitoringTokenSource.Token);
        }

        private async Task UpdateStorageDataAsync()
        {
            try
            {
                if (HDD_Engine.Seguro)
                {
                    await CambioUnidadAsync(HDD_Engine.HDD);
                }
                else
                {
                    await IniciarAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateStorageDataAsync: {ex.Message}");
            }
        }
        #endregion

        #region Actualización de Datos
        private async Task IniciarAsync()
        {
            try
            {
                var storageDevices = await Task.Run(() => DeviceInfoService.Storage());
                var primaryStorage = storageDevices.FirstOrDefault(item => item.Principal);

                if (primaryStorage != null)
                {
                    await UpdateStorageUIAsync(primaryStorage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en IniciarAsync: {ex.Message}");
            }
        }

        private async Task CambioUnidadAsync(string unidad)
        {
            try
            {
                var storageDevices = await Task.Run(() => DeviceInfoService.Storage());
                var targetStorage = storageDevices.FirstOrDefault(item => item.Unidad.Equals(unidad, StringComparison.OrdinalIgnoreCase));

                if (targetStorage != null)
                {
                    await UpdateStorageUIAsync(targetStorage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CambioUnidadAsync: {ex.Message}");
            }
        }

        private async Task UpdateStorageUIAsync(InfoSystem_v2.Models.Storage storage)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    _rutaUnidad = storage.Unidad;

                    // ✅ ACTUALIZAR INFORMACIÓN BÁSICA
                    txbTitulo.Content = $"Disco ({storage.Tipo})";
                    gridTotal.ToolTip = storage.Modelo;

                    // ✅ ACTUALIZAR BARRA DE PROGRESO
                    UpdateProgressBar(storage);

                    // ✅ ACTUALIZAR INFORMACIÓN DE ESPACIO
                    UpdateSpaceInfo(storage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateStorageUIAsync: {ex.Message}");
                }
            });
        }

        private void UpdateProgressBar(InfoSystem_v2.Models.Storage storage)
        {
            if (double.TryParse(storage.TamañoTotal.Replace("GB", "").Trim(), out double maxSize) &&
                double.TryParse(storage.EspacioUsado.Replace("GB", "").Trim(), out double usedSize))
            {
                ButtonProgressAssist.SetMaximum(btnMD, maxSize);
                ButtonProgressAssist.SetValue(btnMD, usedSize);

                double percentage = (usedSize / maxSize) * 100.0;
                txbHDDP.Text = $"{percentage:F0}%";
            }
            else
            {
                // ✅ FALLBACK EN CASO DE ERROR DE CONVERSIÓN
                ButtonProgressAssist.SetMaximum(btnMD, 100);
                ButtonProgressAssist.SetValue(btnMD, 0);
                txbHDDP.Text = "N/A";
            }
        }

        private void UpdateSpaceInfo(InfoSystem_v2.Models.Storage storage)
        {
            txbHDD.Text = $"{storage.EspacioUsado} Usado{Environment.NewLine}{storage.EspacioLibre} Libre";
        }
        #endregion

        #region Gestión de Ventanas
        private void Button_Alarma(object sender, RoutedEventArgs e)
        {
            try
            {
                var alarmWindow = new Alarma_Detallado_MD();
                alarmWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir alarma HDD: {ex.Message}");
                MessageBox.Show("Error al abrir ventana de alarmas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var plusWindow = new Plus_Detallado_MD();
                plusWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir plus HDD: {ex.Message}");
                MessageBox.Show("Error al abrir ventana plus", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topWindow = new Top_Detalle_MD();
                topWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir top HDD: {ex.Message}");
                MessageBox.Show("Error al abrir ventana top", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Eventos de UI
        private void btnHDD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_rutaUnidad))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _rutaUnidad,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("No se ha detectado una unidad válida", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir unidad: {ex.Message}");
                MessageBox.Show($"Error al abrir la unidad: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _monitoringTokenSource?.Cancel();
                _monitoringTokenSource?.Dispose();
                _isMonitoring = false;
            }
        }
        #endregion
    }
}