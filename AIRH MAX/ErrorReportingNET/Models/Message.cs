using Discord;

namespace ErrorReportingNET.Models
{
    public class Message
    {
        public enum Type
        {
            Error,
            Warning,
            Info,
            Success
        }

        public static Color GetColor(Type type)
        {
            return type switch
            {
                Type.Error => new Color(255, 0, 0),
                Type.Warning => new Color(255, 165, 0),
                Type.Info => new Color(28, 102, 156),
                Type.Success => new Color(51, 184, 67),
                _ => new Color(255, 255, 255)
            };
        }

        public static string GetReportType(Type type)
        {
            return type switch
            {
                Type.Error => "Error",
                Type.Warning => "Advertencia",
                Type.Info => "Información",
                Type.Success => "Success",
                _ => "Unknown"
            };
        }
    }
}
