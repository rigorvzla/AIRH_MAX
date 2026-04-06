using NAudio.Wave;
using System.Diagnostics;
using Vosk_STT.Models;

namespace Vosk_STT
{
    public class MicrophoneManager : IDisposable
    {
        private WaveInEvent _currentWaveIn;
        private int _selectedDeviceId = -1; // -1 = dispositivo por defecto
        private bool _isRecording = false;

        public event Action<string> OnMicrophoneChanged;
        public event Action<string> OnError;
        public event Action<WaveInEventArgs> OnAudioDataAvailable;

        public int CurrentDeviceId => _selectedDeviceId;
        public bool IsRecording => _isRecording;

        // Propiedad para acceder al WaveIn actual (útil para integración)
        public WaveInEvent CurrentWaveIn => _currentWaveIn;

        public MicrophoneManager()
        {
            Debug.WriteLine("🎤 Gestor de micrófonos inicializado");
        }

        /// <summary>
        /// Obtiene todos los micrófonos disponibles en el sistema
        /// </summary>
        public List<MicrophoneInfo> GetAvailableMicrophones()
        {
            var microphones = new List<MicrophoneInfo>();

            try
            {
                int deviceCount = WaveIn.DeviceCount;

                if (deviceCount == 0)
                {
                    OnError?.Invoke("❌ No se encontraron micrófonos en el sistema");
                    return microphones;
                }

                for (int deviceId = 0; deviceId < deviceCount; deviceId++)
                {
                    var capabilities = WaveIn.GetCapabilities(deviceId);
                    microphones.Add(new MicrophoneInfo
                    {
                        DeviceId = deviceId,
                        Name = capabilities.ProductName,
                        Channels = capabilities.Channels,
                        IsDefault = (deviceId == 0) // Generalmente el 0 es el predeterminado
                    });
                }

                return microphones;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error listando micrófonos: {ex.Message}");
                return microphones;
            }
        }

        /// <summary>
        /// Selecciona un micrófono por su ID de dispositivo
        /// </summary>
        public bool SelectMicrophone(int deviceId) 
        {
            try
            {
                // Validar que el dispositivo existe
                if (deviceId < -1 || deviceId >= WaveIn.DeviceCount)
                {
                    OnError?.Invoke($"❌ ID de micrófono inválido: {deviceId}");
                    return false;
                }

                // Detener grabación actual si está activa
                StopRecording();

                // Liberar recurso anterior
                _currentWaveIn?.Dispose();

                _selectedDeviceId = deviceId;

                // Crear nueva instancia del WaveIn
                _currentWaveIn = new WaveInEvent
                {
                    DeviceNumber = _selectedDeviceId,
                    WaveFormat = new WaveFormat(16000, 16, 1), // Formato compatible con Vosk
                    BufferMilliseconds = 50,
                    NumberOfBuffers = 3
                };

                // Configurar event handlers
                _currentWaveIn.DataAvailable += OnDataAvailable;
                _currentWaveIn.RecordingStopped += OnRecordingStopped;

                var microphoneName = GetCurrentMicrophone()?.Name ?? "Desconocido";
                OnMicrophoneChanged?.Invoke($"✅ Micrófono seleccionado: {microphoneName} (ID: {deviceId})");

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error seleccionando micrófono: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Selecciona un micrófono por nombre (búsqueda parcial)
        /// </summary>
        public bool SelectMicrophoneByName(string microphoneName)
        {
            try
            {
                var microphones = GetAvailableMicrophones();
                var microphone = microphones.FirstOrDefault(m =>
                    m.Name.Contains(microphoneName, StringComparison.OrdinalIgnoreCase));

                if (microphone != null)
                {
                    return SelectMicrophone(microphone.DeviceId);
                }

                OnError?.Invoke($"❌ No se encontró micrófono: {microphoneName}");
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error buscando micrófono: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Selecciona el micrófono predeterminado del sistema
        /// </summary>
        public bool SelectDefaultMicrophone()
        {
            return SelectMicrophone(-1); // -1 = dispositivo por defecto
        }

        /// <summary>
        /// Obtiene información del micrófono actualmente seleccionado
        /// </summary>
        public MicrophoneInfo GetCurrentMicrophone()
        {
            try
            {
                if (_selectedDeviceId == -1)
                {
                    return new MicrophoneInfo
                    {
                        DeviceId = -1,
                        Name = "Micrófono predeterminado del sistema",
                        IsDefault = true
                    };
                }

                if (_selectedDeviceId >= 0 && _selectedDeviceId < WaveIn.DeviceCount)
                {
                    var capabilities = WaveIn.GetCapabilities(_selectedDeviceId);
                    return new MicrophoneInfo
                    {
                        DeviceId = _selectedDeviceId,
                        Name = capabilities.ProductName,
                        Channels = capabilities.Channels,
                        IsDefault = (_selectedDeviceId == 0)
                    };
                }

                return new MicrophoneInfo { DeviceId = -1, Name = "Desconocido" };
            }
            catch
            {
                return new MicrophoneInfo { DeviceId = -1, Name = "Error al obtener información" };
            }
        }

        /// <summary>
        /// Inicia la grabación con el micrófono seleccionado
        /// </summary>
        public bool StartRecording()
        {
            try
            {
                if (_currentWaveIn == null)
                {
                    // Si no hay micrófono seleccionado, usar el predeterminado
                    SelectDefaultMicrophone();
                }

                if (_isRecording)
                {
                    OnError?.Invoke("⚠️ La grabación ya está en curso");
                    return false;
                }

                _currentWaveIn?.StartRecording();
                _isRecording = true;
                OnMicrophoneChanged?.Invoke("🎤 Grabación iniciada");
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error iniciando grabación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detiene la grabación
        /// </summary>
        public void StopRecording()
        {
            try
            {
                if (_isRecording && _currentWaveIn != null)
                {
                    _currentWaveIn.StopRecording();
                    _isRecording = false;
                    OnMicrophoneChanged?.Invoke("⏹️ Grabación detenida");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error deteniendo grabación: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el formato de audio actual
        /// </summary>
        public WaveFormat GetCurrentWaveFormat()
        {
            return _currentWaveIn?.WaveFormat ?? new WaveFormat(16000, 16, 1);
        }

        /// <summary>
        /// Configura el formato de audio (útil para personalización)
        /// </summary>
        public void SetWaveFormat(int sampleRate = 16000, int bits = 16, int channels = 1)
        {
            try
            {
                if (_currentWaveIn != null && !_isRecording)
                {
                    _currentWaveIn.WaveFormat = new WaveFormat(sampleRate, bits, channels);
                    OnMicrophoneChanged?.Invoke($"🔧 Formato actualizado: {sampleRate}Hz, {bits}bit, {channels} canal(es)");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error configurando formato: {ex.Message}");
            }
        }

        /// <summary>
        /// Configura el tamaño del buffer (afecta la latencia)
        /// </summary>
        public void SetBufferSize(int bufferMilliseconds = 50, int numberOfBuffers = 3)
        {
            try
            {
                if (_currentWaveIn != null && !_isRecording)
                {
                    _currentWaveIn.BufferMilliseconds = bufferMilliseconds;
                    _currentWaveIn.NumberOfBuffers = numberOfBuffers;
                    OnMicrophoneChanged?.Invoke($"🔧 Buffer actualizado: {bufferMilliseconds}ms, {numberOfBuffers} buffers");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"❌ Error configurando buffer: {ex.Message}");
            }
        }

        // Event handlers internos
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            OnAudioDataAvailable?.Invoke(e);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            _isRecording = false;

            if (e.Exception != null)
            {
                OnError?.Invoke($"❌ Error en grabación: {e.Exception.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de uso del micrófono
        /// </summary>
        public MicrophoneStats GetStats()
        {
            return new MicrophoneStats
            {
                IsRecording = _isRecording,
                DeviceId = _selectedDeviceId,
                DeviceName = GetCurrentMicrophone()?.Name ?? "Desconocido",
                WaveFormat = GetCurrentWaveFormat()
            };
        }

        /// <summary>
        /// Libera los recursos del micrófono
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopRecording();
                _currentWaveIn?.Dispose();
                _currentWaveIn = null;
                _isRecording = false;
                Debug.WriteLine("🎤 Gestor de micrófonos liberado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error liberando micrófono: {ex.Message}");
            }
        }
    }
}