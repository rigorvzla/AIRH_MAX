using LLM_ServerMain;
using System.Diagnostics;

namespace AIRH_MAX.ClassView.IA
{
    /// <summary>
    /// Clase para manejar la plataforma del servidor LLM (Local y Online)
    /// </summary>
    [System.Reflection.Obfuscation(Feature = "all", Exclude = true)]
    public static class ServerPlataform
    {
        private static LLMServer _currentServer;
        private static LLMServerConfig _currentConfig;

        /// <summary>
        /// Inicia el servidor LLM (local u online)
        /// </summary>
        public static async Task<string> ServerLLM_StartAsync(string apiKey, string modelName, string systemPrompt, string endpoint, bool Local = true)
        {
            try
            {
                // Detener servidor anterior si existe
                ServerLocal_Stop();

                var config = new LLMServerConfig
                {
                    Mode = Local ? ServerMode.LocalExecutable : ServerMode.OnlineAPI,
                    ModelName = modelName,
                    ApiKey = apiKey,
                    Endpoint = endpoint,
                    SystemPrompt = systemPrompt,
                    ModelsDirectory = "llm\\models",
                    MaxTokens = 512,
                    Temperature = 0.1f,
                    ContextSize = 5632
                };

                _currentConfig = config;
                _currentServer = new LLMServer(config);

                // Suscribirse a eventos para manejo de errores y reconexión
                _currentServer.ErrorOccurred += (s, e) =>
                {
                    Debug.WriteLine($"{e.ErrorMessage}");
                };

                _currentServer.Disconnected += (s, e) =>
                {
                    Debug.WriteLine("🔌 Servidor desconectado - se intentará reconectar automáticamente");
                };

                _currentServer.Connected += (s, e) =>
                {
                    Debug.WriteLine("✅ Servidor reconectado exitosamente");
                };

                bool started = await _currentServer.StartAsync();

                if (started)
                {
                    return Local ? "Servidor Local Iniciado" : "Conexión Online Establecida";
                }
                else
                {
                    return "Error: No se pudo iniciar el servidor";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Envía un mensaje al servidor activo (local u online)
        /// </summary>
        public static async Task<string> SendMessage(string message)
        {
            try
            {
                if (_currentServer == null)
                {
                    return "Error: Servidor no iniciado";
                }

                var responses = await _currentServer.SendMessageAsync(message);

                if (responses == null || responses.Length == 0)
                {
                    return "Error: No se recibió respuesta";
                }

                // Serializar las respuestas a JSON
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = null,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                return System.Text.Json.JsonSerializer.Serialize(responses, jsonOptions);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }


        /// <summary>
        /// Obtiene la lista de nombres de modelos disponibles localmente
        /// </summary>
        public static List<string> GetModelNames()
        {
            try
            {
                if (_currentServer != null)
                {
                    return _currentServer.GetAvailableModels();
                }
                else
                {
                    // Crear un servidor temporal solo para leer los modelos
                    var tempConfig = new LLMServerConfig { ModelsDirectory = "llm/models" };
                    var tempServer = new LLMServer(tempConfig);
                    return tempServer.GetAvailableModels();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error obteniendo modelos: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtiene el primer modelo disponible localmente
        /// </summary>
        public static string GetFirstModelName()
        {
            var models = GetModelNames();
            return models?.FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Detiene el servidor local
        /// </summary>
        public static void ServerLocal_Stop()
        {
            try
            {
                if (_currentServer != null)
                {
                    _currentServer.StopAsync().Wait(5000);
                    _currentServer.Dispose();
                    _currentServer = null;
                    _currentConfig = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deteniendo servidor: {ex.Message}");
            }
        }
    }
}