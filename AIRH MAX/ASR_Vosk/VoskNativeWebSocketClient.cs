using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Vosk_STT
{
    public class VoskNativeWebSocketClient : IDisposable
    {
        private ClientWebSocket _webSocket;
        private readonly string _serverUrl;
        private bool _isConnected;
        private WaveInEvent _waveIn;
        private CancellationTokenSource _cancellation;
        private ProfessionalAudioProcessor _audioProcessor;
        private Process _serverProcess;
        private readonly string _serverExePath;

        public MicrophoneManager MicrophoneManager { get; private set; }

        public event Action<string> OnConnected;
        public event Action<string> OnTranscription;
        public event Action<string> OnPartialTranscription;
        public event Action<string> OnError;
        public event Action<string> OnDisconnected;
        public event Action<string> OnServerStarted;

        // NUEVO: Eventos para medidor de voz
        public event Action<float> OnVoiceLevel;      // Para barra de progreso (0-1)
        public event Action<bool> OnVoiceActivity;    // Para indicador de actividad
        public event Action<string> OnVoiceMeterInfo; // Información del medidor

        public VoskNativeWebSocketClient(string serverIp = "localhost", int port = 8764, string serverExePath = "/stt/VoskWebSocketServer.exe")
        {
            _serverUrl = $"ws://{serverIp}:{port}";
            _serverExePath = serverExePath;
            _webSocket = new ClientWebSocket();
            _cancellation = new CancellationTokenSource();
            
            // INICIALIZAR PROCESADOR
            _audioProcessor = new ProfessionalAudioProcessor();

            // CONECTAR EVENTOS DEL PROCESADOR
            _audioProcessor.OnVoiceLevelChanged += (level) =>
            {
                OnVoiceLevel?.Invoke(level);
            };

            _audioProcessor.OnVoiceActivityChanged += (isActive) =>
            {
                OnVoiceActivity?.Invoke(isActive);
            };

            _audioProcessor.OnVoiceLevelInfo += (info) =>
            {
                OnVoiceMeterInfo?.Invoke(info);
            };

            // INICIALIZAR MicrophoneManager
            MicrophoneManager = new MicrophoneManager();
            MicrophoneManager.OnAudioDataAvailable += OnMicrophoneDataAvailable;
            MicrophoneManager.OnError += (error) => OnError?.Invoke($"[MicManager] {error}");
            MicrophoneManager.OnMicrophoneChanged += (message) => OnTranscription?.Invoke($"[MicManager] {message}");

            Debug.WriteLine("🎤 Cliente Vosk con medidor de voz en tiempo real");
        }

        // Handler para datos de audio del MicrophoneManager - MEJORADO
        private async void OnMicrophoneDataAvailable(WaveInEventArgs e)
        {
            if (_isConnected && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    // Usar el procesador profesional de audio
                    byte[] processedAudio = _audioProcessor.ProcessAudio(e.Buffer, e.BytesRecorded);

                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(processedAudio, 0, processedAudio.Length),
                        WebSocketMessageType.Binary,
                        true,
                        _cancellation.Token
                    );
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"❌ Error enviando audio: {ex.Message}");
                }
            }
        }

        // NUEVO: Método para calibrar supresión de ruido
        public async Task CalibrateNoiseSuppression()
        {
            try
            {
                OnTranscription?.Invoke("🔧 Calibrando supresión de ruido... No hables por 2 segundos.");

                // Detener temporalmente el streaming si está activo
                bool wasStreaming = MicrophoneManager?.IsRecording == true;
                if (wasStreaming)
                {
                    MicrophoneManager.StopRecording();
                    await Task.Delay(100);
                }

                // Calibrar
                _audioProcessor.CalibrateNoiseFloor();

                // Reanudar si estaba grabando
                if (wasStreaming)
                {
                    await Task.Delay(2100); // Tiempo de calibración
                    MicrophoneManager.StartRecording();
                    OnTranscription?.Invoke("✅ Calibración completada - Supresión de ruido activa");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error en calibración: {ex.Message}");
            }
        }

        // NUEVO: Control de procesamiento de audio
        public void EnableAudioProcessing(bool enable)
        {
            _audioProcessor.EnableProcessing(enable);
            OnTranscription?.Invoke(enable ? "✅ Procesamiento de audio ACTIVADO" : "⏸️ Procesamiento de audio PAUSADO");
        }

        public void SetVoiceMeterSensitivity(float sensitivity)
        {
            _audioProcessor.SetVoiceMeterSensitivity(sensitivity);
        }

        public void SetVoiceMeterSmoothing(float smoothing)
        {
            _audioProcessor.SetVoiceMeterSmoothing(smoothing);
        }

        public string GetVoiceMeterInfo()
        {
            var info = _audioProcessor.GetVoiceLevelInfo();
            return $"🔊 Medidor de voz: Nivel={info.CurrentLevel:P0} | Pico={info.PeakLevel:P0} | Activo={info.IsVoiceActive}";
        }

        public async Task<bool> StartServerAsync()
        {
            try
            {
                // Verificar si el servidor ya está ejecutándose
                if (await IsServerRunningAsync())
                {
                    OnServerStarted?.Invoke("✅ Servidor Vosk ya está en ejecución");
                    return true;
                }

                // Verificar que el ejecutable existe
                if (!File.Exists(_serverExePath))
                {
                    OnError?.Invoke($"❌ No se encuentra el servidor: {_serverExePath}");
                    return false;
                }

                // Iniciar el servidor
                var startInfo = new ProcessStartInfo
                {
                    FileName = _serverExePath,
                    WorkingDirectory = Path.GetDirectoryName(_serverExePath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _serverProcess = new Process { StartInfo = startInfo };

                // Capturar salida del servidor
                _serverProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine($"[Servidor] {e.Data}");
                        if (e.Data.Contains("INICIADO") || e.Data.Contains("Esperando conexiones"))
                        {
                            OnServerStarted?.Invoke("🚀 Servidor Vosk iniciado correctamente");
                        }
                    }
                };

                _serverProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine($"VSK {e.Data}");
                    }
                };

                if (_serverProcess.Start())
                {
                    _serverProcess.BeginOutputReadLine();
                    _serverProcess.BeginErrorReadLine();

                    // Esperar a que el servidor esté listo
                    OnServerStarted?.Invoke("⏳ Iniciando servidor Vosk...");

                    for (int i = 0; i < 30; i++) // Timeout de 30 intentos (15 segundos)
                    {
                        if (await IsServerRunningAsync())
                        {
                            OnServerStarted?.Invoke("✅ Servidor Vosk listo y escuchando en puerto 8765");
                            return true;
                        }
                        await Task.Delay(500);
                    }

                    OnError?.Invoke("❌ Timeout: El servidor no respondió en 15 segundos");
                    return false;
                }
                else
                {
                    OnError?.Invoke("❌ No se pudo iniciar el proceso del servidor");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error iniciando servidor: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> IsServerRunningAsync()
        {
            try
            {
                using var testClient = new ClientWebSocket();
                var cancellationToken = new CancellationTokenSource(1000).Token; // Timeout de 1 segundo

                await testClient.ConnectAsync(new Uri(_serverUrl), cancellationToken);
                await testClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test", cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ConnectAsync(bool autoStartServer = true)
        {
            try
            {
                // Si se solicita auto-inicio y no hay conexión, iniciar servidor
                if (autoStartServer && !await IsServerRunningAsync())
                {
                    OnConnected?.Invoke("🔧 Iniciando servidor automáticamente...");
                    if (!await StartServerAsync())
                    {
                        OnError?.Invoke("❌ No se pudo conectar al servidor");
                        return;
                    }
                }

                await _webSocket.ConnectAsync(new Uri(_serverUrl), _cancellation.Token);
                _isConnected = true;

                // Iniciar escucha de mensajes
                _ = Task.Run(ReceiveMessages);

                OnConnected?.Invoke("✅ Conectado al servidor Vosk");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error conectando: {ex.Message}");

                // Intentar auto-inicio si falla la conexión
                if (autoStartServer)
                {
                    OnConnected?.Invoke("🔄 Intentando iniciar servidor...");
                    if (await StartServerAsync())
                    {
                        // Reintentar conexión después de iniciar el servidor
                        await Task.Delay(2000);
                        await ConnectAsync(false); // No auto-iniciar nuevamente
                    }
                }
            }
        }

        public async Task StartRealtimeStreaming()
        {
            if (!_isConnected)
            {
                OnError?.Invoke("❌ No conectado al servidor");
                return;
            }

            try
            {
                // OPCIÓN 1: Usar MicrophoneManager (recomendado)
                if (MicrophoneManager != null)
                {
                    // Configurar el formato y buffer si no está configurado
                    if (MicrophoneManager.CurrentWaveIn == null)
                    {
                        MicrophoneManager.SetWaveFormat(16000, 16, 1);
                        MicrophoneManager.SetBufferSize(50, 3);
                    }

                    // Iniciar grabación con el micrófono seleccionado
                    if (MicrophoneManager.StartRecording())
                    {
                        var currentMic = MicrophoneManager.GetCurrentMicrophone();
                        OnTranscription?.Invoke($"🎤 Transmitiendo desde: {currentMic.Name} con PROCESAMIENTO PROFESIONAL DE AUDIO");
                        OnTranscription?.Invoke($"🔊 Características: Supresión de ruido + EQ vocal + Compresión + Detección inteligente de voz");
                    }
                    else
                    {
                        OnError?.Invoke("❌ No se pudo iniciar la grabación con el micrófono seleccionado");
                    }
                }
                // OPCIÓN 2: Usar WaveInEvent directamente (fallback)
                else
                {
                    // Detener streaming anterior si existe
                    _waveIn?.StopRecording();
                    _waveIn?.Dispose();

                    // Configurar micrófono por defecto
                    _waveIn = new WaveInEvent
                    {
                        WaveFormat = new WaveFormat(16000, 16, 1),
                        BufferMilliseconds = 50
                    };

                    _waveIn.DataAvailable += async (s, e) =>
                    {
                        if (_isConnected && _webSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                // Usar el procesador profesional de audio
                                byte[] processedAudio = _audioProcessor.ProcessAudio(e.Buffer, e.BytesRecorded);

                                await _webSocket.SendAsync(
                                    new ArraySegment<byte>(processedAudio, 0, processedAudio.Length),
                                    WebSocketMessageType.Binary,
                                    true,
                                    _cancellation.Token
                                );
                            }
                            catch (Exception ex)
                            {
                                OnError?.Invoke($"❌ Error enviando audio: {ex.Message}");
                            }
                        }
                    };

                    _waveIn.StartRecording();
                    OnTranscription?.Invoke("🎤 Transmitiendo audio con PROCESAMIENTO PROFESIONAL DE AUDIO");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error iniciando stream: {ex.Message}");
            }
        }

        public async Task StopStreaming()
        {
            try
            {
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;

                MicrophoneManager.StopRecording();
                OnTranscription?.Invoke("🛑 Stream detenido");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error deteniendo stream: {ex.Message}");
            }
        }

        public async Task StopServerAsync()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    await _serverProcess.WaitForExitAsync(); //genera error al detener
                    _serverProcess.Dispose();
                    _serverProcess = null;
                    OnDisconnected?.Invoke("🛑 Servidor Vosk detenido");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error deteniendo servidor: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[4096];

            while (_isConnected && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellation.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessServerMessage(message);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_isConnected)
                    {
                        OnError?.Invoke($"❌ Error recibiendo mensaje: {ex.Message}");
                    }
                    break;
                }
            }
        }

        private void ProcessServerMessage(string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);

                if (doc.RootElement.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetString();

                    switch (type)
                    {
                        case "connected":
                            if (doc.RootElement.TryGetProperty("message", out var messageElement))
                            {
                                OnConnected?.Invoke("🔗 " + messageElement.GetString());
                            }
                            else
                            {
                                OnConnected?.Invoke("🔗 Conectado al servidor");
                            }
                            break;

                        case "final":
                            if (doc.RootElement.TryGetProperty("text", out var textElement))
                            {
                                var text = textElement.GetString();
                                if (!string.IsNullOrEmpty(text))
                                {
                                    OnTranscription?.Invoke(text);
                                }
                            }
                            break;

                        case "partial":
                            if (doc.RootElement.TryGetProperty("text", out var partialTextElement))
                            {
                                var text = partialTextElement.GetString();
                                if (!string.IsNullOrEmpty(text))
                                {
                                    OnPartialTranscription?.Invoke(text);
                                }
                            }
                            break;

                        case "error":
                            if (doc.RootElement.TryGetProperty("message", out var errorElement))
                            {
                                OnError?.Invoke("❌ " + errorElement.GetString());
                            }
                            break;
                    }
                }
                else if (doc.RootElement.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    if (status == "success")
                    {
                        OnConnected?.Invoke("✅ Servidor respondiendo");
                    }
                }
                else
                {
                    OnError?.Invoke($"❌ Mensaje inesperado: {message}");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error procesando mensaje: {ex.Message}");
            }
        }

        public void SetAudioProcessingMode(string mode)
        {
            try
            {
                switch (mode.ToLower())
                {
                    case "closevoice":
                    case "priority":
                        _audioProcessor.SetCloseVoicePriority();
                        OnTranscription?.Invoke("🔊 Modo: Voz cercana prioritaria (sonidos lejanos atenuados)");
                        break;

                    case "balanced":
                        _audioProcessor.SetBalancedMode();
                        OnTranscription?.Invoke("⚖️ Modo: Balanceado");
                        break;

                    case "sensitive":
                        _audioProcessor.SetSensitiveMode();
                        OnTranscription?.Invoke("🎯 Modo: Sensible");
                        break;

                    default:
                        OnError?.Invoke($"❌ Modo no reconocido: {mode}. Usando prioridad de voz cercana.");
                        _audioProcessor.SetCloseVoicePriority();
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error cambiando modo: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            _isConnected = false;
            _cancellation.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnect",
                    CancellationToken.None
                );
            }

            _webSocket?.Dispose();
            _waveIn?.Dispose();
            _audioProcessor?.Dispose();

            // Detener servidor si fue iniciado por esta instancia
            await StopServerAsync();

            OnDisconnected?.Invoke("🔌 Desconectado");
        }

        public void KillVoskProcess()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("VoskWebSocketServer"))
                {
                    process.Kill();
                    process.Dispose();
                }
            }
            catch
            {
                // Si falla, no hacemos nada
            }
        }

        public void Dispose()
        {
            try
            {
                // 1. Matar servidor inmediatamente
                _serverProcess?.Kill();
                _serverProcess?.Dispose();

                // 2. Detener audio
                _waveIn?.StopRecording();
                _waveIn?.Dispose();

                // 3. Cerrar conexión
                _webSocket?.Dispose();
                _audioProcessor?.Dispose();
                _cancellation?.Cancel();

                Debug.WriteLine("🔌 Todos los recursos liberados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error limpiando: {ex.Message}");
            }
        }
    }
}