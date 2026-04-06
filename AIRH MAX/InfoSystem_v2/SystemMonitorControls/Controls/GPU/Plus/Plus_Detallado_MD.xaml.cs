using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Diagnostics;
using System.Windows;

namespace SystemMonitorControls.GPU.Plus
{
    public partial class Plus_Detallado_MD : Window, IDisposable
    {
        #region Campos y Propiedades
        private System.Timers.Timer _monitoringTimer;
        private readonly List<double> _cargaLista = new();
        private readonly List<double> _temperaturaLista = new();
        private readonly List<string> _timeList = new();
        private int _tiempoSegundos = 10;
        private bool _disposed = false;
        private const int UPDATE_INTERVAL = 10000; // 10 segundos
        private const int MAX_DATA_POINTS = 30; // Límite para evitar crecimiento infinito
        #endregion

        public Plus_Detallado_MD()
        {
            InitializeComponent();
        }

        #region Monitorización y Actualización
        private void StartMonitoring()
        {
            _monitoringTimer = new System.Timers.Timer(UPDATE_INTERVAL);
            _monitoringTimer.Elapsed += async (sender, e) => await LoadGpuDataAsync();
            _monitoringTimer.Start();

            // ✅ Carga inicial inmediata
            _ = LoadGpuDataAsync();
        }

        private async Task LoadGpuDataAsync()
        {
            try
            {
                var gpuData = await Task.Run(() => MonitorService.GPU());
                await ProcessGpuDataAsync(gpuData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos GPU: {ex.Message}");
            }
        }

        private async Task ProcessGpuDataAsync(InfoSystem_v2.Models.GPU gpu)
        {
            try
            {
                var (carga, temperatura, etiquetaCarga, etiquetaTemperatura) = CalculateGpuMetrics(gpu);

                AddDataPoints(carga, temperatura);
                await UpdateChartAsync(carga, etiquetaCarga, temperatura, etiquetaTemperatura);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error procesando datos GPU: {ex.Message}");
            }
        }

        private (double carga, double temperatura, string etiquetaCarga, string etiquetaTemperatura) CalculateGpuMetrics(InfoSystem_v2.Models.GPU gpu)
        {
            if (gpu.Fisico)
            {
                // ✅ GPU DEDICADA
                double carga = 0;
                double temperatura = gpu.Temperatura ?? 0;

                if (gpu.MemoriaUsada.HasValue && gpu.MemoriaTotal.HasValue && gpu.MemoriaTotal.Value > 0)
                {
                    carga = (gpu.MemoriaUsada.Value / gpu.MemoriaTotal.Value) * 100;
                }

                return (Math.Round(carga, 0), temperatura, "Carga GPU (%)", "Temperatura (ºC)");
            }
            else
            {
                // ✅ GPU INTEGRADA
                return (0, 0, "GPU Integrada", "Sin datos de temperatura");
            }
        }

        private void AddDataPoints(double carga, double temperatura)
        {
            // ✅ AGREGAR NUEVOS DATOS
            _cargaLista.Add(carga);
            _temperaturaLista.Add(temperatura);
            _timeList.Add($"{_tiempoSegundos} Seg");
            _tiempoSegundos += 10;

            // ✅ MANTENER LÍMITE DE DATOS
            if (_cargaLista.Count > MAX_DATA_POINTS)
            {
                _cargaLista.RemoveAt(0);
                _temperaturaLista.RemoveAt(0);
                _timeList.RemoveAt(0);
            }
        }
        #endregion

        #region Actualización de Gráfica
        private async Task UpdateChartAsync(double carga, string etiquetaCarga, double temperatura, string etiquetaTemperatura)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateChart(carga, etiquetaCarga, temperatura, etiquetaTemperatura);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando gráfica: {ex.Message}");
                }
            });
        }

        private void UpdateChart(double carga, string etiquetaCarga, double temperatura, string etiquetaTemperatura)
        {
            var series = CreateChartSeries(etiquetaCarga, etiquetaTemperatura);
            var xAxes = CreateXAxes();
            var yAxes = CreateYAxes();

            graf.Series = series;
            graf.XAxes = xAxes;
            graf.YAxes = yAxes;
        }

        private ISeries[] CreateChartSeries(string etiquetaCarga, string etiquetaTemperatura)
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = etiquetaCarga,
                    Values = _cargaLista.ToArray(),
                    Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue:F1}%"
                },
                new LineSeries<double>
                {
                    Name = etiquetaTemperatura,
                    Values = _temperaturaLista.ToArray(),
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue:F1}º"
                }
            };
        }

        private Axis[] CreateXAxes()
        {
            return new Axis[]
            {
                new Axis
                {
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    Labels = _timeList.ToArray(),
                    TextSize = 10
                }
            };
        }

        private Axis[] CreateYAxes()
        {
            return new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Green),
                    TextSize = 10,
                    MinLimit = 0,
                    MaxLimit = 100
                }
            };
        }
        #endregion

        #region Eventos de Ventana
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartMonitoring();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _monitoringTimer?.Stop();
                _monitoringTimer?.Dispose();

                // ✅ LIMPIAR LISTAS
                _cargaLista.Clear();
                _temperaturaLista.Clear();
                _timeList.Clear();
            }
        }
        #endregion
    }
}