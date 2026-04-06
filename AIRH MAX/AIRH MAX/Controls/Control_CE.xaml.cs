using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.APIs;
using AIRH_MAX.Views;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_CE : UserControl
    {
        public Control_CE()
        {
            InitializeComponent();
        }

        private async void sendEmail()
        {
            try
            {
                MainWindow.NotificacionEvent.MensajeBox = "Enviando mensaje";

                bool success = await EmailService.SendEmailAsync(
                    TextBoxDestino.Text,
                    txtAsunto.Text,
                    txtContenido.Text
                );

                if (success)
                {
                    MainWindow.NotificacionEvent.MensajeBox = "Mensaje enviado exitosamente";

                    txtAsunto.Text = "";
                    txtContenido.Text = "";
                }
                else
                {
                    MainWindow.NotificacionEvent.MensajeBox = "Error al enviar el mensaje";
                }
            }
            catch (Exception a)
            {
                MainWindow.NotificacionEvent.Log = $"Error: {a.Message}";
            }
        }

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            sendEmail();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
        }
    }
}
