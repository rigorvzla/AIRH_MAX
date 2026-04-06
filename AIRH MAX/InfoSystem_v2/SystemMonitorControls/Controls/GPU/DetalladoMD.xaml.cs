using InfoSystem_v2.Models; // ✅ AGREGAR ESTA DIRECTIVA
using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.GPU.Plus;

namespace SystemMonitorControls.GPU
{
    public partial class DetalladoMD : UserControl, IDisposable
    {
        #region Campos y Propiedades
        private Alarma.Alarma_DetalladoMD alarma = null;
        private Plus_Detallado_MD plus = null;
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
                var gpu = await Task.Run(() => DeviceInfoService.GPU());

                await Dispatcher.InvokeAsync(() =>
                {
                    txbGPUTitulo.Text = $"GPU ({gpu.Nombre})";
                    gridTotal.ToolTip = $"Vel {gpu.ClockCore} MHz";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en InitializeStaticDataAsync: {ex.Message}");
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
                        await UpdateGpuDataAsync();
                        await Task.Delay(3000, _monitoringTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en monitorización GPU: {ex.Message}");
                        await Task.Delay(5000);
                    }
                }
            }, _monitoringTokenSource.Token);
        }

        private async Task UpdateGpuDataAsync()
        {
            try
            {
                var gpu = await Task.Run(() => MonitorService.GPU());
                await UpdateGpuUIAsync(gpu);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateGpuDataAsync: {ex.Message}");
            }
        }

        private async Task UpdateGpuUIAsync(InfoSystem_v2.Models.GPU gpu) // ✅ CORREGIDO: InfoSystem_v2.Models.GPU
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateGpuProgressBar(gpu);
                    UpdateGpuMemoryInfo(gpu);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateGpuUIAsync: {ex.Message}");
                }
            });
        }
        #endregion

        #region Actualización de UI
        private void UpdateGpuProgressBar(InfoSystem_v2.Models.GPU gpu) // ✅ CORREGIDO: InfoSystem_v2.Models.GPU
        {
            if (gpu.MemoriaTotal.HasValue && gpu.MemoriaUsada.HasValue)
            {
                ButtonProgressAssist.SetMaximum(btnGPU, gpu.MemoriaTotal.Value);
                ButtonProgressAssist.SetValue(btnGPU, gpu.MemoriaUsada.Value);

                var percentage = (gpu.MemoriaUsada.Value / gpu.MemoriaTotal.Value) * 100;
                txbGPUP.Text = $"{Math.Round(percentage, 0)}%";
            }
            else
            {
                // ✅ RESET PARA GPU INTEGRADA
                ButtonProgressAssist.SetMaximum(btnGPU, 100);
                ButtonProgressAssist.SetValue(btnGPU, 0);
                txbGPUP.Text = "N/A";
            }
        }

        private void UpdateGpuMemoryInfo(InfoSystem_v2.Models.GPU gpu) // ✅ CORREGIDO: InfoSystem_v2.Models.GPU
        {
            if (gpu.MemoriaUsada.HasValue && gpu.MemoriaTotal.HasValue)
            {
                var usedMemory = NormalizeFileSize(gpu.MemoriaUsada.Value).Replace("MB", "GB");
                var totalMemory = NormalizeFileSize(gpu.MemoriaTotal.Value).Replace("MB", "GB");
                txbGPU.Text = $"{usedMemory} Usado{Environment.NewLine}{totalMemory} Total";
            }
            else
            {
                // ✅ GPU INTEGRADA
                txbGPU.Text = $"Memoria Integrada{Environment.NewLine}{gpu.MemoriaIntegrada}";
            }
        }
        #endregion

        #region Gestión de Ventanas
        private void Button_Alarma(object sender, RoutedEventArgs e)
        {
            try
            {
                if (alarma == null)
                {
                    alarma = new Alarma.Alarma_DetalladoMD();
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
                Debug.WriteLine($"Error al abrir alarma: {ex.Message}");
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
                Debug.WriteLine($"Error al abrir plus: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // ✅ CÓDIGO COMENTADO MANTENIDO
            //TOP.Top_Detallado_MD top = null;
            //if (top == null)
            //{
            //    top = new();
            //    top.Closed += (a, b) => top = null;
            //    top.Show();
            //}
            //else
            //{
            //    top.Show();
            //}
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

        #region Eventos de UI
        private void btnGPU_Click(object sender, RoutedEventArgs e)
        {
            // ✅ EVENTO VACÍO MANTENIDO
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

                _isMonitoring = false;
            }
        }
        #endregion
    }
}