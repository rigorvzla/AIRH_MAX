using LLM_ServerMain.Models;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LLM_ServerMain
{
    /// <summary>
    /// Modo de ejecución del servidor
    /// </summary>
    public enum ServerMode
    {
        LocalExecutable,  // ✅ CAMBIADO: LocalPython → LocalExecutable
        OnlineAPI
    }

    /// <summary>
    /// Servidor LLM unificado para modos local y online
    /// </summary>
    public class LLMServer : IDisposable
    {
        private readonly LLMServerConfig _config;
        private Process _serverProcess; // ✅ CAMBIADO: _pythonProcess → _serverProcess
        private LLMClient _webSocketClient;
        private HttpClient _httpClient;
        private bool _disposed = false;

        #region Eventos
        public event EventHandler<ServerEventArgs> ServerStarted;
        public event EventHandler<ServerEventArgs> ServerStopped;
        public event EventHandler<ModelResponseEventArgs> ResponseReceived;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;
        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;
        #endregion

        #region Propiedades
        public bool IsRunning => _serverProcess != null && !_serverProcess.HasExited;
        public bool IsConnected => _webSocketClient?.IsConnected == true;
        public ServerMode CurrentMode => _config.Mode;
        public string ServerUrl => $"ws://localhost:{_config.Port}";
        #endregion

        #region Constructores
        public LLMServer(LLMServerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (config.Mode == ServerMode.LocalExecutable)
            {
                _webSocketClient = new LLMClient(ServerUrl, config.ModelsDirectory);
                SetupWebSocketEvents();
            }
            else
            {
                _httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromMinutes(5);
            }
        }

        public LLMServer() : this(new LLMServerConfig())
        {
        }

        private void SetupWebSocketEvents()
        {
            _webSocketClient.Connected += (s, e) => OnConnected(e);
            _webSocketClient.Disconnected += (s, e) => OnDisconnected(e);
            _webSocketClient.ResponseReceived += (s, e) => OnResponseReceived(e);
            _webSocketClient.ErrorOccurred += (s, e) => OnErrorOccurred(e);
        }
        #endregion

        #region Métodos Principales
        /// <summary>
        /// Inicia el servidor LLM
        /// </summary>
        public async Task<bool> StartAsync()
        {
            try
            {
                if (_config.Mode == ServerMode.LocalExecutable)
                {
                    return await StartLocalServerAsync();
                }
                else
                {
                    return await StartOnlineConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error al iniciar servidor: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
                return false;
            }
        }

        /// <summary>
        /// Detiene el servidor LLM
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                if (_config.Mode == ServerMode.LocalExecutable)
                {
                    StopLocalServer();
                    await _webSocketClient?.DisconnectAsync();
                }
                else
                {
                    _httpClient?.Dispose();
                    _httpClient = null;
                }

                OnServerStopped(new ServerEventArgs { Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error al detener servidor: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// Envía un mensaje al modelo
        /// </summary>
        public async Task<ModelResponse[]> SendMessageAsync(string message)
        {
            if (_config.Mode == ServerMode.LocalExecutable)
            {
                return await SendMessageLocalAsync(message);
            }
            else
            {
                return await SendMessageOnlineAsync(message);
            }
        }
        #endregion

        #region Servidor Local (Ejecutable)
        private async Task<bool> StartLocalServerAsync()
        {
            // ✅ VERIFICACIÓN MEJORADA: Buscar el ejecutable en varias ubicaciones
            string serverExePath = FindServerExecutable();
            if (string.IsNullOrEmpty(serverExePath))
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"No se encontró el servidor ejecutable. Buscado en: {_config.ServerExePath}",
                    Timestamp = DateTime.Now
                });
                return false;
            }

            // Verificar que el modelo existe
            var modelPath = Path.Combine(Environment.CurrentDirectory, _config.ModelsDirectory, $"{_config.ModelName}.gguf");
            if (!File.Exists(modelPath))
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Modelo no encontrado: {modelPath}",
                    Timestamp = DateTime.Now
                });
                return false;
            }

            // ✅ INICIAR PROCESO DEL EJECUTABLE
            var startInfo = new ProcessStartInfo
            {
                FileName = serverExePath,
                Arguments = BuildServerArguments(modelPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(serverExePath)
            };

            try
            {
                _serverProcess = new Process { StartInfo = startInfo };
                _serverProcess.OutputDataReceived += OnServerOutput;
                _serverProcess.ErrorDataReceived += OnServerError;

                Debug.WriteLine($"\n🚀 Iniciando proceso del servidor...\nModelo: {Path.GetFileNameWithoutExtension(modelPath)}");
                _serverProcess.Start();
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();

                // ✅ INICIALIZAR CLIENTE WEBSOCKET ANTES DE ESPERAR
                if (_webSocketClient == null)
                {
                    _webSocketClient = new LLMClient(ServerUrl, _config.ModelsDirectory);
                    SetupWebSocketEvents();
                }

                // Esperar a que el servidor esté listo
                return await WaitForServerReadyAsync(60); // 60 segundos de timeout
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error iniciando servidor: {ex.Message}");
                return false;
            }
        }

        private string FindServerExecutable()
        {
            // Buscar en varias ubicaciones posibles
            var possiblePaths = new List<string>
            {
                _config.ServerExePath, // Ruta directa
                Path.Combine(Directory.GetCurrentDirectory(), _config.ServerExePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ServerExePath),
                Path.Combine(Environment.CurrentDirectory, _config.ServerExePath)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    Debug.WriteLine($"✅ Servidor encontrado en: {path}");
                    return path;
                }
            }

            return null;
        }

        private string BuildServerArguments(string modelPath)
        {
            var args = new List<string>
    {
        $"--model \"{modelPath}\"",
        $"--port {_config.Port}",
        $"--ctx-size {_config.ContextSize}",
        $"--max-tokens {_config.MaxTokens}",
        $"--temperature {_config.Temperature.ToString(CultureInfo.InvariantCulture)}" // ✅ FORZAR PUNTO DECIMAL
    };

            // Solo agregar --prompt si tiene valor
            if (!string.IsNullOrEmpty(_config.SystemPrompt))
            {
                // Escapar comillas en el prompt
                var escapedPrompt = _config.SystemPrompt.Replace("\"", "\\\"");
                args.Add($"--prompt-text \"{escapedPrompt}\"");
            }

            // Usar --gpu-layers si está habilitado
            if (_config.UseGPU && _config.GpuLayers > 0)
            {
                args.Add($"--gpu-layers {_config.GpuLayers}");
            }

            Debug.WriteLine($"🔧 Argumentos del servidor: {string.Join(" ", args)}");
            return string.Join(" ", args);
        }

        private void OnServerOutput(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine($"SERVER: {e.Data}");

                // ✅ DETECCIÓN ACTUALIZADA para los nuevos mensajes del servidor
                if (e.Data.Contains("Servidor WebSocket iniciado") ||
                    e.Data.Contains("✅ Servidor WebSocket iniciado") ||
                    e.Data.Contains("✅ Servidor activo y listo") ||  // NUEVO
                    e.Data.Contains("listo para recibir conexiones") ||  // NUEVO
                    e.Data.Contains("listo") && e.Data.Contains("8766"))
                {
                    Debug.WriteLine("✅ Servidor detectado como listo - disparando evento ServerStarted");
                    OnServerStarted(new ServerEventArgs { Timestamp = DateTime.Now });
                }
            }
        }

        private void OnServerError(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"{e.Data}",
                    Timestamp = DateTime.Now
                });
            }
        }

        private void StopLocalServer()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill(true);
                    _serverProcess.WaitForExit(5000);
                    _serverProcess.Dispose();
                    _serverProcess = null;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error al detener proceso del servidor: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
            }
        }

        private async Task<ModelResponse[]> SendMessageLocalAsync(string message)
        {
            if (!_webSocketClient.IsConnected)
            {
                await _webSocketClient.ConnectAsync();
            }

            var completionSource = new TaskCompletionSource<ModelResponse[]>();
            EventHandler<ModelResponseEventArgs> handler = null;

            handler = (s, e) =>
            {
                _webSocketClient.ResponseReceived -= handler;
                completionSource.SetResult(new[] { e.Response });
            };

            _webSocketClient.ResponseReceived += handler;

            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2));
            var sendTask = completionSource.Task;

            // Usar el nuevo método con reintento automático
            await _webSocketClient.GenerateResponseAsync(message, _config.MaxTokens, _config.Temperature);

            var completedTask = await Task.WhenAny(sendTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                _webSocketClient.ResponseReceived -= handler;
                throw new TimeoutException("Timeout esperando respuesta del modelo");
            }

            return await sendTask;
        }
        #endregion

        #region Servidor Online (sin cambios)
        private async Task<bool> StartOnlineConnectionAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_config.ApiKey) || string.IsNullOrEmpty(_config.Endpoint))
                {
                    throw new InvalidOperationException("ApiKey y Endpoint son requeridos para modo online");
                }

                OnServerStarted(new ServerEventArgs { Timestamp = DateTime.Now });
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error en conexión online: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
                return false;
            }
        }

        private async Task<ModelResponse[]> SendMessageOnlineAsync(string message)
        {
            try
            {
                // ✅ 1. EVITAR USAR 'dynamic' Y TIPOS ANÓNIMOS
                // Crear el JSON manualmente para evitar problemas de serialización
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{");

                // ✅ 2. CONSTRUIR JSON MANUALMENTE
                jsonBuilder.Append($"\"model\":\"{EscapeJsonString(_config.ModelName)}\",");
                jsonBuilder.Append("\"messages\":[");

                // System prompt si existe
                if (!string.IsNullOrEmpty(_config.SystemPrompt))
                {
                    jsonBuilder.Append("{\"role\":\"system\",\"content\":\"" +
                                     EscapeJsonString(_config.SystemPrompt) + "\"},");
                }

                // Mensaje del usuario
                jsonBuilder.Append("{\"role\":\"user\",\"content\":\"" +
                                 EscapeJsonString(message) + "\"}");

                jsonBuilder.Append("],");
                jsonBuilder.Append($"\"max_tokens\":{_config.MaxTokens},");
                jsonBuilder.Append($"\"temperature\":{_config.Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                jsonBuilder.Append("}");

                string json = jsonBuilder.ToString();

                // ✅ 3. HTTP CLIENT CON CONFIGURACIÓN EXPLÍCITA
                using var httpClient = CreateHttpClient();
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ✅ 4. HEADERS DIRECTOS SIN USAR COLECCIONES COMPLEJAS
                if (!string.IsNullOrEmpty(_config.ApiKey))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Authorization",
                        $"Bearer {_config.ApiKey}"
                    );
                }

                // ✅ 5. LOG PARA DEBUG EN RELEASE
                System.Diagnostics.Debug.WriteLine($"[LLM] Enviando petición a: {_config.Endpoint}");
                System.Diagnostics.Debug.WriteLine($"[LLM] JSON: {json}");

                var response = await httpClient.PostAsync(_config.Endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"HTTP {response.StatusCode}: {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[LLM] Respuesta: {responseJson}");

                // ✅ 6. LLAMAR AL MÉTODO DE EXTRACCIÓN DE FORMA DIRECTA
                var result = ExtractModelResponseFromJson(responseJson);

                if (result == null || result.Length == 0)
                {
                    throw new InvalidOperationException("No se pudo extraer respuesta del JSON");
                }

                return result;
            }
            catch (Exception ex)
            {
                // ✅ 7. RESPUESTA DE ERROR ESTÁNDAR
                System.Diagnostics.Debug.WriteLine($"[LLM] Error: {ex}");
                return new ModelResponse[]
                {
                new ModelResponse
                {
                    model_response = $"Error de comunicación: {ex.Message}",
                    user_command = "error",
                    user_command_extra = new()
                }
                };
            }
        }

        // ✅ 8. MÉTODO AUXILIAR PARA ESCAPAR JSON
        private string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        // ✅ 9. CREACIÓN EXPLÍCITA DE HTTPCLIENT
        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();

            // Configuraciones específicas
            handler.UseDefaultCredentials = false;
            handler.PreAuthenticate = false;

            var client = new HttpClient(handler);

            // Timeouts explícitos
            client.Timeout = TimeSpan.FromSeconds(90);

            // Headers básicos
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        // ✅ NUEVO MÉTODO MEJORADO PARA EXTRAER LA RESPUESTA
        // ✅ MÉTODO CORREGIDO PARA EXTRAER LA RESPUESTA
        private ModelResponse[] ExtractModelResponseFromJson(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Buscar la respuesta en la estructura estándar de APIs
                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var content))
                    {
                        string responseText = content.GetString()?.Trim() ?? "";

                        System.Diagnostics.Debug.WriteLine($"[LLM] Respuesta cruda: {responseText}");

                        // ✅ PRIMERO: Intentar extraer y parsear el JSON interno
                        var extractedResponses = ExtractAndParseModelResponses(responseText);
                        if (extractedResponses != null && extractedResponses.Length > 0)
                        {
                            return extractedResponses;
                        }

                        // ✅ SEGUNDO: Si no hay JSON interno, usar la respuesta como texto simple
                        return new[] { new ModelResponse
                {
                    model_response = responseText,
                    user_command = ExtractCommandFromResponse(responseText),
                    user_command_extra = new ModelResponseExtra()
                }};
                    }
                }

                // ✅ FALLBACK: Si no se encuentra la estructura esperada
                return new[] { new ModelResponse
        {
            model_response = "Respuesta no procesable",
            user_command = "error",
            user_command_extra = new ModelResponseExtra()
        }};
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LLM] Error en ExtractModelResponseFromJson: {ex}");
                return new[] { new ModelResponse
        {
            model_response = $"Error procesando respuesta: {ex.Message}",
            user_command = "error",
            user_command_extra = new ModelResponseExtra()
        }};
            }
        }

        // ✅ NUEVO MÉTODO PARA EXTRAER Y PARSEAR MODELRESPONSES
        private ModelResponse[] ExtractAndParseModelResponses(string responseText)
        {
            try
            {
                // Buscar el array JSON interno [ ... ]
                int startIndex = responseText.IndexOf('[');
                int endIndex = responseText.LastIndexOf(']');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string jsonArray = responseText.Substring(startIndex, endIndex - startIndex + 1);

                    // Limpiar el JSON - quitar escapes y normalizar
                    jsonArray = jsonArray.Replace("\\n", "")
                                        .Replace("\\\"", "\"")
                                        .Replace("\\t", "")
                                        .Replace("\\r", "")
                                        .Trim();

                    System.Diagnostics.Debug.WriteLine($"[LLM] JSON extraído: {jsonArray}");

                    // Intentar deserializar
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    };

                    var responses = JsonSerializer.Deserialize<ModelResponse[]>(jsonArray, options);

                    if (responses != null && responses.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LLM] ✅ JSON parseado correctamente, {responses.Length} respuestas");
                        return responses;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[LLM] ❌ No se pudo extraer JSON válido");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LLM] ❌ Error en ExtractAndParseModelResponses: {ex}");
                return null;
            }
        }

        // ✅ MÉTODO PARA EXTRAER EL COMANDO DE LA RESPUESTA
        private string ExtractCommandFromResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return "chat";

            response = response.ToLower();

            if (response.Contains("buscar") || response.Contains("search"))
                return "buscar";
            else if (response.Contains("hora") || response.Contains("time"))
                return "hora_actual";
            else if (response.Contains("clima") || response.Contains("weather"))
                return "clima";
            else if (response.Contains("calcul") || response.Contains("math"))
                return "calculo";
            else if (response.Contains("error") || response.Contains("❌"))
                return "error";
            else
                return "chat";
        }
        #endregion

        #region Utilidades
        private async Task<bool> WaitForServerReadyAsync(int timeoutSeconds)
        {
            Debug.WriteLine("⏳ Esperando que el servidor esté listo...");

            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);
            int attempt = 0;

            while (DateTime.Now - startTime < timeout)
            {
                attempt++;

                // Verificar si el proceso del servidor sigue ejecutándose
                if (!IsRunning)
                {
                    Debug.WriteLine("❌ El proceso del servidor se detuvo");
                    return false;
                }

                try
                {
                    // ✅ ESPERAR ANTES DEL PRIMER INTENTO - DAR TIEMPO AL SERVIDOR
                    if (attempt == 1)
                    {
                        Debug.WriteLine("🔄 Dando tiempo al servidor para iniciar...");
                        await Task.Delay(3000); // Reducido a 3 segundos
                    }

                    Debug.WriteLine($"🔄 Intento de conexión #{attempt}...");

                    // ✅ USAR EL CLIENTE PRINCIPAL, NO CREAR UNO NUEVO
                    if (!_webSocketClient.IsConnected)
                    {
                        await _webSocketClient.ConnectAsync();
                    }

                    if (_webSocketClient.IsConnected)
                    {
                        Debug.WriteLine("✅ Conexión establecida - Servidor listo\n");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // ✅ MANEJO ESPECÍFICO DE ERRORES
                    if (ex is ObjectDisposedException)
                    {
                        Debug.WriteLine("⚠️ Cliente WebSocket no disponible, recreando...");
                        // Recrear el cliente si está disposed
                        _webSocketClient?.Dispose();
                        _webSocketClient = new LLMClient(ServerUrl, _config.ModelsDirectory);
                        SetupWebSocketEvents();
                    }
                    else if (ex.Message.Contains("Unable to connect") || ex.Message.Contains("10061"))
                    {
                        Debug.WriteLine($"⏰ Servidor aún no está listo (intento {attempt})...");
                    }
                    else
                    {
                        Debug.WriteLine($"⚠️ Error en intento {attempt}: {ex.Message}");
                    }

                    // ✅ ESPERAR PROGRESIVAMENTE MÁS TIEMPO
                    var waitTime = Math.Min(attempt * 1000, 3000); // Reducido: 1s, 2s, 3s...
                    if (DateTime.Now - startTime + TimeSpan.FromMilliseconds(waitTime) < timeout)
                    {
                        Debug.WriteLine($"🔄 Reintentando en {waitTime / 1000} segundos...");
                        await Task.Delay(waitTime);
                    }
                }
            }

            Debug.WriteLine($"❌ Timeout: No se pudo conectar después de {timeoutSeconds} segundos");
            return false;
        }

        /// <summary>
        /// Obtiene la lista de modelos disponibles localmente
        /// </summary>
        public List<string> GetAvailableModels()
        {
            var models = new List<string>();
            try
            {
                if (Directory.Exists(_config.ModelsDirectory))
                {
                    var modelFiles = Directory.GetFiles(_config.ModelsDirectory, "*.gguf");
                    foreach (var file in modelFiles)
                    {
                        models.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error leyendo directorio de modelos: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
            }
            return models;
        }

        /// <summary>
        /// Carga un modelo específico en el servidor local
        /// </summary>
        public async Task LoadModelAsync(string modelName)
        {
            if (_config.Mode == ServerMode.LocalExecutable && _webSocketClient.IsConnected)
            {
                var modelPath = Path.Combine(_config.ModelsDirectory, $"{modelName}.gguf");
                await _webSocketClient.LoadModelAsync(modelPath);
            }
        }
        #endregion

        #region Disparadores de Eventos
        protected virtual void OnServerStarted(ServerEventArgs e) => ServerStarted?.Invoke(this, e);
        protected virtual void OnServerStopped(ServerEventArgs e) => ServerStopped?.Invoke(this, e);
        protected virtual void OnResponseReceived(ModelResponseEventArgs e) => ResponseReceived?.Invoke(this, e);
        protected virtual void OnErrorOccurred(ErrorEventArgs e) => ErrorOccurred?.Invoke(this, e);
        protected virtual void OnConnected(ConnectionEventArgs e) => Connected?.Invoke(this, e);
        protected virtual void OnDisconnected(ConnectionEventArgs e) => Disconnected?.Invoke(this, e);
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (!_disposed)
            {
                StopAsync().Wait(5000);
                _serverProcess?.Dispose();
                _webSocketClient?.Dispose();
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
        #endregion
    }


    #region Modelos de Datos (Completos)
    public class ReconnectionEventArgs : EventArgs
    {
        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime Timestamp { get; set; }
    }
    internal class SimpleResponse
    {
        public string type { get; set; }
        public string message { get; set; }
    }

    public class ServerEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public string ServerUrl { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ModelResponseEventArgs : EventArgs
    {
        public ModelResponse Response { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public string RawMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PingEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
