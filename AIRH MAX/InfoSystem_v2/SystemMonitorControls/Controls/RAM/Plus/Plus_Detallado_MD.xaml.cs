using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System.Windows;
using InfoSystem_v2.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using LiveChartsCore.Defaults;

namespace SystemMonitorControls.RAM.Plus
{
    public partial class Plus_Detallado_MD : Window, IDisposable
    {
        #region Campos y Configuración
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly PeriodicTimer _updateTimer;
        private readonly ConcurrentQueue<ObservableValue> _ramDataQueue = new();
        private readonly ConcurrentQueue<string> _timeLabelsQueue = new();
        private readonly SemaphoreSlim _chartUpdateSemaphore = new(1, 1);
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly int _maxDataPoints = 30;

        private int _timeCounter = 2;
        private bool _isMonitoring = false;
        private bool _disposed = false;
        #endregion

        public Plus_Detallado_MD()
        {
            InitializeComponent();
            _updateTimer = new PeriodicTimer(_updateInterval);
            InitializeChart();
        }

        #region Inicialización del Gráfico
        private void InitializeChart()
        {
            // ✅ CONFIGURACIÓN INICIAL OPTIMIZADA
            graf.Series = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Name = "Carga RAM (%)",
                    Values = new ObservableValue[] { new ObservableValue(0) },
                    Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)),
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.Blue),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue),
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue}%",
                    LineSmoothness = 0
                }
            };

            graf.XAxes = new Axis[]
            {
                new Axis
                {
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    Labels = Array.Empty<string>(),
                    TextSize = 10,
                }
            };

            graf.YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    TextSize = 10,
                }
            };
        }
        #endregion

        #region Monitorización Asíncrona Eficiente
        private async void StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            try
            {
                // ✅ BUCLE DE ACTUALIZACIÓN MODERNO
                while (await _updateTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    await UpdateChartDataAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // ✅ CANCELACIÓN LIMPIA
                Debug.WriteLine("Monitorización de RAM cancelada limpiamente");
            }
            catch (Exception ex)
            {
                await HandleMonitoringErrorAsync(ex);
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task UpdateChartDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // ✅ OBTENER DATOS DE RAM
                var ramUsage = await GetRAMUsageAsync(cancellationToken);

                if (ramUsage.HasValue)
                {
                    // ✅ AGREGAR A COLAS CONCURRENTES
                    _ramDataQueue.Enqueue(new ObservableValue(ramUsage.Value));
                    _timeLabelsQueue.Enqueue($"{_timeCounter} Seg");

                    _timeCounter += 2;

                    // ✅ MANTENER LÍMITE DE DATOS
                    MaintainQueueSize();

                    // ✅ ACTUALIZAR GRÁFICO
                    await UpdateChartAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando datos: {ex.Message}");
            }
        }

        private async Task<double?> GetRAMUsageAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var ram = MonitorService.RAM();
                    return Math.Round(ram.Porcentaje, 0);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo uso de RAM: {ex.Message}");
                return null;
            }
        }

        private void MaintainQueueSize()
        {
            // ✅ MANTENER TAMAÑO ÓPTIMO DE COLAS
            while (_ramDataQueue.Count > _maxDataPoints)
            {
                _ramDataQueue.TryDequeue(out _);
            }
            while (_timeLabelsQueue.Count > _maxDataPoints)
            {
                _timeLabelsQueue.TryDequeue(out _);
            }
        }

        private async Task UpdateChartAsync(CancellationToken cancellationToken = default)
        {
            // ✅ EVITAR ACTUALIZACIONES CONCURRENTES
            if (!await _chartUpdateSemaphore.WaitAsync(0, cancellationToken))
                return;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var ramData = _ramDataQueue.ToArray();
                var timeLabels = _timeLabelsQueue.ToArray();

                if (ramData.Length == 0) return;

                // ✅ UNA SOLA ACTUALIZACIÓN DE UI
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateChartVisuals(ramData, timeLabels);
                }, System.Windows.Threading.DispatcherPriority.Background, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando gráfico: {ex.Message}");
            }
            finally
            {
                _chartUpdateSemaphore.Release();
            }
        }

        private void UpdateChartVisuals(ObservableValue[] ramData, string[] timeLabels)
        {
            try
            {
                // ✅ ACTUALIZAR SERIES
                if (graf.Series is ISeries[] seriesArray && seriesArray.Length > 0)
                {
                    if (seriesArray[0] is LineSeries<ObservableValue> lineSeries)
                    {
                        lineSeries.Values = ramData;
                    }
                }

                // ✅ ACTUALIZAR EJES
                if (graf.XAxes is Axis[] xAxesArray && xAxesArray.Length > 0)
                {
                    xAxesArray[0].Labels = timeLabels;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en actualización visual: {ex.Message}");
            }
        }
        #endregion

        #region Manejo de Errores
        private async Task HandleMonitoringErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error en monitorización: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error en la monitorización de RAM.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }
        #endregion

        #region Event Handlers Optimizados
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ INICIAR MONITORIZACIÓN ASÍNCRONA
            StartMonitoringAsync();
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
                Debug.WriteLine($"Error en DragMove: {ex.Message}");
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Métodos de Utilidad
        public void ClearChartData()
        {
            // ✅ LIMPIAR DATOS DEL GRÁFICO
            while (_ramDataQueue.TryDequeue(out _)) { }
            while (_timeLabelsQueue.TryDequeue(out _)) { }

            _timeCounter = 2;

            Dispatcher.Invoke(() =>
            {
                if (graf.Series is ISeries[] seriesArray && seriesArray.Length > 0)
                {
                    if (seriesArray[0] is LineSeries<ObservableValue> lineSeries)
                    {
                        lineSeries.Values = new ObservableValue[] { new ObservableValue(0) };
                    }
                }

                if (graf.XAxes is Axis[] xAxesArray && xAxesArray.Length > 0)
                {
                    xAxesArray[0].Labels = Array.Empty<string>();
                }
            });
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                // ✅ LIMPIEZA ADECUADA DE RECURSOS
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                _updateTimer?.Dispose();
                _chartUpdateSemaphore?.Dispose();

                // ✅ LIMPIAR COLAS
                while (_ramDataQueue.TryDequeue(out _)) { }
                while (_timeLabelsQueue.TryDequeue(out _)) { }

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~Plus_Detallado_MD()
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
                    _chartUpdateSemaphore?.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}