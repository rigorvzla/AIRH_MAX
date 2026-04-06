using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.CPU.TOP;

namespace SystemMonitorControls.CPU
{
    public partial class DetalladoMD : UserControl, IDisposable
    {
        #region Campos y Propiedades
        private Alarma.Alarma_Detallado_MD alarma = null;
        private Plus.Plus_Detallado_MD plus = null;
        private Top_Detallado_MD top = null;
        private CancellationTokenSource _monitoringTokenSource;
        private bool _isDisposed = false;
        private bool _isMonitoring = false;
        #endregion

        public DetalladoMD()
        {
            InitializeComponent();
            _monitoringTokenSource = new CancellationTokenSource();
        }

        #region Inicialización y Monitorización
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ INICIALIZAR DATOS ESTÁTICOS
                Inicio();

                // ✅ INICIAR MONITORIZACIÓN EN SEGUNDO PLANO
                StartMonitoring();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UserControl_Loaded: {ex.Message}");
            }
        }

        private void Inicio()
        {
            try
            {
                // ✅ CARGAR DATOS ESTÁTICOS UNA SOLA VEZ
                var cpu = DeviceInfoService.CPU();

                // ✅ ACTUALIZAR UI
                txbcpu.Text = $"CPU ({cpu.Nucleos} Nucleos)";
                gridTotal.ToolTip = cpu.Modelo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Inicio: {ex.Message}");
            }
        }

        private void StartMonitoring()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            // ✅ MONITORIZACIÓN EN SEGUNDO PLANO CON CONTROL
            Task.Run(async () =>
            {
                while (!_monitoringTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await UpdateMonitorDataAsync();
                        await Task.Delay(3000, _monitoringTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // ✅ MONITORIZACIÓN CANCELADA NORMALMENTE
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en monitorización: {ex.Message}");
                        await Task.Delay(5000); // ✅ ESPERAR MÁS EN CASO DE ERROR
                    }
                }
            }, _monitoringTokenSource.Token);
        }

        private async Task UpdateMonitorDataAsync()
        {
            try
            {
                // ✅ OBTENER DATOS DE MONITORIZACIÓN
                var cpuM = await Task.Run(() => MonitorService.CPU());

                // ✅ ACTUALIZAR UI DE FORMA EFICIENTE
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateUI(cpuM);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateMonitorDataAsync: {ex.Message}");
            }
        }

        private void UpdateUI(DeviceMonitor.CPU cpuM)
        {
            try
            {
                // ✅ CONFIGURACIÓN DE PROGRESO
                ButtonProgressAssist.SetMaximum(btnCPU, 100);
                ButtonProgressAssist.SetValue(btnCPU, cpuM.CargaGeneral);

                // ✅ ACTUALIZAR TEXTOS
                txbTemp.Text = $"{Math.Round(cpuM.TemperaturaGeneral, 0)}ºC";
                txbCPU.Text = $"{cpuM.MHzUsado:F0} MHz Usado{Environment.NewLine}{cpuM.MHzTotal:F0} MHz Total";
                txbCPUP.Text = $"{Math.Round(cpuM.CargaGeneral, 0)}%";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateUI: {ex.Message}");
            }
        }
        #endregion

        #region Gestión de Ventanas (MANTENIDO ORIGINAL)
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
                Debug.WriteLine($"Error al abrir alarma: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (plus == null)
                {
                    plus = new Plus.Plus_Detallado_MD();
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
            try
            {
                if (top == null)
                {
                    top = new Top_Detallado_MD();
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
                Debug.WriteLine($"Error al abrir top: {ex.Message}");
            }
        }
        #endregion

        #region Eventos de UI (MANTENIDO ORIGINAL)
        private void btnCPU_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ ABRIR ADMINISTRADOR DE TAREAS
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir taskmgr: {ex.Message}");
            }
        }
        #endregion

        #region Disposable Pattern
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                // ✅ DETENER MONITORIZACIÓN
                _monitoringTokenSource?.Cancel();
                _monitoringTokenSource?.Dispose();

                // ✅ CERRAR VENTANAS ABIERTAS
                alarma?.Close();
                plus?.Close();
                top?.Close();

                _isMonitoring = false;
            }
        }
        #endregion
    }
}