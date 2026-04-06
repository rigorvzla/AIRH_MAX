using InfoSystem_v2.Models;
using InfoSystem_v2.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using SystemMonitorControls.Configuracion;

namespace SystemMonitorControls.RED.TOP
{
    public partial class Top_DetalladoMD : Window, IDisposable
    {
        #region Campos y Configuración
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly PeriodicTimer _updateTimer;
        private readonly ConcurrentQueue<ObservableValue> _downloadDataQueue = new();
        private readonly ConcurrentQueue<ObservableValue> _uploadDataQueue = new();
        private readonly ConcurrentQueue<string> _timeLabelsQueue = new();
        private readonly SemaphoreSlim _chartUpdateSemaphore = new(1, 1);
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly int _maxDataPoints = 30;

        private int _timeCounter = 2;
        private bool _isMonitoring = false;
        private bool _disposed = false;
        #endregion

        public Top_DetalladoMD()
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
                    Name = "Subida (%)",
                    Values = new ObservableValue[] { new ObservableValue(0) },
                    Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(90)),
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.Blue),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue),
                    TooltipLabelFormatter = (chartPoint) => $"{Math.Round(chartPoint.PrimaryValue, 0)}%",
                    LineSmoothness = 0
                },
                new LineSeries<ObservableValue>
                {
                    Name = "Descarga (%)",
                    Values = new ObservableValue[] { new ObservableValue(0) },
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)),
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColors.Green),
                    GeometryStroke = new SolidColorPaint(SKColors.Green),
                    TooltipLabelFormatter = (chartPoint) => $"{Math.Round(chartPoint.PrimaryValue, 0)}%",
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await StartMonitoringAsync();
        }

        private async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            try
            {
                while (await _updateTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    await UpdateChartDataAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Monitorización de RED TOP cancelada limpiamente");
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

                var networkData = await GetNetworkDataAsync(cancellationToken);

                if (networkData != null)
                {
                    await ProcessNetworkDataAsync(networkData, cancellationToken);
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

        private async Task ProcessNetworkDataAsync(DeviceMonitor.RED networkData, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ CALCULAR PORCENTAJES DE FORMA ASÍNCRONA
                var downloadPercentage = await CalculatePercentageAsync(
                    networkData.VelocidadDescarga ?? 0,
                    true,
                    cancellationToken);

                var uploadPercentage = await CalculatePercentageAsync(
                    networkData.VelocidadSubida ?? 0,
                    false,
                    cancellationToken);

                // ✅ AGREGAR A COLAS
                _downloadDataQueue.Enqueue(new ObservableValue(downloadPercentage));
                _uploadDataQueue.Enqueue(new ObservableValue(uploadPercentage));
                _timeLabelsQueue.Enqueue($"{_timeCounter} Seg");

                _timeCounter += 2;

                // ✅ MANTENER LÍMITE DE DATOS
                MaintainQueueSizes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error procesando datos de red: {ex.Message}");
            }
        }

        private async Task<double> CalculatePercentageAsync(double currentSpeed, bool isDownload, CancellationToken cancellationToken)
        {
            try
            {
                var maxSpeed = await GetMaxSpeedAsync(isDownload, cancellationToken);

                if (maxSpeed <= 0) return 0;

                var percentage = (currentSpeed / maxSpeed) * 100;
                return Math.Round(Math.Min(percentage, 100), 0); // ✅ LIMITAR A 100%
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculando porcentaje: {ex.Message}");
                return 0;
            }
        }

        private async Task<double> GetMaxSpeedAsync(bool isDownload, CancellationToken cancellationToken)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var speed = isDownload ?
                        Propiedades.CargarPropiedad.Vel_DescargaRED() :
                        Propiedades.CargarPropiedad.Vel_SubidaRED();

                    return Convert.ToDouble(speed);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo velocidad máxima: {ex.Message}");
                return 100.0; // ✅ VALOR POR DEFECTO
            }
        }

        private void MaintainQueueSizes()
        {
            // ✅ MANTENER TAMAÑO ÓPTIMO DE COLAS
            while (_downloadDataQueue.Count > _maxDataPoints)
            {
                _downloadDataQueue.TryDequeue(out _);
            }
            while (_uploadDataQueue.Count > _maxDataPoints)
            {
                _uploadDataQueue.TryDequeue(out _);
            }
            while (_timeLabelsQueue.Count > _maxDataPoints)
            {
                _timeLabelsQueue.TryDequeue(out _);
            }
        }
        #endregion

        #region Actualización del Gráfico Optimizada
        private async Task UpdateChartAsync(CancellationToken cancellationToken = default)
        {
            if (!await _chartUpdateSemaphore.WaitAsync(0, cancellationToken))
                return;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var downloadData = _downloadDataQueue.ToArray();
                var uploadData = _uploadDataQueue.ToArray();
                var timeLabels = _timeLabelsQueue.ToArray();

                if (downloadData.Length == 0 || uploadData.Length == 0) return;

                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateChartVisuals(downloadData, uploadData, timeLabels);
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

        private void UpdateChartVisuals(ObservableValue[] downloadData, ObservableValue[] uploadData, string[] timeLabels)
        {
            try
            {
                // ✅ ACTUALIZAR SERIES
                if (graf.Series is ISeries[] seriesArray && seriesArray.Length >= 2)
                {
                    if (seriesArray[0] is LineSeries<ObservableValue> uploadSeries)
                    {
                        uploadSeries.Values = uploadData;
                    }

                    if (seriesArray[1] is LineSeries<ObservableValue> downloadSeries)
                    {
                        downloadSeries.Values = downloadData;
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
                    "Error en la monitorización de RED. El gráfico puede no estar actualizándose correctamente.",
                    "Error del Sistema",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }
        #endregion

        #region Event Handlers Optimizados
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
            while (_downloadDataQueue.TryDequeue(out _)) { }
            while (_uploadDataQueue.TryDequeue(out _)) { }
            while (_timeLabelsQueue.TryDequeue(out _)) { }

            _timeCounter = 2;

            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (graf.Series is ISeries[] seriesArray && seriesArray.Length >= 2)
                    {
                        if (seriesArray[0] is LineSeries<ObservableValue> uploadSeries)
                        {
                            uploadSeries.Values = new ObservableValue[] { new ObservableValue(0) };
                        }

                        if (seriesArray[1] is LineSeries<ObservableValue> downloadSeries)
                        {
                            downloadSeries.Values = new ObservableValue[] { new ObservableValue(0) };
                        }
                    }

                    if (graf.XAxes is Axis[] xAxesArray && xAxesArray.Length > 0)
                    {
                        xAxesArray[0].Labels = Array.Empty<string>();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error limpiando datos del gráfico: {ex.Message}");
                }
            });
        }

        public string GetMonitoringStatus()
        {
            return _isMonitoring ? "Monitorizando" : "Detenido";
        }

        public int GetDataPointCount()
        {
            return Math.Min(_downloadDataQueue.Count, _uploadDataQueue.Count);
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
                _chartUpdateSemaphore?.Dispose();

                // ✅ LIMPIAR COLAS
                while (_downloadDataQueue.TryDequeue(out _)) { }
                while (_uploadDataQueue.TryDequeue(out _)) { }
                while (_timeLabelsQueue.TryDequeue(out _)) { }

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~Top_DetalladoMD()
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