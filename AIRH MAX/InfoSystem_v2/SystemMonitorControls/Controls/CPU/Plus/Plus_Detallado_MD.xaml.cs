using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Diagnostics;
using System.Windows;

namespace SystemMonitorControls.CPU.Plus
{
    public partial class Plus_Detallado_MD : Window, IDisposable
    {
        #region Modelos
        public class ModelView
        {
            public string Nucleo { get; set; }
            public string Hilo { get; set; }
            public string Temperatura { get; set; }
            public string Carga { get; set; }
        }
        #endregion

        #region Campos y Propiedades
        private System.Timers.Timer _monitoringTimer;
        private readonly List<double> _temperaturasLista = new();
        private readonly List<double> _cargaLista = new();
        private readonly List<string> _timeList = new();
        private int _tiempoSegundos = 2;
        private bool _disposed = false;
        private const int UPDATE_INTERVAL = 2000; // 2 segundos (más datos)
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
            _monitoringTimer.Elapsed += async (sender, e) => await LoadCpuDataAsync();
            _monitoringTimer.Start();

            // ✅ Carga inicial inmediata
            _ = LoadCpuDataAsync();
        }

        private async Task LoadCpuDataAsync()
        {
            try
            {
                var cpuData = await Task.Run(() => MonitorService.CPU());
                await UpdateAllUIAsync(cpuData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos CPU: {ex.Message}");
            }
        }

        private async Task UpdateAllUIAsync(DeviceMonitor.CPU cpuData)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateTemperatureAndLoadLists(cpuData);
                    UpdateCoreTemperaturesList(cpuData);
                    UpdateThreadLoadList(cpuData);
                    UpdateChart();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando UI: {ex.Message}");
                }
            });
        }
        #endregion

        #region Actualización de Listas
        private void UpdateTemperatureAndLoadLists(DeviceMonitor.CPU cpuData)
        {
            // ✅ AGREGAR NUEVOS DATOS
            _temperaturasLista.Add(cpuData.TemperaturaGeneral);
            _cargaLista.Add(cpuData.CargaGeneral);
            _timeList.Add($"{_tiempoSegundos} Seg");
            _tiempoSegundos += 2;

            // ✅ MANTENER LÍMITE DE DATOS PARA EVITAR CRECIMIENTO INFINITO
            if (_temperaturasLista.Count > MAX_DATA_POINTS)
            {
                _temperaturasLista.RemoveAt(0);
                _cargaLista.RemoveAt(0);
                _timeList.RemoveAt(0);
            }
        }

        private void UpdateCoreTemperaturesList(DeviceMonitor.CPU cpuData)
        {
            ListaProcesos.Items.Clear();

            foreach (var item in cpuData.Temperaturas_Nucleos)
            {
                var model = new ModelView
                {
                    Temperatura = $"{item.Value:F1}°C",
                    Nucleo = item.Key
                };
                ListaProcesos.Items.Add(model);
            }
        }

        private void UpdateThreadLoadList(DeviceMonitor.CPU cpuData)
        {
            ListaProcesos2.Items.Clear();

            foreach (var item in cpuData.Hilos_Carga)
            {
                var model = new ModelView
                {
                    Carga = $"{item.Value:F0}%",
                    Hilo = item.Key
                };
                ListaProcesos2.Items.Add(model);
            }
        }
        #endregion

        #region Actualización de Gráfica
        private void UpdateChart()
        {
            try
            {
                var series = CreateChartSeries();
                var axes = CreateChartAxes();

                graf.Series = series;
                graf.XAxes = axes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando gráfica: {ex.Message}");
            }
        }

        private ISeries[] CreateChartSeries()
        {
            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Temperatura (ºC)",
                    Values = _temperaturasLista.ToArray(),
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue:F1}º"
                },
                new LineSeries<double>
                {
                    Name = "Carga CPU (%)",
                    Values = _cargaLista.ToArray(),
                    Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue:F1}%"
                }
            };
        }

        private Axis[] CreateChartAxes()
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
            DragMove();
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
                _temperaturasLista.Clear();
                _cargaLista.Clear();
                _timeList.Clear();
            }
        }
        #endregion
    }
}