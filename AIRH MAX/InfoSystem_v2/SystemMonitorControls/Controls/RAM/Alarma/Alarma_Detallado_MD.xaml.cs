using System.Windows;
using System.Windows.Input;
using System.Globalization;
using System.Diagnostics;

namespace SystemMonitorControls.RAM.Alarma
{
    public partial class Alarma_Detallado_MD : Window, IDisposable
    {
        #region Campos y Configuración
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly SemaphoreSlim _saveSemaphore = new(1, 1);
        private bool _disposed = false;
        private bool _isInitialized = false;
        #endregion

        public Alarma_Detallado_MD()
        {
            InitializeComponent();
            InitializeAsync();
        }

        #region Inicialización Asíncrona
        private async void InitializeAsync()
        {
            try
            {
                await LoadConfigurationAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                await HandleInitializationErrorAsync(ex);
            }
        }

        private async Task LoadConfigurationAsync()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // ✅ CARGAR CONFIGURACIÓN DE FORMA ASÍNCRONA
                    var ramThreshold = await Task.Run(() => Configuracion.Propiedades.CargarPropiedad.RAM());
                    var isEnabled = await Task.Run(() => Configuracion.Propiedades.Estado.RAM);

                    txt_CargaHDD.Text = ramThreshold ?? "80";
                    cbActivar.IsChecked = isEnabled;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cargando configuración: {ex.Message}");
                    // ✅ FALLBACK ELEGANTE
                    txt_CargaHDD.Text = "80";
                    cbActivar.IsChecked = false;
                }
            });
        }
        #endregion

        #region Manejo de Eventos Optimizados
        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            await SaveConfigurationAsync(true, txt_CargaHDD.Text);
        }

        private async void cbActivar_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            await SaveConfigurationAsync(false, "0");
        }

        private async Task SaveConfigurationAsync(bool isEnabled, string threshold)
        {
            // ✅ SEMAPHORE PARA EVITAR GUARDADOS CONCURRENTES
            if (!await _saveSemaphore.WaitAsync(0, _cancellationTokenSource.Token))
                return;

            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                // ✅ VALIDAR THRESHOLD ANTES DE GUARDAR
                if (isEnabled && !IsValidThreshold(threshold))
                {
                    await ShowValidationErrorAsync("El valor de umbral debe ser un número entre 1 y 100");
                    Dispatcher.Invoke(() => cbActivar.IsChecked = false);
                    return;
                }

                await Task.Run(() =>
                {
                    Configuracion.Propiedades.GuardarPropiedad.RAM(isEnabled, threshold);
                }, _cancellationTokenSource.Token);

                Debug.WriteLine($"Configuración RAM guardada: Habilitado={isEnabled}, Umbral={threshold}");
            }
            catch (OperationCanceledException)
            {
                // ✅ CANCELACIÓN LIMPIA
                Debug.WriteLine("Guardado de configuración cancelado");
            }
            catch (Exception ex)
            {
                await HandleSaveErrorAsync(ex);
            }
            finally
            {
                _saveSemaphore.Release();
            }
        }
        #endregion

        #region Validación de Entrada Robusta
        private void TxtHDD_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // ✅ VALIDACIÓN MEJORADA DE TECLAS
                if (IsNavigationKey(e.Key) || IsFunctionKey(e.Key))
                {
                    return; // Permitir teclas de navegación
                }

                if (!IsNumericKey(e.Key))
                {
                    e.Handled = true;
                    return;
                }

                // ✅ VALIDACIÓN EN TIEMPO REAL DEL VALOR COMPLETO
                var currentText = txt_CargaHDD.Text;
                var newText = GetPotentialNewText(currentText, e);

                if (!IsValidThreshold(newText))
                {
                    e.Handled = true;
                    ShowInputWarning("El valor debe estar entre 1 y 100");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en validación de tecla: {ex.Message}");
                e.Handled = true; // ✅ FALLBACK SEGURO
            }
        }

        private bool IsNavigationKey(Key key)
        {
            return key == Key.Tab || key == Key.Enter || key == Key.Escape ||
                   key == Key.Back || key == Key.Delete || key == Key.Left ||
                   key == Key.Right || key == Key.Home || key == Key.End;
        }

        private bool IsFunctionKey(Key key)
        {
            return key >= Key.F1 && key <= Key.F24;
        }

        private bool IsNumericKey(Key key)
        {
            // ✅ PERMITIR NÚMEROS DEL TECLADO PRINCIPAL Y NUMPAD
            return (key >= Key.D0 && key <= Key.D9) ||
                   (key >= Key.NumPad0 && key <= Key.NumPad9);
        }

        private string GetPotentialNewText(string currentText, KeyEventArgs e)
        {
            if (e.Key == Key.Back && currentText.Length > 0)
            {
                return currentText.Substring(0, currentText.Length - 1);
            }
            else if (e.Key == Key.Delete)
            {
                return currentText;
            }
            else
            {
                var newChar = KeyToChar(e.Key);
                return currentText + newChar;
            }
        }

        private char KeyToChar(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9)
                return (char)('0' + (key - Key.D0));
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return (char)('0' + (key - Key.NumPad0));
            return '\0';
        }

        private bool IsValidThreshold(string threshold)
        {
            if (string.IsNullOrWhiteSpace(threshold))
                return false;

            // ✅ VALIDAR RANGO 1-100
            if (int.TryParse(threshold, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value >= 1 && value <= 100;
            }

            return false;
        }
        #endregion

        #region Manejo de Errores y UI
        private async Task HandleInitializationErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error inicializando ventana de alarma RAM: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error cargando la configuración de alarma de RAM. Se usarán valores por defecto.",
                    "Error de Inicialización",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }

        private async Task HandleSaveErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error guardando configuración RAM: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error guardando la configuración de alarma de RAM.",
                    "Error de Guardado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }

        private async Task ShowValidationErrorAsync(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private void ShowInputWarning(string message)
        {
            // ✅ FEEDBACK VISUAL SIN BLOQUEAR
            Dispatcher.InvokeAsync(() =>
            {
                var originalBorder = txt_CargaHDD.BorderBrush;
                txt_CargaHDD.BorderBrush = System.Windows.Media.Brushes.Red;

                // ✅ RESTAURAR DESPUÉS DE 2 SEGUNDOS
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        txt_CargaHDD.BorderBrush = originalBorder;
                    });
                });
            });
        }
        #endregion

        #region Event Handlers Optimizados
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ LA CARGA SE HACE EN InitializeAsync PARA EVITAR BLOQUEOS
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        public string GetCurrentThreshold()
        {
            return Dispatcher.Invoke(() => txt_CargaHDD.Text);
        }

        public bool IsAlarmEnabled()
        {
            return Dispatcher.Invoke(() => cbActivar.IsChecked == true);
        }

        public async Task UpdateThresholdAsync(string newThreshold)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (IsValidThreshold(newThreshold))
                {
                    txt_CargaHDD.Text = newThreshold;
                }
            });
        }

        public async Task EnableAlarmAsync(bool enable)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                cbActivar.IsChecked = enable;
            });
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _saveSemaphore?.Dispose();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~Alarma_Detallado_MD()
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
                    _saveSemaphore?.Dispose();
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