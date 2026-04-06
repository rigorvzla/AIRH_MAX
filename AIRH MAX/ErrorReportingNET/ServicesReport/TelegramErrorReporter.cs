using ErrorReportingNET.Helper;
using System.IO;
using System.Net;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace ErrorReportingNET.ServicesReport;

public static class TelegramErrorReporter
{
    private static string _version = string.Empty;
    private static string _chatId = string.Empty;
    private static string _botToken = string.Empty;

    public static void Initialize(string version, string chatId, string botToken)
    {
        _version = version;
        _chatId = chatId;
        _botToken = botToken;
    }

    public static void SendTelegramReport(Exception exception, bool hasInnerException = false)
    {
        string report;
        if (hasInnerException && exception.InnerException != null)
        {
            report = $@"Producto: {Assembly.GetEntryAssembly()?.FullName.Split(',')[0].Replace(" ", "_")}
Version: {_version}

<b>Dispositivo</b>
Windows: {Environment.OSVersion}
Equipo: {Environment.MachineName}
Usuario: {Environment.UserName}

{PhysicalNetworkUsed.GetSimpleNetworkInfo()}

<b>Reporte de Error</b>
Mensaje: {exception.Message}

Excepción: {exception.InnerException.Message}

Source: {exception.InnerException.Source}

StackTrace: {exception.InnerException.StackTrace}

Lugar: {exception.TargetSite?.DeclaringType?.Name}";
        }
        else
        {
            report = $@"Producto: <b>{Assembly.GetEntryAssembly()?.FullName.Split(',')[0]}</b>
Version: {_version}

<b>Dispositivo</b>
Windows: {Environment.OSVersion}
Equipo: {Environment.MachineName}
Usuario: {Environment.UserName}

{PhysicalNetworkUsed.GetSimpleNetworkInfo()}

<b>Reporte de Error</b>
Mensaje: {exception.Message}

Lugar de error: {exception.TargetSite?.DeclaringType?.Name}";
        }

        SendReport(report);
    }

    // === ACCESO A INTERNET ===
    private static bool AccesoInternet()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry("www.google.com");
            return true;
        }
        catch
        {
            return false;
        }
    }

    // === ENVÍO ===
    private static void SendReport(string report)
    {
        string logsPath = Path.Combine(Environment.CurrentDirectory, "Logs");
        string app = Assembly.GetEntryAssembly()?.FullName.Split(',')[0];
        string logFile = Path.Combine(logsPath, $"log_{app}.txt");

        Directory.CreateDirectory(logsPath);
        File.WriteAllText(logFile, report);

        if (AccesoInternet())
        {
            try
            {
                TelegramBotClient bot = new TelegramBotClient(_botToken);
                bot.SendMessage(_chatId, report, parseMode: ParseMode.Html).GetAwaiter().GetResult();
            }
            catch
            {
                // Silencioso si Telegram falla
            }
        }
    }
}