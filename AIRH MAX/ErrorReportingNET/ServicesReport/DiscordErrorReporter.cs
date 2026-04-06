using ErrorReportingNET.Helper;
using ErrorReportingNET.Models;
using ErrorReportingNET.Services;
using System.Reflection;

namespace ErrorReportingNET.ServicesReport;

public static class DiscordErrorReporter
{
    private static string _version = string.Empty;

    public static void Initialize(string version)
    {
        _version = version;
    }

    public static void SendDiscordReport(Exception exception, bool hasInnerException = false)
    {
        string reportD;
        if (hasInnerException && exception.InnerException != null)
        {
            reportD = $@"
Mensaje: {exception.Message}

Excepción: {exception.InnerException.Message}

Source: {exception.InnerException.Source}

StackTrace: {exception.InnerException.StackTrace}

Lugar: {exception.TargetSite?.DeclaringType?.Name}";
        }
        else
        {
            reportD = $@"
Mensaje: {exception.Message}
Lugar de error: {exception.TargetSite?.DeclaringType?.Name}";
        }

        var reportForDiscord = new Report
        {
            Version = _version,
            Dispositivo = $"Windows: {Environment.OSVersion}\r\nEquipo: {Environment.MachineName}\r\nUsuario: {Environment.UserName}",
            Producto = Assembly.GetEntryAssembly()?.FullName.Split(',')[0].Replace(" ", "_"),
            WebhookUrl = "https://discord.com/api/webhooks/1474124900536156240/QRyeq4GZDKHlmpGagulBGj_tWVr_xVgY89Iq12KBByn3UcLEqjV7mEOCltWvb-kjFV16",
            Descripcion = reportD,
            Red = PhysicalNetworkUsed.GetNetworkInterfaceInfo(),
            Gps = PhysicalNetworkUsed.GetGeolocationInfoText()
        };

        DiscordMessage.SendEmbedAsync(reportForDiscord, Message.Type.Error);
    }
}