using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.RAM.Plus;

namespace SystemMonitorControls.RAM
{
    public partial class DetalladoMD : UserControl, IDisposable
    {
        #region Campos y Propiedades
        private Alarma.Alarma_Detallado_MD alarma = null;
        private Plus_Detallado_MD plus = null;
        private TOP.Top_Detallado_MD top = null;
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
                await InitializeStaticDataAsync();
                StartMonitoring();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UserControl_Loaded: {ex.Message}");
            }
        }

        private async Task InitializeStaticDataAsync()
        {
            try
            {
                var ramDevices = await Task.Run(() => DeviceInfoService.RAM());
                await UpdateRamStaticInfoAsync(ramDevices);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en InitializeStaticDataAsync: {ex.Message}");
            }
        }

        private async Task UpdateRamStaticInfoAsync(List<InfoSystem_v2.Models.RAM> ramDevices)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var validRam = ramDevices.FirstOrDefault(item =>
                        !item.Tipo.Equals("Unknown", StringComparison.OrdinalIgnoreCase));

                    if (validRam != null)
                    {
                        txbRAMTitulo.Text = $"RAM ({validRam.Tipo})";
                        gridTotal.ToolTip = $"Vel {validRam.Velocidad} MHz";
                    }
                    else
                    {
                        // ✅ FALLBACK SI NO HAY RAM VÁLIDA
                        txbRAMTitulo.Text = "RAM";
                        gridTotal.ToolTip = "Información de RAM no disponible";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateRamStaticInfoAsync: {ex.Message}");
                }
            });
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
                        await UpdateRamUsageAsync();
                        await Task.Delay(3000, _monitoringTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en monitorización RAM: {ex.Message}");
                        await Task.Delay(5000); // ✅ ESPERAR MÁS EN CASO DE ERROR
                    }
                }
            }, _monitoringTokenSource.Token);
        }

        private async Task UpdateRamUsageAsync()
        {
            try
            {
                var ramUsage = await Task.Run(() => MonitorService.RAM());
                await UpdateRamUIAsync(ramUsage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateRamUsageAsync: {ex.Message}");
            }
        }

        private async Task UpdateRamUIAsync(DeviceMonitor.RAM ramUsage)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateRamProgressBar(ramUsage);
                    UpdateRamUsageInfo(ramUsage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateRamUIAsync: {ex.Message}");
                }
            });
        }
        #endregion

        #region Actualización de UI - SOLO CORREGIDA LA BARRA DE PROGRESO
        private void UpdateRamProgressBar(DeviceMonitor.RAM ramUsage)
        {
            // ✅ CORRECCIÓN: Validar valores antes de asignar
            if (ramUsage.Total > 0)
            {
                ButtonProgressAssist.SetMaximum(btnRAM, ramUsage.Total);
                ButtonProgressAssist.SetValue(btnRAM, ramUsage.Actual);

                // ✅ CORRECCIÓN: Calcular porcentaje si viene 0
                double porcentaje = ramUsage.Porcentaje;
                if (porcentaje <= 0)
                {
                    porcentaje = (ramUsage.Actual / ramUsage.Total) * 100;
                }
                txbRAMP.Text = $"{Math.Round(porcentaje, 0)}%";
            }
            else
            {
                // ✅ CORRECCIÓN: Valores por defecto si hay error
                ButtonProgressAssist.SetMaximum(btnRAM, 100);
                ButtonProgressAssist.SetValue(btnRAM, 0);
                txbRAMP.Text = "0%";
            }
        }

        private void UpdateRamUsageInfo(DeviceMonitor.RAM ramUsage)
        {
            // ✅ MANTENER EL CÓDIGO ORIGINAL PERO CORREGIR LA CONVERSIÓN
            // Convertir MB a GB correctamente (1 GB = 1024 MB)
            //var usedMemoryGB = ramUsage.Actual / 1024.0;
            //var totalMemoryGB = ramUsage.Total / 1024.0;

            // Formatear con 1 decimal
            var usedMemory = $"{NormalizeFileSize(ramUsage.Actual):0.#} GB";
            var totalMemory = $"{NormalizeFileSize(ramUsage.Total):0.#} GB";

            txbRAM.Text = $"{usedMemory.Replace("MB", "")} Usado{Environment.NewLine}{totalMemory.Replace("MB", "")} Total";
        }
        #endregion

        #region Gestión de Ventanas
        private void Button_Alarma(object sender, RoutedEventArgs e)
        {
            try
            {
                if (alarma == null)
                {
                    alarma = new Alarma.Alarma_Detallado_MD();
                    alarma.Closed += (a, b) => alarma = null;
                    alarma.Show();
                }
                else
                {
                    alarma.Show();
                    alarma.Activate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir alarma RAM: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (plus == null)
                {
                    plus = new Plus_Detallado_MD();
                    plus.Closed += (a, b) => plus = null;
                    plus.Show();
                }
                else
                {
                    plus.Show();
                    plus.Activate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir plus RAM: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (top == null)
                {
                    top = new TOP.Top_Detallado_MD();
                    top.Closed += (a, b) => top = null;
                    top.Show();
                }
                else
                {
                    top.Show();
                    top.Activate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir top RAM: {ex.Message}");
            }
        }
        #endregion

        #region Eventos de UI
        private void btnRAM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir taskmgr: {ex.Message}");
                MessageBox.Show("No se pudo abrir el Administrador de tareas",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Utilidades
        private static string NormalizeFileSize(double fileSize)
        {
            if (fileSize <= 0) return "0 B";

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

                alarma?.Close();
                plus?.Close();
                top?.Close();

                _isMonitoring = false;
            }
        }
        #endregion
    }
}