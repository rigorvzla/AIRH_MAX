using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.ViewModel;
using System.Reflection;
using System.Windows;
using Telegram.Bot;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Comentario : UserControl
    {
        public Control_Comentario()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolicitudOnline.AccesoInternet())
            {
                TelegramBotClient Bot = new TelegramBotClient(ConstantElementsSecrets.TokenTelegramBot);

                string Reporte = $@"*Comentario*
Producto: {Assembly.GetEntryAssembly().FullName.Split(',')[0]}
Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString()}

Correo: *{correo.Text}*
Mensaje: {ComentarioBox.Text}";

                Telegram.Bot.Types.Message message = Bot.SendMessage(ConstantElementsSecrets.DeviceTelegram, Reporte, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown).Result;
                Xceed.Wpf.Toolkit.MessageBox.Show("Mensaje enviado, gracias por su comentario.", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No hay conexión a internet, para enviar el comentario", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
