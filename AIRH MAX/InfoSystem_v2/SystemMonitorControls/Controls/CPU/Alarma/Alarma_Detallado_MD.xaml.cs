using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SystemMonitorControls.Configuracion;

namespace SystemMonitorControls.CPU.Alarma
{
    public partial class Alarma_Detallado_MD : Window
    {
        #region Campos y Constantes
        private bool _isInitialized = false;
        private const int TEMPERATURE_MAX_LENGTH = 3;
        private const int LOAD_MAX_LENGTH = 3; // Cambiado de 4 a 3 (máximo 100%)
        #endregion

        public Alarma_Detallado_MD()
        {
            InitializeComponent();
        }

        #region Inicialización y Carga de Datos
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadStoredSettings();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                HandleError("Error al cargar configuraciones", ex);
            }
        }

        private void LoadStoredSettings()
        {
            var cpuSettings = Propiedades.CargarPropiedad.CPU();

            // ✅ ASIGNACIÓN SEGURA CON VALIDACIÓN
            txb_TemperaHDD.Text = cpuSettings.Count > 1 ? cpuSettings[1] : "0";
            txt_CargaHDD.Text = cpuSettings.Count > 0 ? cpuSettings[0] : "0";
            cbActivar.IsChecked = Propiedades.Estado.CPU;
        }
        #endregion

        #region Gestión de Alertas
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                if (ValidateInputs())
                {
                    Propiedades.GuardarPropiedad.CPU(
                        true,
                        txt_CargaHDD.Text,
                        txb_TemperaHDD.Text);
                }
                else
                {
                    // ✅ DESMARCAR SI LA VALIDACIÓN FALLA
                    cbActivar.IsChecked = false;
                    ShowValidationError();
                }
            }
            catch (Exception ex)
            {
                HandleError("Error al activar alerta", ex);
                cbActivar.IsChecked = false;
            }
        }

        private void cbActivar_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                Propiedades.GuardarPropiedad.CPU(false, "0", "0");
            }
            catch (Exception ex)
            {
                HandleError("Error al desactivar alerta", ex);
            }
        }
        #endregion

        #region Validación de Entrada
        private bool ValidateInputs()
        {
            return IsValidTemperature(txb_TemperaHDD.Text) &&
                   IsValidLoad(txt_CargaHDD.Text);
        }

        private bool IsValidTemperature(string temperature)
        {
            if (string.IsNullOrWhiteSpace(temperature)) return false;

            if (int.TryParse(temperature, out int temp))
            {
                return temp >= 0 && temp <= 150; // ✅ RANGO RAZONABLE DE TEMPERATURA
            }

            return false;
        }

        private bool IsValidLoad(string load)
        {
            if (string.IsNullOrWhiteSpace(load)) return false;

            if (int.TryParse(load, out int loadValue))
            {
                return loadValue >= 0 && loadValue <= 100; // ✅ PORCENTAJE VÁLIDO
            }

            return false;
        }

        private void ShowValidationError()
        {
            MessageBox.Show(
                "Por favor, ingrese valores válidos:\n" +
                "- Temperatura: 0-150°C\n" +
                "- Carga: 0-100%",
                "Validación de Entrada",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        #endregion

        #region Validación de Teclado
        private void TxtHDD_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // ✅ VALIDACIÓN MÁS ROBUSTA DE ENTRADA NUMÉRICA
                if (IsNumericInput(e.Key) || IsControlKey(e.Key))
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                HandleError("Error en validación de teclado", ex);
                e.Handled = true;
            }
        }

        private static bool IsNumericInput(Key key)
        {
            // ✅ TECLAS NUMÉRICAS (TECLADO PRINCIPAL Y NUMPAD)
            return (key >= Key.D0 && key <= Key.D9) ||
                   (key >= Key.NumPad0 && key <= Key.NumPad9);
        }

        private static bool IsControlKey(Key key)
        {
            // ✅ TECLAS DE CONTROL PERMITIDAS
            return key == Key.Back ||
                   key == Key.Delete ||
                   key == Key.Left ||
                   key == Key.Right ||
                   key == Key.Tab ||
                   key == Key.Enter;
        }
        #endregion

        #region Eventos de Ventana
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                HandleError("Error al mover ventana", ex);
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

        #region Manejo de Errores
        private void HandleError(string message, Exception ex)
        {
            Debug.WriteLine($"{message}: {ex.Message}");

            // ✅ PODRÍAS AGREGAR MÁS LOGGING O NOTIFICACIONES AQUÍ
            // MessageBox.Show($"{message}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}