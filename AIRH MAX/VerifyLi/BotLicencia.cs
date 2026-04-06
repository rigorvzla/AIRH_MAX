using System.IO;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace VerifyLi
{
    internal class BotLicencia
    {
        private static ITelegramBotClient Activador = new TelegramBotClient(Engrane.TokenTelegramBot);
        private static int IDUser = Engrane.DeviceTelegram;
        private static string appName = System.Reflection.Assembly.GetEntryAssembly().FullName.Split(',')[0].ToString();
        private static string mac = KeyManager.GetPrimaryMacAddress;


        private static InlineKeyboardMarkup MenuComandos()
        {
            var firstRow = new[]
            {
                InlineKeyboardButton.WithCallbackData("\u2705 Activar \u2705", "0"),
                InlineKeyboardButton.WithCallbackData("\U0001F6AB Bloquear \U0001F6AB", "1"),
            };

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[] { firstRow });
            return inlineKeyboard;
        }

        /// <summary> <c></c> Solicitud de bloqueo</summary>  
        public static void Solicitud(string Nombre, string Pago, string imagen, string Correo)
        {
            string Datos = $@"***<b>Solicitud de Licencia</b>*** 
{"Producto: " + appName}
{"Nombre: " + Nombre}
{"Correo: " + Correo}
{"Metodo de Pago: " + Pago}
{"MAC: " + mac}
";

            ActivacionBot();
            Activador.SendPhoto(IDUser, InputFile.FromStream(System.IO.File.Open(imagen, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetFileName(imagen)), caption: Datos, parseMode: ParseMode.Html, replyMarkup: MenuComandos());
            System.Windows.MessageBox.Show("Se ha enviado su solicitud no cierre el programa para su correcta activación", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private static void ActivacionBot()
        {
            Task.Run(() =>
            {
                var token = new CancellationTokenSource();
                var canceltoken = token.Token;

                ReceiverOptions ReOpt = new ReceiverOptions();
                ReOpt.AllowedUpdates = Array.Empty<UpdateType>();

                Activador.ReceiveAsync(OnMessage, ErrorMessage, ReOpt, canceltoken);
            });
        }

        private static async Task ErrorMessage(ITelegramBotClient client, Exception e, CancellationToken token)
        {
            if (e is ApiRequestException requestException)
            {

            }
        }

        private static string KeyGen()
        {
            string[] formatos = { "N", "D", "B", "P", "X" };
            Guid guid = Guid.NewGuid();
            Random random = new Random();
            string formatoElegido = formatos[random.Next(formatos.Length)];
            return guid.ToString(formatoElegido);
        }

        private static async Task OnMessage(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.CallbackQuery == null)
            {
                return;
            }

            if (update.CallbackQuery.From.Id.Equals(IDUser))
            {
                if (update.CallbackQuery.Data.Equals("0"))
                {
                    string licencia = KeyGen();
                    await KeyManager.SolicitarLicenciaAsync(licencia);

                    string Datos = $@"***<b>Programa Activado</b>*** 
{"Producto: " + appName}
{"Licencia: " + licencia}
{"MAC: " + mac}";

                    string DatosCliente = $@"***<b>Programa Activado</b>*** 
{"Producto: " + appName}
{"Licencia: " + licencia}";

                    await Activador.SendMessage(IDUser, Datos, parseMode: ParseMode.Html);
                    string ruta = Path.Combine(Environment.CurrentDirectory, "Licencia AIRH MAX.txt");
                    File.WriteAllText(ruta, DatosCliente);
                    System.Windows.MessageBox.Show($"El programa se ha activado, disfrute de su asistente\nGuarde en un lugar seguro la licencia y no la comparta.\nSe guardo en:{ruta}", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    Application.Exit();
                }
                else
                {
                    System.Windows.MessageBox.Show("Se ha rechazado su solicitud, intentelo nuevamente mas tarde", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    await Activador.Close();
                }
            }
        }
    }
}