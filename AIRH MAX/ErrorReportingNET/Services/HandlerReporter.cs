using ErrorReportingNET.ServicesReport;
using System.Windows;

namespace ErrorReportingNET.Services;

public static class HandlerReporter
{
    public static void Initialize(string version, string chatId, string botToken)
    {
        // Inicializar los reporteros específicos
        //TelegramErrorReporter.Initialize(version, chatId, botToken);
        DiscordErrorReporter.Initialize(version);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        Application.Current.DispatcherUnhandledException += OnDispatcherException;
    }

    private static void OnDispatcherException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        bool hasInnerException = e.Exception.InnerException != null;

        // Enviar reporte a Telegram
        //TelegramErrorReporter.SendTelegramReport(e.Exception, hasInnerException);

        // Enviar reporte a Discord
        DiscordErrorReporter.SendDiscordReport(e.Exception, hasInnerException);

        e.Handled = true;
        System.Threading.Thread.Sleep(3000);
        Environment.Exit(0);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception ex)
        {
            bool hasInnerException = ex.InnerException != null;

            // Enviar reporte a Telegram
            //TelegramErrorReporter.SendTelegramReport(ex, hasInnerException);

            // Enviar reporte a Discord
            DiscordErrorReporter.SendDiscordReport(ex, hasInnerException);
        }
    }
}