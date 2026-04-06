using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clipboard = System.Windows.Forms.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace VerifyLi
{
    public partial class Activador : Window
    {
        public Activador()
        {
            InitializeComponent();
            metodoPago.SelectionChanged += MetodoPago_SelectionChanged;
        }

        private void MetodoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (metodoPago.SelectedItem is ComboBoxItem item)
            {
                string metodo = item.Content.ToString();
                groupPago.Visibility = Visibility.Visible;

                switch (metodo)
                {
                    case "PayPal":
                        textDireccionPago.Text = "📧 Paga a:\nrigorrico@gmail.com\n(vía PayPal)\nMonto:25$ Minimo";
                        break;

                    case "Zinli":
                        textDireccionPago.Text = "📱 Zinli Pago:\nrigorrico@gmail.com\n(vía Zinli)\nMonto:25$ Minimo";
                        break;

                    case "Pago Movil (Solo Venezuela)":
                        textDireccionPago.Text = "🏦 Transferencia Bancaria:\nCedula: 18466560\nBanco: Provincial\nTeléfono: 04147495968\nMonto:25$ Minimo";
                        break;

                    case "Cripto (USDT)":
                        textDireccionPago.Text = "🪙 Criptomonedas:\nUSDT (TRC-20): TMJNgo6...\nMonto:25$ Minimo";
                        break;

                    default:
                        groupPago.Visibility = Visibility.Collapsed;
                        return;
                }
            }

            this.InvalidateMeasure();
            this.UpdateLayout();
        }

        private void groupPago_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (groupPago.Visibility != Visibility.Visible)
                return;

            string metodo = (metodoPago.SelectedItem as ComboBoxItem)?.Content.ToString();
            string textoACopiar = "";

            switch (metodo)
            {
                case "PayPal":
                    textoACopiar = "rigorrico@gmail.com";
                    textDireccionPago.Text = "📧 rigorrico@gmail.com";
                    break;

                case "Zinli":
                    textoACopiar = "rigorrico@gmail.com";
                    textDireccionPago.Text = "📱 rigorrico@gmail.com";
                    break;

                case "Pago Movil (Solo Venezuela)":
                    textoACopiar = "18466560\n04147495968\nProvincial";
                    textDireccionPago.Text = "🏦 Cuenta: Banco Provincial";
                    break;

                case "Cripto (USDT)":
                    textoACopiar = "TMJNgo6sZLjC2GJ3gKu4KLCPerp8ENyXG3";
                    textDireccionPago.Text = "🪙 USDT (TRC-20):\nTMJNgo6...";
                    break;
            }

            try
            {
                Clipboard.SetText(textoACopiar);
                MessageBox.Show($"✅ {textoACopiar}\n\nSe ha copiado al portapapeles.", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ No se pudo copiar al portapapeles: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            KeyManager.ActivarLicencia(textLice.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var camposRequeridos = new (object Control, string Nombre)[]
            {
                (textnombre, "Nombre"),
                (textcorreo, "Correo"),
                (textcomprobante, "Comprobante"),
                (metodoPago, "Método de pago")
            };

            foreach (var (Control, Nombre) in camposRequeridos)
            {
                string valor = Control switch
                {
                    TextBox tb => tb.Text,
                    ComboBox cb => cb.Text,
                    _ => string.Empty
                };

                if (string.IsNullOrWhiteSpace(valor))
                {
                    MessageBox.Show(
                        $"El campo '{Nombre}' no puede estar vacío.",
                        "Campos incompletos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
            }

            BotLicencia.Solicitud(textnombre.Text, metodoPago.Text, textcomprobante.Text, textcorreo.Text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imágenes (*.jpg, *.jpeg, *.png, *.gif, *.bmp, *.webp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|Todos los archivos (*.*)|*.*";
            openFileDialog.Title = "Seleccionar una imagen";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textcomprobante.Text = openFileDialog.FileName;
            }
        }
    }
}
