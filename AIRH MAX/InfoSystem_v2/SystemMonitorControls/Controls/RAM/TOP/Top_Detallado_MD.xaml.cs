using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;

namespace SystemMonitorControls.RAM.TOP
{
    public partial class Top_Detallado_MD : Window, IDisposable
    {
        #region Campos y Configuración
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly PeriodicTimer _updateTimer;
        private readonly ConcurrentDictionary<string, ProcessData> _processCache = new();
        private readonly SemaphoreSlim _updateSemaphore = new(1, 1);
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(5);

        private bool _isMonitoring = false;
        private bool _disposed = false;
        #endregion

        public Top_Detallado_MD()
        {
            InitializeComponent();
            _updateTimer = new PeriodicTimer(_updateInterval);
        }

        #region Modelos de Datos Optimizados
        public sealed class ProcessViewModel
        {
            public required string Nombre { get; init; }
            public required string RAM { get; init; }
        }

        private sealed class ProcessData
        {
            public required string Name { get; init; }
            public long WorkingSet { get; set; }
            public required string FormattedRAM { get; set; }
            public DateTime LastUpdated { get; set; }
            public bool IsValid => !string.IsNullOrEmpty(Name) && WorkingSet >= 0;
        }
        #endregion

        #region Monitorización Asíncrona Eficiente
        private async void StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;

            try
            {
                // ✅ CARGA INICIAL INMEDIATA
                await LoadProcessDataAsync(_cancellationTokenSource.Token);

                // ✅ BUCLE DE ACTUALIZACIÓN CON CANCELLATION TOKEN
                while (await _updateTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    await LoadProcessDataAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // ✅ CANCELACIÓN LIMPIA - NO ES UN ERROR
                Debug.WriteLine("Monitorización cancelada limpiamente");
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

        private async Task LoadProcessDataAsync(CancellationToken cancellationToken = default)
        {
            // ✅ SEMAPHORE PARA EVITAR ACTUALIZACIONES CONCURRENTES
            if (!await _updateSemaphore.WaitAsync(0, cancellationToken))
                return;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var topProcesses = await GetTopProcessesByRAMAsync(5, cancellationToken);

                // ✅ UNA SOLA INVOCACIÓN DISPATCHER POR ACTUALIZACIÓN
                await Dispatcher.InvokeAsync(() =>
                {
                    // ✅ ACTUALIZACIÓN ATÓMICA DE LA UI
                    ListaProcesos.Items.Clear();

                    foreach (var process in topProcesses.Where(p => p.IsValid))
                    {
                        ListaProcesos.Items.Add(new ProcessViewModel
                        {
                            Nombre = process.Name,
                            RAM = process.FormattedRAM
                        });
                    }
                }, System.Windows.Threading.DispatcherPriority.Background, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; // ✅ RE-LANZAR PARA MANEJO SUPERIOR
            }
            catch (Exception ex)
            {
                await HandleDataLoadErrorAsync(ex);
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async Task<List<ProcessData>> GetTopProcessesByRAMAsync(int count, CancellationToken cancellationToken)
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await Task.Run(() => GetTopProcessesByRAMInternal(count), cancellationToken);
                }
                catch (Exception ex) when (retryCount < maxRetries - 1)
                {
                    // ✅ REINTENTOS AUTOMÁTICOS CON BACKOFF
                    retryCount++;
                    var delay = TimeSpan.FromMilliseconds(500 * retryCount);
                    await Task.Delay(delay, cancellationToken);
                    Debug.WriteLine($"Reintento {retryCount} después de error: {ex.Message}");
                }
            }

            return new List<ProcessData>(); // ✅ FALLBACK ELEGANTE
        }

        private List<ProcessData> GetTopProcessesByRAMInternal(int count)
        {
            var processes = new List<ProcessData>();
            var currentTime = DateTime.Now;

            try
            {
                // ✅ ACTUALIZAR CACHE DE PROCESOS
                UpdateProcessCache();

                // ✅ OBTENER TOP PROCESOS VÁLIDOS DEL CACHE
                processes = _processCache.Values
                    .Where(p => p.IsValid && (currentTime - p.LastUpdated) < _cacheExpiration)
                    .OrderByDescending(p => p.WorkingSet)
                    .Take(count)
                    .ToList();

                // ✅ FALLBACK SI NO HAY SUFICIENTES PROCESOS VÁLIDOS
                if (processes.Count < count)
                {
                    var freshProcesses = GetFreshProcessData(count);
                    processes = freshProcesses
                        .Where(p => p.IsValid)
                        .Take(count)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetTopProcessesByRAMInternal: {ex.Message}");
                processes = GetFallbackProcessData(); // ✅ FALLBACK ROBUSTO
            }

            return processes;
        }
        #endregion

        #region Gestión de Cache y Procesos
        private void UpdateProcessCache()
        {
            Process[] currentProcesses = Array.Empty<Process>();

            try
            {
                currentProcesses = Process.GetProcesses();
                var currentTime = DateTime.Now;

                // ✅ ACTUALIZACIÓN EFICIENTE DEL CACHE
                foreach (var process in currentProcesses)
                {
                    try
                    {
                        if (process.Id == 0 || string.IsNullOrEmpty(process.ProcessName))
                            continue;

                        var processName = process.ProcessName;
                        var workingSet = process.WorkingSet64;

                        // ✅ VALIDACIÓN DE RAM VÁLIDA
                        if (workingSet < 0) continue;

                        if (_processCache.TryGetValue(processName, out var cachedProcess))
                        {
                            // ✅ ACTUALIZAR PROCESO EXISTENTE
                            cachedProcess.WorkingSet = workingSet;
                            cachedProcess.FormattedRAM = NormalizeFileSizeSafe(workingSet);
                            cachedProcess.LastUpdated = currentTime;
                        }
                        else
                        {
                            // ✅ AGREGAR NUEVO PROCESO CON VALIDACIÓN
                            var formattedRAM = NormalizeFileSizeSafe(workingSet);

                            if (!string.IsNullOrEmpty(formattedRAM))
                            {
                                _processCache[processName] = new ProcessData
                                {
                                    Name = processName,
                                    WorkingSet = workingSet,
                                    FormattedRAM = formattedRAM,
                                    LastUpdated = currentTime
                                };
                            }
                        }
                    }
                    catch (InvalidOperationException) when (process.HasExited)
                    {
                        // ✅ PROCESO TERMINADO - ELIMINAR DEL CACHE
                        _processCache.TryRemove(process.ProcessName, out _);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error actualizando proceso {process.ProcessName}: {ex.Message}");
                    }
                }

                // ✅ LIMPIEZA PERIÓDICA DEL CACHE
                CleanProcessCache(currentProcesses);
            }
            finally
            {
                // ✅ LIBERACIÓN ADECUADA DE RECURSOS
                foreach (var process in currentProcesses)
                {
                    process?.Dispose();
                }
            }
        }

        private void CleanProcessCache(Process[] currentProcesses)
        {
            try
            {
                var currentProcessNames = currentProcesses
                    .Where(p => p != null && !string.IsNullOrEmpty(p.ProcessName))
                    .Select(p => p.ProcessName)
                    .ToHashSet();

                var expiredTime = DateTime.Now - _cacheExpiration;
                var keysToRemove = new List<string>();

                // ✅ IDENTIFICAR PROCESOS A ELIMINAR
                foreach (var kvp in _processCache)
                {
                    if (!currentProcessNames.Contains(kvp.Key) ||
                        kvp.Value.LastUpdated < expiredTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                // ✅ ELIMINACIÓN SEGURA
                foreach (var key in keysToRemove)
                {
                    _processCache.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CleanProcessCache: {ex.Message}");
            }
        }

        private List<ProcessData> GetFreshProcessData(int count)
        {
            var processes = new List<ProcessData>();

            try
            {
                // ✅ BÚSQUEDA EFICIENTE CON GESTIÓN DE RECURSOS
                using var processList = new ProcessCollection();
                var freshProcesses = processList.GetProcesses(count);
                processes.AddRange(freshProcesses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetFreshProcessData: {ex.Message}");
            }

            return processes;
        }

        private List<ProcessData> GetFallbackProcessData()
        {
            // ✅ FALLBACK ELEGANTE CON DATOS MÍNIMOS
            return _processCache.Values
                .Where(p => p.IsValid)
                .OrderByDescending(p => p.WorkingSet)
                .Take(3)
                .ToList();
        }
        #endregion

        #region Métodos de Utilidad Robusta
        private static string NormalizeFileSizeSafe(long fileSize)
        {
            // ✅ VALIDACIÓN COMPLETA DE ENTRADA
            if (fileSize <= 0)
                return "0 B";

            try
            {
                string[] units = { "B", "KB", "MB", "GB", "TB" };
                int unitIndex = 0;
                double size = fileSize;

                // ✅ EVITAR DIVISIÓN POR CERO Y OVERFLOW
                while (size >= 1024 && unitIndex < units.Length - 1)
                {
                    size /= 1024;
                    unitIndex++;
                }

                return unitIndex == 0 ?
                    $"{size:0} {units[unitIndex]}" :
                    $"{size:0.#} {units[unitIndex]}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en NormalizeFileSizeSafe: {ex.Message}");
                return "N/A"; // ✅ FALLBACK ELEGANTE
            }
        }

        private async Task TerminateProcessAsync(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                await ShowErrorMessageAsync("Nombre de proceso inválido");
                return;
            }

            try
            {
                var result = await Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(
                        $"¿Está seguro de que desea terminar el proceso '{processName}'?",
                        "Confirmar terminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question),
                    System.Windows.Threading.DispatcherPriority.Normal);

                if (result != MessageBoxResult.Yes) return;

                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    await ShowErrorMessageAsync("Proceso no encontrado");
                    return;
                }

                bool success = false;

                foreach (var process in processes)
                {
                    try
                    {
                        using (process)
                        {
                            process.Kill();
                            await Task.Run(() => process.WaitForExit(5000));
                            success = true;

                            // ✅ ELIMINAR DEL CACHE INMEDIATAMENTE
                            _processCache.TryRemove(processName, out _);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error terminando proceso {processName}: {ex.Message}");
                    }
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    if (success)
                    {
                        MessageBox.Show("Proceso terminado exitosamente", "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo terminar el proceso", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                // ✅ ACTUALIZACIÓN INMEDIATA DESPUÉS DE TERMINAR PROCESO
                if (success)
                    await LoadProcessDataAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Error: {ex.Message}");
            }
        }
        #endregion

        #region Manejo de Errores y UI Responsive
        private async Task HandleMonitoringErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error en monitorización: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error en la monitorización de procesos. La ventana se cerrará.",
                    "Error del Sistema",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close(); // ✅ CIERRE LIMPIO EN CASO DE ERROR GRAVE
            });
        }

        private async Task HandleDataLoadErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error cargando datos: {ex.Message}");

            // ✅ NO MOSTRAR MENSAJES MOLESTOS AL USUARIO POR ERRORES TEMPORALES
            await Task.CompletedTask;
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
        #endregion

        #region Event Handlers Optimizados
        private async void ListaProcesos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ✅ EVITAR PROCESAMIENTO SI NO HAY ELEMENTOS SELECCIONADOS
            if (e.AddedItems.Count == 0 || e.AddedItems[0] is not ProcessViewModel selectedItem)
                return;

            try
            {
                await TerminateProcessAsync(selectedItem.Nombre);
            }
            finally
            {
                // ✅ DESELECCIONAR PARA EVITAR MÚLTIPLES EVENTOS
                Dispatcher.InvokeAsync(() => ListaProcesos.SelectedItem = null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ INICIAR MONITORIZACIÓN ASÍNCRONA
            StartMonitoringAsync();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose(); // ✅ LIMPIEZA ADECUADA AL CERRAR
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove(); // ✅ MANTENER FUNCIONALIDAD DE ARRASTRE
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // ✅ CIERRE LIMPIO
        }
        #endregion

        #region Disposable Pattern Implementado
        public void Dispose()
        {
            if (!_disposed)
            {
                // ✅ CANCELACIÓN LIMPIA DE OPERACIONES
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                _updateTimer?.Dispose();
                _updateSemaphore?.Dispose();
                _processCache.Clear();

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~Top_Detallado_MD()
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

        #region Clase Auxiliar para Gestión de Procesos (CORREGIDA)
        private sealed class ProcessCollection : IDisposable
        {
            private readonly List<Process> _processes = new();

            public IEnumerable<ProcessData> GetProcesses(int count)
            {
                var processDataList = new List<ProcessData>();
                ClearProcesses();

                try
                {
                    var processes = Process.GetProcesses();

                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.Id == 0 || string.IsNullOrEmpty(process.ProcessName))
                            {
                                process.Dispose();
                                continue;
                            }

                            var workingSet = process.WorkingSet64;
                            if (workingSet < 0)
                            {
                                process.Dispose();
                                continue;
                            }

                            var formattedRAM = NormalizeFileSizeSafe(workingSet);
                            if (string.IsNullOrEmpty(formattedRAM))
                            {
                                process.Dispose();
                                continue;
                            }

                            _processes.Add(process);

                            processDataList.Add(new ProcessData
                            {
                                Name = process.ProcessName,
                                WorkingSet = workingSet,
                                FormattedRAM = formattedRAM,
                                LastUpdated = DateTime.Now
                            });

                            // ✅ LIMITAR EL NÚMERO DE PROCESOS DEVUELTOS
                            if (processDataList.Count >= count)
                                break;
                        }
                        catch
                        {
                            process?.Dispose();
                            continue;
                        }
                    }

                    return processDataList;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en ProcessCollection.GetProcesses: {ex.Message}");
                    return processDataList; // ✅ DEVOLVER LISTA PARCIAL EN LUGAR DE FALLAR
                }
            }

            private void ClearProcesses()
            {
                foreach (var process in _processes)
                {
                    process?.Dispose();
                }
                _processes.Clear();
            }

            public void Dispose()
            {
                ClearProcesses();
                GC.SuppressFinalize(this);
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