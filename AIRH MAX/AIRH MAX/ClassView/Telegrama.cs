using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Views;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AIRH_MAX.ClassView
{
    internal class Telegrama : Window
    {
        public static readonly ITelegramBotClient Bot = new TelegramBotClient(ConstantElementsSecrets.TokenTelegramBot);

        public static async Task TelegramStart()
        {
            var token = new CancellationTokenSource();
            var canceltoken = token.Token;

            ReceiverOptions ReOpt = new ReceiverOptions();
            ReOpt.AllowedUpdates = Array.Empty<UpdateType>();

            Engrane.ni.ShowBalloonTip(10000, "Conexion establecida en:", "Telegram", ToolTipIcon.None);

            await Bot.SendMessage(Engrane.File_Config().ID_Telegram, $@"*Bienvenido:* {Engrane.File_Config().Usuario}
Comunicación establecida con *¡Exito!*
Selecciona ""Una opción del Menú""
===========================
Modo de Uso:
Escribe la acción seguido de "":"" luego el texto y envialo, asi AV-AIRH ejecutara la acción solicitada
Voz: (_Texto a enviar_)
===========================", parseMode: ParseMode.Markdown, replyMarkup: MenuComandos());

            await Bot.ReceiveAsync(OnMessage, ErrorMessage, ReOpt, canceltoken);
        }

        private static async Task ErrorMessage(ITelegramBotClient client, Exception exception, CancellationToken token)
        {

        }

        private static async Task OnMessage(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                EntradaComandos(update.CallbackQuery.Data, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Type);
                return;
            }

            var message = update.Message;
            if (message == null) return;

            switch (message.Type)
            {
                case MessageType.Voice:
                    VoiceCatcherAsync(update);
                    break;

                case MessageType.Text:
                    var text = message.Text.Trim().ToLower();
                    if (text.StartsWith("voz:"))
                    {
                        MainWindow.NotificacionEvent.MensajeBox = text.Split(':', 2)[1];
                    }
                    EntradaMensaje(text, message.Chat.Id, message.From?.Username);
                    break;
            }
        }

        private static InlineKeyboardMarkup MenuComandos()
        {
            var buttons = new List<InlineKeyboardButton[]>
            {
                // Primera fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("\U0001F603 Saludos \U0001F603", "/hola")
                },

                // Segunda fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Apagar PC", "/apagar_equipo"),
                    InlineKeyboardButton.WithCallbackData("Reiniciar PC", "/reiniciar_equipo")
                },

                // Tercera fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Cancelar Apagado", "/cancelar_apagado"),
                    InlineKeyboardButton.WithCallbackData("Info PC", "/informacion_pc")
                },

                // Cuarta fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Procesos PC Full", "/lista_de_procesos_detallados"),
                    InlineKeyboardButton.WithCallbackData("Procesos PC Lite", "/lista_de_procesos")
                },

                // Quinta fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Screenshot", "/foto_de_pantalla"),
                    InlineKeyboardButton.WithCallbackData("Screenshot All", "/foto_de_pantalla_All"),
                    InlineKeyboardButton.WithCallbackData("¿Intruso?", "/foto_intruso")
                },

                // Sexta fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Comandos Aplicaciones", "/comandos_aplicaciones"),
                    InlineKeyboardButton.WithCallbackData("Comandos Sociales", "/comandos_sociales")
                },

                // Séptima fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Comandos Webs", "/comandos_webs"),
                    InlineKeyboardButton.WithCallbackData("Comandos Carpetas", "/comandos_carpetas")
                },

                // Octava fila
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Comandos Arduino", "/comandos_arduino")
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }

        private static void EntradaMensaje(string Texto, long ChatId, string From)
        {
            if (ChatId.Equals(Convert.ToInt32(Engrane.File_Config().ID_Telegram)))
            {
                MainWindow.NotificacionEvent.Pizarra = From + "(!)" + Texto;
            }
        }

        private static void EntradaComandos(string Texto, long ChatId, MessageType type)
        {
            if (ChatId.ToString().Equals(Engrane.File_Config().ID_Telegram) && Texto != null)
            {
                if (Texto.Equals("Menú"))
                {
                    Bot.SendMessage(Engrane.File_Config().ID_Telegram, string.Empty, replyMarkup: MenuComandos());
                }
                if (Texto == "/comandos_arduino")
                {
                    ListaComandos("Arduino");
                }
                if (Texto == "/comandos_sociales")
                {
                    ListaComandos("Social");
                }
                if (Texto == "/comandos_webs")
                {
                    ListaComandos("Web");
                }
                if (Texto == "/comandos_aplicaciones")
                {
                    ListaComandos("App");
                }
                if (Texto == "/comandos_carpetas")
                {
                    ListaComandos("Carpetas");
                }

                if (type == MessageType.Text)
                {
                    string source = Texto;
                    string[] resto;

                    if (Texto.ToLower().Split(':')[0] == "ce")
                    {
                        foreach (Process proc in Process.GetProcessesByName(Texto.ToLower().Split(':')[1].Trim()))
                        {
                            if (proc.ProcessName != "AIRH")
                            {
                                proc.Kill();
                                Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Proceso cerrado");
                            }
                        }
                    }

                    if (source == "/foto_de_pantalla")
                    {
                        try
                        {
                            var img = Engrane.File_Config().Dir_Pantalla_Capturas + "\\" + Screenshots.Screen_Primary();
                            var f = InputFile.FromStream(System.IO.File.Open(img, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read));
                            Bot.SendPhoto(Engrane.File_Config().ID_Telegram, f, caption: DateTime.Now.ToString());

                        }
                        catch (Exception a)
                        {
                            Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Error al enviar la captura");
                        }
                    }
                    if (source == "/foto_de_pantalla_All")
                    {
                        try
                        {
                            int mon = 1;
                            var img = Screenshots.Screen_All();
                            foreach (var item in img)
                            {
                                var f = InputFile.FromStream(System.IO.File.Open(item, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read));
                                Bot.SendPhoto(Engrane.File_Config().ID_Telegram, f, caption: DateTime.Now.ToString() + $" Monitor {mon++}");
                            }

                        }
                        catch (Exception a)
                        {
                            Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Error al enviar la captura");
                        }
                    }
                    if (source == "/hola")
                    {
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, $"Hola {Engrane.File_Config().Usuario} Computador encendido");
                    }
                    if (source == "/apagar_equipo")
                    {
                        Process.Start("shutdown.exe", "-s -t 30");
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Apagando sistema");
                    }
                    if (source == "/reiniciar_equipo")
                    {
                        Process.Start("shutdown.exe", "-r -t 30");
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Reiniciando sistema");
                    }
                    if (source == "/cancelar_apagado")
                    {
                        Process.Start("shutdown.exe", "/a");
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Desactivación del equipo cancelada");
                    }
                    if (source == "/informacion_pc")
                    {
                        DriveInfo[] drives = DriveInfo.GetDrives();
                        foreach (DriveInfo drive in drives)
                        {
                            if (drive.IsReady)
                            {
                                long total1 = drive.TotalSize;
                                long libre1 = drive.TotalFreeSpace;
                                long resultado1 = total1 - libre1;
                                Bot.SendMessage(Engrane.File_Config().ID_Telegram, $"*Dispositivos de Almacenamiento*" + Environment.NewLine +
    $"*Nombre:* " + drive.Name + Environment.NewLine +
    $"*Etiqueta:* " + drive.VolumeLabel + Environment.NewLine +
    $"*Tipo de Unidad:* " + drive.DriveType.ToString() + Environment.NewLine +
    $"*Tipo de Formato:* " + drive.DriveFormat + Environment.NewLine +
    $"*Espacio Usado:* " + Support.NormalizeFileSize(resultado1) + Environment.NewLine +
    $"*Espacio Libre:* " + Support.NormalizeFileSize(drive.TotalFreeSpace) + Environment.NewLine +
    $"*Tamaño Total:* " + Support.NormalizeFileSize(drive.TotalSize) + Environment.NewLine + Environment.NewLine, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                        }

                        var pcInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                        ulong total2 = pcInfo.TotalPhysicalMemory;
                        ulong libre2 = pcInfo.AvailablePhysicalMemory;
                        ulong resultado2 = total2 - libre2;
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram,
    $"*Memoria RAM usada:* " + Support.NormalizeFileSize(Convert.ToInt64(resultado2)) + Environment.NewLine +
    $"*Memoria RAM libre:* " + Support.NormalizeFileSize(Convert.ToInt64(pcInfo.AvailablePhysicalMemory)) + Environment.NewLine +
    $"*Memoria RAM total:* " + Support.NormalizeFileSize(Convert.ToInt64(pcInfo.TotalPhysicalMemory)), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    }
                    if (source == "/foto_intruso")
                    {
                        Task.Run(() =>
                        {
                            string intruso = WebCam.TomarFotoIntruso().Result;
                            System.Threading.Thread.Sleep(6000);

                            try
                            {
                                var fa = InputFile.FromStream(System.IO.File.Open(intruso, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read));
                                Bot.SendPhoto(Engrane.File_Config().ID_Telegram, fa, caption: DateTime.Now.ToString());
                            }
                            catch (Exception)
                            {
                                Bot.SendMessage(Engrane.File_Config().ID_Telegram, "Camara no detectada");
                            }
                        });
                    }
                    if (source == "/lista_de_procesos_detallados")
                    {
                        Process[] procesos2;
                        procesos2 = Process.GetProcesses();
                        InlineKeyboardMarkup menu;
                        List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

                        foreach (Process pro in procesos2)
                        {
                            if (pro.ProcessName != "AIRH")
                            {
                                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(pro.ProcessName, "ce:" + pro.ProcessName) });
                            }
                        }


                        menu = new InlineKeyboardMarkup(buttons.ToArray());
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, $"Procesos activos: {procesos2.Length}", replyMarkup: menu);

                    }
                    if (source == "/lista_de_procesos")
                    {
                        Process[] procesos2;
                        procesos2 = Process.GetProcesses();
                        InlineKeyboardMarkup menu;
                        List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

                        foreach (Process pro in procesos2)
                        {
                            if (!string.IsNullOrEmpty(pro.MainWindowTitle))
                            {
                                if (pro.ProcessName != "AIRH")
                                {
                                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(pro.ProcessName, "ce:" + pro.ProcessName) });
                                }
                            }
                        }

                        menu = new InlineKeyboardMarkup(buttons.ToArray());
                        Bot.SendMessage(Engrane.File_Config().ID_Telegram, $"Procesos activos: {procesos2.Length}", replyMarkup: menu);
                    }
                }
            }
        }

        private static void ListaComandos(string Tabla)
        {
            InlineKeyboardMarkup menu;
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            var cmd = DB_Lite.ObtenerRegistros(Tabla, "comando");

            if (cmd.Count.Equals(0))
            {
                Bot.SendMessage(Engrane.File_Config().ID_Telegram, "No hay comandos registrados");
            }
            else
            {
                foreach (var pro in cmd)
                {
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(pro, pro) });
                }

                menu = new InlineKeyboardMarkup(buttons.ToArray());
                Bot.SendMessage(Engrane.File_Config().ID_Telegram, $"Comandos {Tabla}: {cmd.Count}", replyMarkup: menu);
            }
        }

        private static async void VoiceCatcherAsync(Update e)
        {
            string temp = Environment.CurrentDirectory + "\\Temp";
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            var fileId = e.Message.Voice.FileId;
            var fileInfo = await Bot.GetFile(fileId);
            var filePath = fileInfo.FilePath;
            string rutaSalida = temp + "\\voice.ogg";

            using (FileStream fileStream = System.IO.File.OpenWrite(rutaSalida))
            {
                await Bot.DownloadFile(filePath: filePath, destination: fileStream);
                fileStream.Close();
                fileStream.Dispose();
            }

            PlayerSpy.Player(rutaSalida);
        }
    }
}
