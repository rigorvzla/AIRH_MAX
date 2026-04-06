using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SystemMonitorControls.Properties;

namespace SystemMonitorControls.RED.Alarma
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
                    var redConfig = await Task.Run(() => Configuracion.Propiedades.CargarPropiedad.RED());
                    var isEnabled = await Task.Run(() => Configuracion.Propiedades.Estado.RED);
                    var downloadSpeed = await Task.Run(() => Configuracion.Propiedades.CargarPropiedad.Vel_DescargaRED());
                    var uploadSpeed = await Task.Run(() => Configuracion.Propiedades.CargarPropiedad.Vel_SubidaRED());

                    // ✅ ASIGNAR VALORES CON VALIDACIÓN
                    txt_CargaHDD.Text = redConfig?.Count > 0 ? redConfig[0] : "0";
                    txt_Subida.Text = redConfig?.Count > 1 ? redConfig[1] : "0";

                    txt_valorbajada.Text = "Su velocidad de descarga es: " + NormalizeFileSizeSafe(Convert.ToDouble(downloadSpeed));
                    txt_valorsubida.Text = "Su velocidad de subida es: " + NormalizeFileSizeSafe(Convert.ToDouble(uploadSpeed));

                    cbActivar.IsChecked = isEnabled;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cargando configuración: {ex.Message}");
                    // ✅ FALLBACK ELEGANTE
                    SetDefaultValues();
                }
            });
        }

        private void SetDefaultValues()
        {
            txt_CargaHDD.Text = "0";
            txt_Subida.Text = "0";
            txt_valorbajada.Text = "Su velocidad de descarga es: 0 B";
            txt_valorsubida.Text = "Su velocidad de subida es: 0 B";
            cbActivar.IsChecked = false;
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
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var currentText = textBox.Text;
                    var newText = GetPotentialNewText(currentText, e);

                    if (!IsValidThreshold(newText))
                    {
                        e.Handled = true;
                        ShowInputWarning("El valor debe estar entre 0 y 100");
                    }
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
                return true; // Permitir vacío temporalmente

            // ✅ VALIDAR RANGO 0-100
            if (int.TryParse(threshold, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value >= 0 && value <= 100;
            }

            return false;
        }
        #endregion

        #region Manejo de Eventos Optimizados
        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            await SaveConfigurationAsync(true);
        }

        private async void cbActivar_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            await SaveConfigurationAsync(false);
        }

        private async Task SaveConfigurationAsync(bool isEnabled)
        {
            // ✅ SEMAPHORE PARA EVITAR GUARDADOS CONCURRENTES
            if (!await _saveSemaphore.WaitAsync(0, _cancellationTokenSource.Token))
                return;

            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                // ✅ VALIDAR THRESHOLDS ANTES DE GUARDAR
                if (isEnabled && !AreValidThresholds())
                {
                    await ShowValidationErrorAsync("Los valores de umbral deben ser números entre 0 y 100");
                    Dispatcher.Invoke(() => cbActivar.IsChecked = false);
                    return;
                }

                var downloadThreshold = isEnabled ? txt_CargaHDD.Text : "0";
                var uploadThreshold = isEnabled ? txt_Subida.Text : "0";
                var configValue = $"{downloadThreshold} {uploadThreshold}";

                await Task.Run(() =>
                {
                    Configuracion.Propiedades.GuardarPropiedad.RED(isEnabled, configValue);
                }, _cancellationTokenSource.Token);

                Debug.WriteLine($"Configuración RED guardada: Habilitado={isEnabled}, Bajada={downloadThreshold}, Subida={uploadThreshold}");
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

        private bool AreValidThresholds()
        {
            return IsValidThreshold(txt_CargaHDD.Text) &&
                   IsValidThreshold(txt_Subida.Text) &&
                   !string.IsNullOrWhiteSpace(txt_CargaHDD.Text) &&
                   !string.IsNullOrWhiteSpace(txt_Subida.Text);
        }
        #endregion

        #region Event Handlers Originales Optimizados
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ResetConfigurationAsync();
        }

        private async Task ResetConfigurationAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    Settings.Default.BajadaRED = string.Empty;
                    Settings.Default.SubidaRED = string.Empty;
                    Settings.Default.Vel_SubidaRED = string.Empty;
                    Settings.Default.Vel_DescargaRED = string.Empty;
                    Settings.Default.Save();
                });

                // ✅ ACTUALIZAR UI DESPUÉS DEL RESET
                await Dispatcher.InvokeAsync(() =>
                {
                    SetDefaultValues();
                    MessageBox.Show("Configuración reiniciada correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });

                Debug.WriteLine("Configuración de RED reiniciada");
            }
            catch (Exception ex)
            {
                await HandleResetErrorAsync(ex);
            }
        }
        #endregion

        #region Manejo de Errores y UI
        private async Task HandleInitializationErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error inicializando ventana de alarma RED: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error cargando la configuración de alarma de RED. Se usarán valores por defecto.",
                    "Error de Inicialización",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }

        private async Task HandleSaveErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error guardando configuración RED: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error guardando la configuración de alarma de RED.",
                    "Error de Guardado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }

        private async Task HandleResetErrorAsync(Exception ex)
        {
            Debug.WriteLine($"Error reiniciando configuración RED: {ex.Message}");

            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    "Error reiniciando la configuración de RED.",
                    "Error de Reinicio",
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
                txt_Subida.BorderBrush = System.Windows.Media.Brushes.Red;

                // ✅ RESTAURAR DESPUÉS DE 2 SEGUNDOS
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        txt_CargaHDD.BorderBrush = originalBorder;
                        txt_Subida.BorderBrush = originalBorder;
                    });
                });
            });
        }
        #endregion

        #region Métodos de Utilidad Optimizados
        private static string NormalizeFileSizeSafe(double fileSize)
        {
            if (fileSize <= 0) return "0 B";

            try
            {
                string[] units = { "B", "KB", "MB", "GB", "TB" };
                int unitIndex = 0;
                double size = fileSize;

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
                Debug.WriteLine($"Error normalizando tamaño de archivo: {ex.Message}");
                return "N/A";
            }
        }

        public string GetCurrentDownloadThreshold()
        {
            return Dispatcher.Invoke(() => txt_CargaHDD.Text);
        }

        public string GetCurrentUploadThreshold()
        {
            return Dispatcher.Invoke(() => txt_Subida.Text);
        }

        public bool IsAlarmEnabled()
        {
            return Dispatcher.Invoke(() => cbActivar.IsChecked == true);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}