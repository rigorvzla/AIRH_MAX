using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;

namespace AIRH_MAX.ClassView
{
    internal class Email
    {
        public static class PopMailKit
        {
            public static void CheckMail(string userName, string password, string hostName, int port, bool useSSL, string mensaje, string servidor)
            {
                try
                {
                    using var client = new Pop3Client();
                    client.Connect(hostName, port, useSSL);
                    client.Authenticate(userName, password);

                    int messageCount = client.GetMessageCount();
                    if (messageCount > 0)
                    {
                        Views.MainWindow.NotificacionEvent.MensajeBox = $"Tienes {messageCount} {mensaje}";
                    }

                    client.Disconnect(true);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    Views.MainWindow.NotificacionEvent.Log = "Error en la configuración: " + ex.Message;
                }
                catch (Exception ex)
                {
                    Views.MainWindow.NotificacionEvent.Log = $"No fue posible revisar tu correo. Revisa tu usuario, contraseña y servidor ({servidor}). Error: {ex.Message}";
                }
            }
        }

        public static class ImapMailKit
        {
            public static void CheckMail(string userName, string password, string hostName, int port, bool useSSL, string mensaje, string servidor)
            {
                try
                {
                    using var client = new ImapClient();
                    client.Connect(hostName, port, useSSL);
                    client.Authenticate(userName, password);
                    client.Inbox.Open(FolderAccess.ReadOnly);

                    int recentMessages = client.Inbox.Recent;
                    if (recentMessages > 0)
                    {
                        Views.MainWindow.NotificacionEvent.MensajeBox = $"Tienes {recentMessages} {mensaje}";
                    }

                    client.Disconnect(true);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    Views.MainWindow.NotificacionEvent.Log = "Error en la configuración: " + ex.Message;
                }
                catch (Exception ex)
                {
                    Views.MainWindow.NotificacionEvent.Log = $"No fue posible revisar tu correo. Revisa tu usuario, contraseña y servidor ({servidor}). Error: {ex.Message}";
                }
            }
        }

        //public static void popMailKit(string UserName, string Password, string HostName, int Port, bool useSSL, string mensaje, string servidor)
        //{
        //    try
        //    {
        //        using (var client = new Pop3Client())
        //        {
        //            client.Connect(HostName, Port, useSSL);
        //            client.Authenticate(UserName, Password);
        //            if (client.GetMessageCount() != 0)
        //            {
        //                MainWindow.NotificacionEvent.MensajeBox = "Tienes " + client.GetMessageCount() + mensaje;
        //            }
        //            client.Disconnect(true);
        //        }
        //    }
        //    catch
        //    {
        //        MainWindow.NotificacionEvent.Log = "No fue posible revisar tu correo, revisa tu usario, contraseña y servidor. revisa la configuracion de correo " + servidor;
        //    }
        //}

        //public static void imapMailKit(string UserName, string Password, string HostName, int Port, bool useSSL, string mensaje, string servidor)
        //{
        //    try
        //    {
        //        using (var client = new ImapClient())
        //        {
        //            client.Connect(HostName, Port, useSSL);
        //            client.Authenticate(UserName, Password);
        //            client.Inbox.Open(FolderAccess.ReadOnly);
        //            if (client.Inbox.Recent != 0)
        //            {
        //                MainWindow.NotificacionEvent.MensajeBox = "Tienes " + client.Inbox.Recent + mensaje;
        //            }
        //            client.Disconnect(true);
        //        }
        //    }
        //    catch
        //    {
        //        MainWindow.NotificacionEvent.Log = "No fue posible revisar tu correo, revisa tu usario, contraseña y servidor. revisa la configuracion de correo " + servidor;
        //    }
        //}
    }
}