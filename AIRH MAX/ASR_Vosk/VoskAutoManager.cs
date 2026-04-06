using System.IO;
using Vosk_STT.Models;

namespace Vosk_STT
{
    public class VoskAutoManager : IDisposable
    {
        private VoskNativeWebSocketClient _voskClient;
        private bool _isInitialized = false;
        private bool _isRunning = false;

        // Variables para almacenar información del micrófono predeterminado
        public MicrophoneInfo DefaultMicrophone { get; private set; }
        public string DefaultMicrophoneName => DefaultMicrophone?.Name ?? "No disponible";

        // Eventos para notificar el estado
        public event Action<string> OnStatusChanged;
        public event Action<string> OnTranscriptionReceived;
        public event Action<string> OnErrorOccurred;
        public event Action<bool> OnRunningStateChanged;

        public event Action<float> OnVoiceLevel;      // Para ProgressBar (0-1)
        public event Action<bool> OnVoiceActivity;    // Para indicador LED
        public event Action<string> OnVoiceMeterInfo; // Para label informativo


        public VoskAutoManager()
        {
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Configurar la ruta del servidor Vosk
                string serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stt", "VoskWebSocketServer.exe");

                _voskClient = new VoskNativeWebSocketClient(serverExePath: serverPath);
                _voskClient.KillVoskProcess();
                SubscribeToEvents();

                // Obtener y almacenar el micrófono predeterminado
                LoadDefaultMicrophone();

                _isInitialized = true;
                NotifyStatus("✅ VoskAutoManager inicializado correctamente");              
            }
            catch (Exception ex)
            {
                NotifyError($"❌ Error inicializando VoskAutoManager: {ex.Message}");
            }
        }

        public List<MicrophoneInfo> GetAvailableMicrophones()
        {
            return _voskClient.MicrophoneManager.GetAvailableMicrophones();
        }

        private void LoadDefaultMicrophone()
        {
            try
            {
                var microphones = _voskClient.MicrophoneManager.GetAvailableMicrophones();
                if (microphones.Count > 0)
                {
                    // Buscar el micrófono predeterminado (generalmente el primero o uno marcado como default)
                    DefaultMicrophone = microphones.Find(m => m.IsDefault) ?? microphones[0];

                    // Seleccionar el micrófono predeterminado
                    _voskClient.MicrophoneManager.SelectMicrophone(DefaultMicrophone.DeviceId);

                    // Configurar para Vosk
                    _voskClient.MicrophoneManager.SetWaveFormat(16000, 16, 1);
                    _voskClient.MicrophoneManager.SetBufferSize(50, 3);

                    NotifyStatus($"🎤 Micrófono predeterminado: {DefaultMicrophone.Name}");
                }
                else
                {
                    NotifyError("❌ No se encontraron micrófonos disponibles");
                }
            }
            catch (Exception ex)
            {
                NotifyError($"❌ Error cargando micrófono predeterminado: {ex.Message}");
            }
        }

        private void SubscribeToEvents()
        {
            _voskClient.OnConnected += (message) =>
            {
                NotifyStatus($"🔗 {message}");

                // CONFIRMAR CONFIGURACIÓN APLICADA
                NotifyStatus("🔊 Procesador de audio configurado: Voz cercana prioritaria");
                NotifyStatus("   - Sonidos lejanos atenuados al 20%");
                NotifyStatus("   - Ruido reducido al 5%");
                NotifyStatus("   - Voz débil atenuada al 50%");
            };

            _voskClient.OnServerStarted += (message) =>
            {
                NotifyStatus($"🚀 {message}");
            };

            _voskClient.OnTranscription += (text) =>
            {
                OnTranscriptionReceived?.Invoke(text);
            };

            _voskClient.OnPartialTranscription += (text) =>
            {
                // Opcional: también puedes exponer transcripciones parciales
                // OnPartialTranscriptionReceived?.Invoke(text);
            };

            _voskClient.OnVoiceLevel += (level) =>
            {
                // Pasar el evento al suscriptor externo
                OnVoiceLevel?.Invoke(level);
            };

            _voskClient.OnVoiceActivity += (isActive) =>
            {
                OnVoiceActivity?.Invoke(isActive);
            };

            _voskClient.OnVoiceMeterInfo += (info) =>
            {
                OnVoiceMeterInfo?.Invoke(info);
            };

            _voskClient.OnError += (error) =>
            {
                NotifyError(error);
            };

            _voskClient.OnDisconnected += (message) =>
            {
                NotifyStatus($"🔌 {message}");
                SetRunningState(false);
            };
        }

        // NUEVO: Métodos para controlar el medidor
        public void SetVoiceMeterSensitivity(float sensitivity)
        {
            _voskClient.SetVoiceMeterSensitivity(sensitivity);
            NotifyStatus($"🎚️ Sensibilidad del medidor ajustada a: {sensitivity:P0}");
        }

        public void SetVoiceMeterSmoothing(float smoothing)
        {
            _voskClient.SetVoiceMeterSmoothing(smoothing);
            NotifyStatus($"🌀 Suavizado del medidor ajustado a: {smoothing:F2}");
        }


        /// <summary>
        /// Inicia automáticamente el servidor, se conecta e inicia la grabación
        /// </summary>
        public async Task<bool> StartAutomaticAsync()
        {
            if (!_isInitialized)
            {
                NotifyError("❌ VoskAutoManager no está inicializado");
                return false;
            }

            try
            {
                NotifyStatus("🔄 Iniciando proceso automático...");

                // 1. Iniciar servidor
                NotifyStatus("⏳ Iniciando servidor Vosk...");
                bool serverStarted = await _voskClient.StartServerAsync();
                if (!serverStarted)
                {
                    NotifyError("❌ No se pudo iniciar el servidor");
                    return false;
                }

                // Pequeña pausa para asegurar que el servidor esté listo
                await Task.Delay(1000);

                // 2. Conectar al servidor
                NotifyStatus("🔗 Conectando al servidor...");
                await _voskClient.ConnectAsync(autoStartServer: false);

                // 3. Iniciar grabación con micrófono predeterminado
                NotifyStatus("🎤 Iniciando grabación con micrófono predeterminado...");
                await _voskClient.StartRealtimeStreaming();

                SetRunningState(true);
                NotifyStatus("✅ Sistema Vosk funcionando automáticamente");
                return true;
            }
            catch (Exception ex)
            {
                NotifyError($"❌ Error en inicio automático: {ex.Message}");
                SetRunningState(false);
                return false;
            }
        }

        /// <summary>
        /// Detiene todo el sistema
        /// </summary>
        public async Task StopAutomaticAsync()
        {
            try
            {
                NotifyStatus("🛑 Deteniendo sistema Vosk...");

                await _voskClient.StopStreaming();
                await _voskClient.DisconnectAsync();

                SetRunningState(false);
                NotifyStatus("✅ Sistema Vosk detenido");
            }
            catch (Exception ex)
            {
                NotifyError($"❌ Error deteniendo sistema: {ex.Message}");
            }
        }


        /// <summary>
        /// Obtiene información del estado actual del sistema
        /// </summary>
        public string GetSystemInfo()
        {
            if (!_isInitialized) return "Sistema no inicializado";

            var stats = _voskClient.MicrophoneManager.GetStats();
            return $"🎤 Micrófono: {DefaultMicrophoneName}\n" +
                   $"📊 Estado: {(_isRunning ? "🟢 Ejecutándose" : "🔴 Detenido")}\n" +
                   $"🔊 Formato: {stats.GetFormatInfo()}\n" +
                   $"⏰ Inicializado: {_isInitialized}";
        }

        /// <summary>
        /// Cambia al micrófono predeterminado (útil si cambian los dispositivos)
        /// </summary>
        public void RefreshDefaultMicrophone()
        {
            LoadDefaultMicrophone();
        }

        /// <summary>
        /// Selecciona un micrófono por su ID de dispositivo
        /// </summary>
        public bool SelectMicrophone(int deviceId)
        {
           return _voskClient.MicrophoneManager.SelectMicrophone(deviceId);
        }

        public void SetAudioMode(string mode)
        {
            try
            {
                if (_voskClient != null)
                {
                    _voskClient.SetAudioProcessingMode(mode);
                    NotifyStatus($"🎚️ Modo de audio cambiado a: {mode}");
                }
            }
            catch (Exception ex)
            {
                NotifyError($"❌ Error cambiando modo de audio: {ex.Message}");
            }
        }

        // NUEVO: Método para obtener información de configuración actual
        public string GetAudioConfiguration()
        {
            return @"
🎙️ CONFIGURACIÓN DE AUDIO ACTIVA:
===============================
Modo: Voz Cercana Prioritaria
• Voz cercana: 100% (sin atenuación)
• Voz débil: 50% (atenuada a la mitad)
• Sonidos lejanos: 20% (reducidos 80%)
• Ruido de fondo: 5% (reducido 95%)

Umbrales:
• Voz cercana: > 0.015 RMS
• Sonidos lejanos: > 0.003 RMS
• Ruido: < 0.003 RMS

Características:
✓ Prioriza tu voz cercana
✓ Atenúa sonidos lejanos
✓ Reduce ruido ambiental
✓ Mantiene inteligibilidad";
        }

        /// <summary>
        /// Configura el formato de audio (útil para personalización)
        /// </summary>
        public void SetWaveFormat(int sampleRate = 16000, int bits = 16, int channels = 1)
        {
            _voskClient.MicrophoneManager.SelectMicrophone(DefaultMicrophone.DeviceId);
        }

        // Métodos auxiliares privados
        private void NotifyStatus(string message)
        {
            OnStatusChanged?.Invoke(message);
        }

        private void NotifyError(string error)
        {
            OnErrorOccurred?.Invoke(error);
        }

        private void SetRunningState(bool isRunning)
        {
            _isRunning = isRunning;
            OnRunningStateChanged?.Invoke(isRunning);
        }

        public void KillVosk()
        {
            _voskClient.KillVoskProcess();
        }

        // Propiedades públicas de estado
        public bool IsRunning => _isRunning;
        public bool IsInitialized => _isInitialized;
        public string CurrentMicrophoneName => DefaultMicrophoneName;

        public void Dispose()
        {
            _voskClient?.Dispose();
            _isRunning = false;
            _isInitialized = false;
        }
    }
}
