using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace AIRH_MAX.Models
{
    // Serializador personalizado para mensajes WebSocket
    public static class WebSocketJsonSerializer
    {
        public static string Serialize(object message)
        {
            if (message is WebSocketMessage wsMsg)
            {
                return $"{{\"type\":\"{EscapeJson(wsMsg.Type)}\",\"message\":\"{EscapeJson(wsMsg.Message)}\"}}";
            }
            else if (message is WebSocketPing ping)
            {
                return "{\"type\":\"ping\"}";
            }
            else if (message is System.Collections.Generic.Dictionary<string, object> dict)
            {
                var parts = new List<string>();
                foreach (var kvp in dict)
                {
                    parts.Add($"\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value?.ToString() ?? "")}\"");
                }
                return $"{{{string.Join(",", parts)}}}";
            }
            else
            {
                // Para objetos anónimos, usar reflexión simple
                var props = message.GetType().GetProperties();
                var jsonParts = new List<string>();
                foreach (var prop in props)
                {
                    var value = prop.GetValue(message);
                    if (value != null)
                    {
                        jsonParts.Add($"\"{EscapeJson(prop.Name)}\":\"{EscapeJson(value.ToString())}\"");
                    }
                }
                return $"{{{string.Join(",", jsonParts)}}}";
            }
        }

        public static WebSocketMessage DeserializeWebSocketMessage(string json)
        {
            var result = new WebSocketMessage();

            try
            {
                // Parseo manual del JSON
                var dict = ParseJsonToDictionary(json);

                if (dict.ContainsKey("type"))
                    result.Type = dict["type"];
                if (dict.ContainsKey("message"))
                    result.Message = dict["message"];
                if (dict.ContainsKey("username"))
                    result.Username = dict["username"];
                if (dict.ContainsKey("timestamp"))
                    result.Timestamp = dict["timestamp"];
                if (dict.ContainsKey("total_users") && int.TryParse(dict["total_users"], out int totalUsers))
                    result.TotalUsers = totalUsers;
                if (dict.ContainsKey("your_username"))
                    result.YourUsername = dict["your_username"];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deserializando: {ex.Message}");
            }

            return result;
        }

        private static Dictionary<string, string> ParseJsonToDictionary(string json)
        {
            var dict = new Dictionary<string, string>();

            try
            {
                json = json.Trim();
                if (json.StartsWith("{") && json.EndsWith("}"))
                {
                    json = json.Substring(1, json.Length - 2);

                    var inString = false;
                    var currentKey = "";
                    var currentValue = "";
                    var keyMode = true;
                    var escapeNext = false;

                    for (int i = 0; i < json.Length; i++)
                    {
                        char c = json[i];

                        if (escapeNext)
                        {
                            if (keyMode)
                                currentKey += c;
                            else
                                currentValue += c;
                            escapeNext = false;
                            continue;
                        }

                        if (c == '\\')
                        {
                            escapeNext = true;
                            continue;
                        }

                        if (c == '"')
                        {
                            inString = !inString;
                            continue;
                        }

                        if (!inString)
                        {
                            if (c == ':' && keyMode)
                            {
                                keyMode = false;
                                continue;
                            }

                            if (c == ',' || (i == json.Length - 1 && !keyMode))
                            {
                                if (!string.IsNullOrEmpty(currentKey))
                                {
                                    dict[currentKey.Trim()] = currentValue.Trim();
                                    currentKey = "";
                                    currentValue = "";
                                    keyMode = true;
                                }
                                continue;
                            }
                        }

                        if (keyMode)
                            currentKey += c;
                        else
                            currentValue += c;
                    }

                    // Último valor si no terminó con coma
                    if (!string.IsNullOrEmpty(currentKey) && !keyMode)
                    {
                        dict[currentKey.Trim()] = currentValue.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parseando JSON: {ex.Message}");
            }

            return dict;
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }
    }

    // Clase para mensajes WebSocket entrantes/salientes
    public class WebSocketMessage
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string Timestamp { get; set; }
        public int TotalUsers { get; set; }
        public string YourUsername { get; set; }
    }

    // Clase para ping
    public class WebSocketPing
    {
        public string Type { get; set; } = "ping";
    }

    public class ChatMessage
    {
        public string Tipo { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsOwnMessage { get; set; }
        public bool IsSystem { get; set; }
        public bool IsWelcome { get; set; }
        public int TotalUsers { get; set; }
        public bool IsError { get; set; }

        public string DisplayTime => Timestamp.ToString("hh:mm tt");

        public static ChatMessage FromServerJson(string type, string username, string message, string timestamp, int totalUsers, bool isOwn = false)
        {
            DateTime parsedTime;
            if (!DateTime.TryParse(timestamp, out parsedTime))
            {
                if (DateTime.TryParseExact(timestamp, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out parsedTime))
                {
                    parsedTime = DateTime.Today.Add(parsedTime.TimeOfDay);
                }
                else
                {
                    parsedTime = DateTime.Now;
                }
            }

            return new ChatMessage
            {
                Tipo = type,
                UserName = username,
                Message = message,
                Timestamp = parsedTime,
                IsOwnMessage = isOwn,
                IsSystem = (type == "system" || type == "welcome"),
                IsWelcome = (type == "welcome"),
                TotalUsers = totalUsers
            };
        }

        public static ChatMessage SystemMessage(string message, bool isError = false)
        {
            return new ChatMessage
            {
                Tipo = "system",
                UserName = "Sistema",
                Message = message,
                Timestamp = DateTime.Now,
                IsSystem = true,
                IsError = isError
            };
        }

        public Brush MessageBackground
        {
            get
            {
                if (IsSystem) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                if (IsOwnMessage) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCF8C6"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            }
        }

        public Brush UserNameColor
        {
            get
            {
                if (IsSystem) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                if (IsOwnMessage) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            }
        }

        public HorizontalAlignment MessageAlignment
        {
            get
            {
                if (IsSystem) return HorizontalAlignment.Center;
                if (IsOwnMessage) return HorizontalAlignment.Right;
                return HorizontalAlignment.Left;
            }
        }
    }

    public class ChatUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}