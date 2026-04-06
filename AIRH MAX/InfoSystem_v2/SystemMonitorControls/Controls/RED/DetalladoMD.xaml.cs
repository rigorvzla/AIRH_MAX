using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.RED
{
    public partial class DetalladoMD : UserControl, IDisposable
    {
        #region Campos y Configuración
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly PeriodicTimer _updateTimer;
        private readonly SemaphoreSlim _updateSemaphore = new(1, 1);
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);

        private readonly ConcurrentDictionary<string, object> _configCache = new();

        private bool _disposed = false;
        private bool _isMonitoring = false;
        #endregion

        #region Propiedades y Campos Originales
        public static bool progressbar = true;
        private Alarma.Alarma_Detallado_MD alarma = null;
        private TOP.Top_DetalladoMD top = null;
        #endregion

        public DetalladoMD()
        {
            InitializeComponent();
            _updateTimer = new PeriodicTimer(_updateInterval);
        }

        #region Inicialización y Monitorización Asíncrona
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
            await StartMonitoringAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ButtonProgressAssist.SetMaximum(btnRED, 100);
                    ButtonProgressAssist.SetValue(btnRED, 0);
                    txbREDP.Text = "0%";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en inicialización: {ex.Message}");
            }
        }

        private async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            try
            {
                while (await _updateTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    await UpdateNetworkDataAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Monitorización de RED cancelada limpiamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en monitorización: {ex.Message}");
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task UpdateNetworkDataAsync(CancellationToken cancellationToken = default)
        {
            if (!await _updateSemaphore.WaitAsync(0, cancellationToken))
                return;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var networkData = await GetNetworkDataAsync(cancellationToken);
                if (networkData != null)
                {
                    await UpdateUIAsync(networkData, cancellationToken);
                    await UpdateSmartMeterAsync(networkData, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando datos de red: {ex.Message}");
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async Task<DeviceMonitor.RED> GetNetworkDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() => MonitorService.RED(), cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo datos de red: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Actualización de UI Optimizada
        private async Task UpdateUIAsync(DeviceMonitor.RED networkData, CancellationToken cancellationToken = default)
        {
            var downloadSpeed = networkData.VelocidadDescarga ?? 0;
            var uploadSpeed = networkData.VelocidadSubida ?? 0;
            var networkName = networkData.NombreRed ?? "Desconocida";

            var percentage = await CalculatePercentageAsync(downloadSpeed, uploadSpeed, cancellationToken);

            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // ✅ MANTENER FUNCIONALIDAD ORIGINAL DE UI
                    txbred.Text = "RED (" + networkName + ")";
                    txbUP.Text = "Subida: " + NormalizeFileSize(Convert.ToInt64(uploadSpeed));
                    txbDown.Text = "Descarga: " + NormalizeFileSize(Convert.ToInt64(downloadSpeed));

                    ButtonProgressAssist.SetValue(btnRED, Math.Round(percentage, 0));
                    txbREDP.Text = Math.Round(percentage, 0) + "%";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando UI: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background, cancellationToken);
        }

        private async Task<double> CalculatePercentageAsync(double downloadSpeed, double uploadSpeed, CancellationToken cancellationToken)
        {
            try
            {
                double a, b;

                if (progressbar)
                {
                    a = downloadSpeed;
                    b = await GetMaxSpeedAsync(true, cancellationToken);
                }
                else
                {
                    a = uploadSpeed;
                    b = await GetMaxSpeedAsync(false, cancellationToken);
                }

                if (b <= 0) return 0;

                var porcentajeRed = (a / b) * 100;
                return Math.Min(porcentajeRed, 100); // ✅ LIMITAR A 100%
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculando porcentaje: {ex.Message}");
                return 0;
            }
        }

        private async Task<double> GetMaxSpeedAsync(bool isDownload, CancellationToken cancellationToken)
        {
            const string downloadKey = "MaxDownloadSpeed";
            const string uploadKey = "MaxUploadSpeed";

            try
            {
                string cacheKey = isDownload ? downloadKey : uploadKey;

                if (_configCache.TryGetValue(cacheKey, out var cachedValue))
                {
                    return (double)cachedValue;
                }

                var maxSpeed = await Task.Run(() =>
                {
                    var speed = isDownload ?
                        Configuracion.Propiedades.CargarPropiedad.Vel_DescargaRED() :
                        Configuracion.Propiedades.CargarPropiedad.Vel_SubidaRED();

                    return Convert.ToDouble(speed, CultureInfo.InvariantCulture);
                }, cancellationToken);

                _configCache[cacheKey] = maxSpeed;
                return maxSpeed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo velocidad máxima: {ex.Message}");
                return 100.0;
            }
        }
        #endregion

        #region Medidor Inteligente Optimizado (Manteniendo lógica original)
        private async Task UpdateSmartMeterAsync(DeviceMonitor.RED networkData, CancellationToken cancellationToken)
        {
            try
            {
                var downloadSpeed = networkData.VelocidadDescarga ?? 0;
                var uploadSpeed = networkData.VelocidadSubida ?? 0;

                // ✅ MANTENER LÓGICA ORIGINAL PERO DE FORMA ASÍNCRONA
                if (downloadSpeed > await GetMaxSpeedAsync(true, cancellationToken))
                {
                    await Task.Run(() =>
                    {
                        Configuracion.Propiedades.GuardarPropiedad.Descarga_TestRED(downloadSpeed.ToString(CultureInfo.InvariantCulture));
                    }, cancellationToken);

                    _configCache["MaxDownloadSpeed"] = downloadSpeed;
                    Debug.WriteLine($"Nueva velocidad máxima de descarga: {downloadSpeed}");
                }

                if (uploadSpeed > await GetMaxSpeedAsync(false, cancellationToken))
                {
                    await Task.Run(() =>
                    {
                        Configuracion.Propiedades.GuardarPropiedad.Subida_TestRED(uploadSpeed.ToString(CultureInfo.InvariantCulture));
                    }, cancellationToken);

                    _configCache["MaxUploadSpeed"] = uploadSpeed;
                    Debug.WriteLine($"Nueva velocidad máxima de subida: {uploadSpeed}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en medidor inteligente: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers Originales (Manteniendo funcionalidad)
        private void Button_Alarma(object sender, RoutedEventArgs e)
        {
            // ✅ MANTENER COMPORTAMIENTO ORIGINAL
            if (alarma == null)
            {
                alarma = new Alarma.Alarma_Detallado_MD();
                alarma.Closed += (a, b) => alarma = null;
                alarma.Show();
            }
            else
            {
                alarma.Show();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // ✅ MANTENER COMPORTAMIENTO ORIGINAL
            progressbar = !progressbar;
            if (progressbar)
            {
                signalIcon.Kind = PackIconKind.ArrowDownBold;
                progressbar = true;
            }
            else
            {
                signalIcon.Kind = PackIconKind.ArrowUpBold;
                progressbar = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // ✅ MANTENER COMPORTAMIENTO ORIGINAL
            if (top == null)
            {
                top = new TOP.Top_DetalladoMD();
                top.Closed += (a, b) => top = null;
                top.Show();
            }
            else
            {
                top.Show();
            }
        }

        private void btnCPU_Click(object sender, RoutedEventArgs e)
        {
            // ✅ MANTENER COMPORTAMIENTO ORIGINAL
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "ms-settings:network-status";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error abriendo configuración de red: {ex.Message}");
            }
        }
        #endregion

        #region Métodos de Utilidad (Manteniendo formato original)
        private static string NormalizeFileSize(double fileSize)
        {
            // ✅ MANTENER MÉTODO ORIGINAL PERO CON MANEJO DE ERRORES
            try
            {
                string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                double size = fileSize;
                var unit = 0;

                while (size >= 1024)
                {
                    size /= 1024;
                    ++unit;
                }

                return $"{size:0.#} {units[unit]}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error normalizando tamaño de archivo: {ex.Message}");
                return "0 B";
            }
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _updateTimer?.Dispose();
                _updateSemaphore?.Dispose();
                _configCache.Clear();

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~DetalladoMD()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Dispose();
                    _updateTimer?.Dispose();
                    _updateSemaphore?.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion
    }
}