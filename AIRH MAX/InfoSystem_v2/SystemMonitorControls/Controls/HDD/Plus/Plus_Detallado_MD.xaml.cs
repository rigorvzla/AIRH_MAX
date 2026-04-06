using InfoSystem_v2.Services;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorControls.Controls.HDD;

namespace SystemMonitorControls.HDD.Plus
{
    public partial class Plus_Detallado_MD : Window, IDisposable
    {
        #region Modelos
        private class StorageViewModel
        {
            public string Unidad { get; set; }
            public string Etiqueta { get; set; }
            public string Usado { get; set; }
            public string Libre { get; set; }
            public string Total { get; set; }
        }
        #endregion

        #region Campos y Propiedades
        private System.Timers.Timer _monitoringTimer;
        private bool _disposed = false;
        private const int UPDATE_INTERVAL = 5000; // 5 segundos
        #endregion

        public Plus_Detallado_MD()
        {
            InitializeComponent();
        }

        #region Monitorización y Actualización
        private void StartMonitoring()
        {
            _monitoringTimer = new System.Timers.Timer(UPDATE_INTERVAL);
            _monitoringTimer.Elapsed += async (sender, e) => await LoadStorageDataAsync();
            _monitoringTimer.Start();

            // ✅ Carga inicial inmediata
            _ = LoadStorageDataAsync();
        }

        private async Task LoadStorageDataAsync()
        {
            try
            {
                var storageDevices = await Task.Run(() => DeviceInfoService.Storage());
                await UpdateStorageListAsync(storageDevices);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos de almacenamiento: {ex.Message}");
            }
        }

        private async Task UpdateStorageListAsync(List<InfoSystem_v2.Models.Storage> storageDevices)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    UpdateStorageListView(storageDevices);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error actualizando lista de almacenamiento: {ex.Message}");
                }
            });
        }

        private void UpdateStorageListView(List<InfoSystem_v2.Models.Storage> storageDevices)
        {
            ListaProcesos.Items.Clear();

            foreach (var storage in storageDevices)
            {
                var viewModel = new StorageViewModel
                {
                    Unidad = storage.Unidad,
                    Etiqueta = !string.IsNullOrEmpty(storage.Etiqueta) ? storage.Etiqueta : "Sin etiqueta",
                    Usado = storage.EspacioUsado,
                    Libre = storage.EspacioLibre,
                    Total = storage.TamañoTotal
                };

                ListaProcesos.Items.Add(viewModel);
            }

            // ✅ ACTUALIZAR CONTADOR DE ELEMENTOS (OPCIONAL)
            Debug.WriteLine($"Unidades de almacenamiento listadas: {storageDevices.Count}");
        }
        #endregion

        #region Eventos de Selección
        private void ListaProcesos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is StorageViewModel selectedStorage)
                {
                    SelectStorageUnit(selectedStorage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en selección de unidad: {ex.Message}");
                MessageBox.Show("Error al seleccionar la unidad", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectStorageUnit(StorageViewModel selectedStorage)
        {
            // ✅ CONFIGURAR UNIDAD SELECCIONADA
            HDD_Engine.HDD = selectedStorage.Unidad;
            HDD_Engine.Seguro = true;

            Debug.WriteLine($"Unidad seleccionada: {selectedStorage.Unidad}");

            // ✅ CERRAR VENTANA DESPUÉS DE LA SELECCIÓN
            Close();
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
            }
        }
        #endregion
    }
}