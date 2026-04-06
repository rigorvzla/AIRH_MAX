using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.CPU.TOP
{
    public partial class Top_Detallado_MD : Window, IDisposable
    {
        #region Modelos
        public class ModelView
        {
            public string Nombre { get; set; }
            public int ID { get; set; }
            public string CPU { get; set; }
        }

        private class ProcessModel
        {
            public string Nombre { get; set; }
            public int ID { get; set; }
            public long CPU { get; set; }
        }
        #endregion

        #region Campos y Propiedades
        private System.Timers.Timer _monitoringTimer;
        private bool _disposed = false;
        private readonly object _lockObject = new object();
        private const int UPDATE_INTERVAL = 5000; // 5 segundos (más eficiente)
        private const int TOP_PROCESS_COUNT = 5;
        #endregion

        public Top_Detallado_MD()
        {
            InitializeComponent();
        }

        #region Monitorización de Procesos
        private void StartMonitoring()
        {
            _monitoringTimer = new System.Timers.Timer(UPDATE_INTERVAL);
            _monitoringTimer.Elapsed += async (sender, e) => await LoadProcessDataAsync();
            _monitoringTimer.Start();

            // ✅ Carga inicial inmediata
            _ = LoadProcessDataAsync();
        }

        private async Task LoadProcessDataAsync()
        {
            try
            {
                var topProcesses = await Task.Run(() => GetTopProcessesByCPU());
                await UpdateUIAsync(topProcesses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando procesos: {ex.Message}");
            }
        }

        private async Task UpdateUIAsync(List<ProcessModel> processes)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    ListaProcesos.Items.Clear();

                    foreach (var process in processes)
                    {
                        var viewModel = new ModelView
                        {
                            Nombre = process.Nombre,
                            ID = process.ID,
                            CPU = $"{process.CPU}%"
                        };
                        ListaProcesos.Items.Add(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando UI: {ex.Message}");
                }
            });
        }
        #endregion

        #region Obtención de Procesos Optimizada
        private List<ProcessModel> GetTopProcessesByCPU()
        {
            var processes = new List<ProcessModel>();

            try
            {
                // ✅ OBTENER PROCESOS UNA SOLA VEZ
                var processList = Process.GetProcesses();

                // ✅ USAR PARALELISMO PARA MEJOR RENDIMIENTO
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount / 2
                };

                var processData = new List<ProcessModel>(processList.Length);

                Parallel.ForEach(processList, parallelOptions, process =>
                {
                    try
                    {
                        if (process.ProcessName == "Idle") return;

                        var cpuUsage = GetProcessCPUUsage(process.Id);
                        if (cpuUsage > 0) // ✅ FILTRAR PROCESOS CON USO REAL
                        {
                            lock (_lockObject)
                            {
                                processData.Add(new ProcessModel
                                {
                                    Nombre = process.ProcessName,
                                    ID = process.Id,
                                    CPU = cpuUsage
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error procesando {process.ProcessName}: {ex.Message}");
                    }
                });

                // ✅ ORDENAR Y TOMAR TOP 5
                processes = processData
                    .OrderByDescending(p => p.CPU)
                    .Take(TOP_PROCESS_COUNT)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetTopProcessesByCPU: {ex.Message}");
            }

            return processes;
        }

        private long GetProcessCPUUsage(int processId)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ProcessID = {processId}");

                using var results = searcher.Get();
                var managementObject = results.Cast<ManagementObject>().FirstOrDefault();

                if (managementObject != null)
                {
                    // ✅ CÁLCULO SIMPLIFICADO DE USO DE CPU
                    var userModeTime = (ulong)managementObject["UserModeTime"];
                    var kernelModeTime = (ulong)managementObject["KernelModeTime"];
                    var totalTime = userModeTime + kernelModeTime;

                    // ✅ CONVERTIR A PORCENTAJE APROXIMADO
                    return (long)(totalTime / 10000.0); // Simplificación para ranking
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo uso de CPU para proceso {processId}: {ex.Message}");
            }

            return 0;
        }
        #endregion

        #region Gestión de Procesos
        private void TerminateProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // ✅ ESPERAR HASTA 5 SEGUNDOS
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error terminando proceso {processName}: {ex.Message}");
                            throw; // ✅ RE-LANZAR PARA MANEJO SUPERIOR
                        }
                    }

                    // ✅ ACTUALIZAR LISTA DESPUÉS DE TERMINAR PROCESO
                    _ = LoadProcessDataAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en TerminateProcess: {ex.Message}");
                throw;
            }
        }

        private async void ListaProcesos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaProcesos.SelectedIndex < 0 || e.AddedItems.Count == 0)
                return;

            try
            {
                var selectedProcess = e.AddedItems[0] as ModelView;
                if (selectedProcess == null) return;

                // ✅ CONFIRMACIÓN CON USUARIO
                var result = MessageBox.Show(
                    $"¿Está seguro que desea terminar el proceso '{selectedProcess.Nombre}'?",
                    "Confirmar terminación de proceso",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await Task.Run(() => TerminateProcess(selectedProcess.Nombre));

                    MessageBox.Show(
                        $"Proceso '{selectedProcess.Nombre}' terminado exitosamente",
                        "Proceso terminado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en selección de proceso: {ex.Message}");
                MessageBox.Show(
                    $"Error al terminar el proceso: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // ✅ DESELECCIONAR ITEM
                ListaProcesos.SelectedIndex = -1;
            }
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
            }
        }
        #endregion
    }
}