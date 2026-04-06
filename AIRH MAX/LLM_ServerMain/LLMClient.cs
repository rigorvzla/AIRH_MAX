using LLM_ServerMain;
using LLM_ServerMain.Models;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ErrorEventArgs = LLM_ServerMain.ErrorEventArgs;

/// <summary>
/// Cliente WebSocket para comunicación con servidores locales
/// </summary>
/// <summary>
/// Cliente WebSocket para comunicación con servidores locales con autoreconexión
/// </summary>
internal class LLMClient : IDisposable
{
    private ClientWebSocket _webSocket;
    private readonly string _serverUrl;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed = false;
    private bool _isReconnecting = false;
    private readonly object _reconnectLock = new object();
    private int _reconnectAttempts = 0;
    private const int MAX_RECONNECT_ATTEMPTS = 5;
    private const int RECONNECT_DELAY_MS = 2000;

    public event EventHandler<ConnectionEventArgs> Connected;
    public event EventHandler<ConnectionEventArgs> Disconnected;
    public event EventHandler<ModelResponseEventArgs> ResponseReceived;
    public event EventHandler<ErrorEventArgs> ErrorOccurred;
    public event EventHandler<PingEventArgs> PingReceived;
    public event EventHandler<ReconnectionEventArgs> ReconnectionAttempt;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    public string ModelsDirectory { get; set; }

    public LLMClient(string serverUrl, string modelsDirectory)
    {
        _serverUrl = serverUrl;
        ModelsDirectory = modelsDirectory;
        _cancellationTokenSource = new CancellationTokenSource();
        _webSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync()
    {
        try
        {
            if (_webSocket.State == WebSocketState.Open)
                return;

            await _webSocket.ConnectAsync(new Uri(_serverUrl), _cancellationTokenSource.Token);
            _reconnectAttempts = 0; // Reset reconnection attempts on successful connection
            OnConnected(new ConnectionEventArgs { ServerUrl = _serverUrl, Timestamp = DateTime.Now });

            // Iniciar escucha en segundo plano
            _ = Task.Run(StartListening);
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = $"Error de conexión: {ex.Message}",
                Exception = ex,
                Timestamp = DateTime.Now
            });

            // Intentar reconexión automática
            await AttemptReconnectionAsync();
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            }
            OnDisconnected(new ConnectionEventArgs { ServerUrl = _serverUrl, Timestamp = DateTime.Now });
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = $"Error al desconectar: {ex.Message}",
                Exception = ex,
                Timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// Intenta reconectarse automáticamente cuando se pierde la conexión
    /// </summary>
    private async Task AttemptReconnectionAsync()
    {
        lock (_reconnectLock)
        {
            if (_isReconnecting || _reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
                return;
            _isReconnecting = true;
        }

        try
        {
            while (_reconnectAttempts < MAX_RECONNECT_ATTEMPTS && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _reconnectAttempts++;

                OnReconnectionAttempt(new ReconnectionEventArgs
                {
                    Attempt = _reconnectAttempts,
                    MaxAttempts = MAX_RECONNECT_ATTEMPTS,
                    Timestamp = DateTime.Now
                });

                Debug.WriteLine($"🔄 Intento de reconexión #{_reconnectAttempts}...");

                try
                {
                    // Dispose del WebSocket anterior
                    _webSocket?.Dispose();

                    // Crear nuevo WebSocket
                    _webSocket = new ClientWebSocket();

                    // Intentar conectar
                    await _webSocket.ConnectAsync(new Uri(_serverUrl), _cancellationTokenSource.Token);

                    // Conexión exitosa
                    _reconnectAttempts = 0;
                    _isReconnecting = false;

                    OnConnected(new ConnectionEventArgs { ServerUrl = _serverUrl, Timestamp = DateTime.Now });
                    Debug.WriteLine("✅ Reconexión exitosa");

                    // Reiniciar la escucha
                    _ = Task.Run(StartListening);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Intento de reconexión #{_reconnectAttempts} falló: {ex.Message}");

                    if (_reconnectAttempts < MAX_RECONNECT_ATTEMPTS)
                    {
                        // Esperar antes del siguiente intento (con backoff exponencial)
                        var delay = RECONNECT_DELAY_MS * _reconnectAttempts;
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
            }

            // Si llegamos aquí, todos los intentos fallaron
            _isReconnecting = false;
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = $"No se pudo reconectar después de {MAX_RECONNECT_ATTEMPTS} intentos",
                Timestamp = DateTime.Now
            });
        }
        catch (OperationCanceledException)
        {
            // Cancelación normal durante la reconexión
            _isReconnecting = false;
        }
    }

    /// <summary>
    /// Envía un mensaje con manejo de reconexión automática
    /// </summary>
    public async Task SendMessageWithRetryAsync(object message)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                if (_webSocket.State != WebSocketState.Open)
                {
                    throw new InvalidOperationException("WebSocket no conectado");
                }

                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    _cancellationTokenSource.Token
                );
                return; // Éxito, salir del bucle
            }
            catch (Exception ex) when (attempt < 3)
            {
                Debug.WriteLine($"⚠️ Error enviando mensaje (intento {attempt}): {ex.Message}");

                // Intentar reconectar antes del siguiente intento
                await AttemptReconnectionAsync();

                // Esperar un poco antes de reintentar
                await Task.Delay(1000 * attempt);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ErrorEventArgs
                {
                    ErrorMessage = $"Error enviando mensaje después de 3 intentos: {ex.Message}",
                    Exception = ex,
                    Timestamp = DateTime.Now
                });
                throw;
            }
        }
    }

    public async Task GenerateResponseAsync(string userMessage, int maxTokens = 512, float temperature = 0.1f)
    {
        var message = new
        {
            type = "generate",
            message = userMessage,
            max_tokens = maxTokens,
            temperature = temperature
        };
        await SendMessageWithRetryAsync(message);
    }

    public async Task PingAsync()
    {
        var message = new { type = "ping" };
        await SendMessageWithRetryAsync(message);
    }

    public async Task LoadModelAsync(string modelPath)
    {
        var message = new { type = "reload_model", model_path = modelPath };
        await SendMessageWithRetryAsync(message);
    }

    private async Task StartListening()
    {
        var buffer = new byte[1024 * 16]; // 16KB buffer

        try
        {
            while (_webSocket.State == WebSocketState.Open &&
                   !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token
                    );
                }
                catch (WebSocketException ex)
                {
                    Debug.WriteLine($"⚠️ WebSocket exception en recepción: {ex.Message}");
                    // Intentar reconexión automática
                    _ = Task.Run(AttemptReconnectionAsync);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessReceivedMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    OnDisconnected(new ConnectionEventArgs { ServerUrl = _serverUrl, Timestamp = DateTime.Now });

                    // Intentar reconexión automática cuando se cierra la conexión
                    _ = Task.Run(AttemptReconnectionAsync);
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación normal, no es un error
        }
        catch (WebSocketException ex)
        {
            Debug.WriteLine($"🔌 WebSocket desconectado: {ex.Message}");
            OnDisconnected(new ConnectionEventArgs { ServerUrl = _serverUrl, Timestamp = DateTime.Now });

            // Intentar reconexión automática
            _ = Task.Run(AttemptReconnectionAsync);
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = $"Error en recepción: {ex.Message}",
                Exception = ex,
                Timestamp = DateTime.Now
            });

            // Intentar reconexión automática en caso de error general
            _ = Task.Run(AttemptReconnectionAsync);
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        try
        {
            // Intentar deserializar como array de ModelResponse (respuesta del servidor)
            var modelResponses = JsonSerializer.Deserialize<List<ModelResponse>>(message);
            if (modelResponses != null && modelResponses.Count > 0)
            {
                OnResponseReceived(new ModelResponseEventArgs
                {
                    Response = modelResponses[0],
                    Timestamp = DateTime.Now
                });
                return;
            }

            // Intentar deserializar como respuesta simple (ping, etc.)
            var simpleResponse = JsonSerializer.Deserialize<SimpleResponse>(message);
            if (simpleResponse != null)
            {
                switch (simpleResponse.type)
                {
                    case "pong":
                        OnPingReceived(new PingEventArgs { Timestamp = DateTime.Now });
                        break;
                    default:
                        Debug.WriteLine($"Respuesta simple recibida: {simpleResponse.type}");
                        break;
                }
                return;
            }

            // Si no se pudo deserializar, tratar como mensaje de error
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = "Formato de respuesta no reconocido",
                RawMessage = message,
                Timestamp = DateTime.Now
            });
        }
        catch (JsonException ex)
        {
            OnErrorOccurred(new ErrorEventArgs
            {
                ErrorMessage = $"Error deserializando JSON: {ex.Message}",
                Exception = ex,
                RawMessage = message,
                Timestamp = DateTime.Now
            });
        }
    }

    #region Event Handlers
    protected virtual void OnConnected(ConnectionEventArgs e) => Connected?.Invoke(this, e);
    protected virtual void OnDisconnected(ConnectionEventArgs e) => Disconnected?.Invoke(this, e);
    protected virtual void OnResponseReceived(ModelResponseEventArgs e) => ResponseReceived?.Invoke(this, e);
    protected virtual void OnErrorOccurred(ErrorEventArgs e) => ErrorOccurred?.Invoke(this, e);
    protected virtual void OnPingReceived(PingEventArgs e) => PingReceived?.Invoke(this, e);
    protected virtual void OnReconnectionAttempt(ReconnectionEventArgs e) => ReconnectionAttempt?.Invoke(this, e);
    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
}

