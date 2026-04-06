using NAudio.Wave;
using PiperSharp;
using PiperSharp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeSharp
{
    public class PiperService : IDisposable
    {
        private bool _isInitialized = false;
        private VoiceModel? _model;
        private bool _disposed = false;
        private readonly object _lockObject = new object();

        public event Action<string>? StatusChanged;
        public event Action<string>? ErrorOccurred;
        public event Action<string>? SpeechStarted;
        public event Action<string>? SpeechCompleted;

        public static ValidationResult ValidateModelFiles(string modelKey)
        {
            var modelDirectory = Path.Combine(PiperDownloader.DefaultModelLocation, modelKey);

            if (!Directory.Exists(modelDirectory))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Directorio del modelo no existe",
                    MissingFiles = new[] { "model.json", "*.onnx.json", "*.onnx" }
                };
            }

            // Buscar archivos .onnx para determinar el nombre del modelo
            var onnxFiles = Directory.GetFiles(modelDirectory, "*.onnx");
            if (!onnxFiles.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "No se encontró archivo .onnx",
                    MissingFiles = new[] { "*.onnx" }
                };
            }

            var onnxFile = onnxFiles[0];
            var modelName = Path.GetFileNameWithoutExtension(onnxFile);

            // Verificar los tres archivos requeridos
            var missingFiles = new List<string>();

            if (!File.Exists(Path.Combine(modelDirectory, "model.json")))
                missingFiles.Add("model.json");

            if (!File.Exists(Path.Combine(modelDirectory, $"{modelName}.onnx.json")))
                missingFiles.Add($"{modelName}.onnx.json");

            if (!File.Exists(Path.Combine(modelDirectory, $"{modelName}.onnx")))
                missingFiles.Add($"{modelName}.onnx");

            if (missingFiles.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Faltan {missingFiles.Count} archivo(s)",
                    MissingFiles = missingFiles.ToArray(),
                    ModelName = modelName
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Todos los archivos presentes",
                ModelName = modelName,
                ModelDirectory = modelDirectory
            };
        }
        private async Task InitializeAsync(string modelKey = "es_MX-claude-high")
        {
            if (_disposed) throw new ObjectDisposedException("PiperService");

            try
            {
                StatusChanged?.Invoke("Inicializando Piper...");

                if (!File.Exists(PiperDownloader.DefaultPiperExecutableLocation))
                {
                    StatusChanged?.Invoke("Descargando Piper...");
                    await PiperDownloader.DownloadPiper().ExtractPiper(PiperDownloader.DefaultLocation);
                }

                StatusChanged?.Invoke("Verificando módulo de voz...");

                var resultModel = ValidateModelFiles(modelKey);               
                if (resultModel.IsValid)
                {
                    StatusChanged?.Invoke("Cargando módulo de voz..."); 
                    _model = await VoiceModel.LoadModelByKey(modelKey);
                }
                else
                {
                    StatusChanged?.Invoke($"Descargando modelo completo.\nRazón: {resultModel.Message}");

                    // Limpiar directorio incompleto si existe
                    var modelDir = Path.Combine(PiperDownloader.DefaultModelLocation, modelKey);
                    if (Directory.Exists(modelDir))
                    {
                        Directory.Delete(modelDir, true);
                    }

                    _model = await PiperDownloader.DownloadModelByKey(modelKey);
                    _model = await VoiceModel.LoadModelByKey(modelKey);
                }

                if (_model == null)
                {
                    ErrorOccurred?.Invoke("No se pudo cargar el modelo de voz.");
                    return;
                }

                _isInitialized = true;
                StatusChanged?.Invoke("Listo para usar");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Error de inicialización: {ex.Message}");
                throw;
            }
        }

        // ✅ MÉTODO DE REINICIALIZACIÓN
        public async Task<string> ReinitializeAsync(string modelKey = "es_MX-claude-high")
        {
            DisposeResources();
            _isInitialized = false;
            await InitializeAsync(modelKey);
            return modelKey;
        }

        public void Speak(string text)
        {
            if (_disposed) throw new ObjectDisposedException("PiperService");
            if (!_isInitialized || _model == null)
            {
                ErrorOccurred?.Invoke("Servicio no inicializado");
                return;
            }

            lock (_lockObject)
            {
                if (_disposed) return;

                try
                {
                    SpeechStarted?.Invoke(text);
                    PlayAudioAndWait(text);
                    SpeechCompleted?.Invoke(text);
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke($"Error: {ex.Message}");
                }
            }
        }

        private void PlayAudioAndWait(string text)
        {
            Process? piperProcess = null;
            StreamWriter? standardInput = null;
            MemoryStream? audioData = null;

            try
            {
                // 1. CREAR PROCESO PIPER
                piperProcess = CreatePiperProcess();
                piperProcess.Start();
                standardInput = piperProcess.StandardInput;

                // 2. ENVIAR TEXTO A PIPER
                standardInput.WriteLine(text.ToUtf8());
                standardInput.Flush();
                standardInput.Close();

                // 3. LEER AUDIO GENERADO
                audioData = new MemoryStream();
                piperProcess.StandardOutput.BaseStream.CopyTo(audioData);
                audioData.Position = 0;

                // 4. ESPERAR A QUE PIPER TERMINE
                piperProcess.WaitForExit(30000);

                if (audioData.Length == 0)
                {
                    throw new Exception("No se generó audio");
                }

                // 5. REPRODUCIR AUDIO
                PlayAudioFromMemory(audioData, text);
            }
            finally
            {
                // Limpieza
                try { standardInput?.Dispose(); } catch { }
                try { audioData?.Dispose(); } catch { }
                try
                {
                    if (piperProcess != null && !piperProcess.HasExited)
                        piperProcess.Kill();
                    piperProcess?.Dispose();
                }
                catch { }
            }
        }

        private void PlayAudioFromMemory(MemoryStream audioData, string text)
        {
            WaveOutEvent? waveOut = null;
            RawSourceWaveStream? waveStream = null;

            try
            {
                // CREAR STREAM DE AUDIO
                var waveFormat = new WaveFormat((int)(_model!.Audio?.SampleRate ?? 16000), 1);
                waveStream = new RawSourceWaveStream(audioData, waveFormat);

                // CREAR REPRODUCTOR
                waveOut = new WaveOutEvent();
                waveOut.Init(waveStream);

                // REPRODUCIR Y ESPERAR
                waveOut.Play();

                // Calcular tiempo estimado de reproducción
                int estimatedPlayTime = CalculateAudioPlayTime(waveStream, text);

                // Esperar que termine la reproducción
                WaitForPlaybackCompletion(waveOut, estimatedPlayTime);
            }
            finally
            {
                try { waveOut?.Stop(); } catch { }
                try { waveOut?.Dispose(); } catch { }
                try { waveStream?.Dispose(); } catch { }
            }
        }

        private void WaitForPlaybackCompletion(WaveOutEvent waveOut, int timeoutMs)
        {
            var startTime = DateTime.Now;

            // ESPERA ACTIVA - Polling del estado
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                {
                    waveOut.Stop();
                    break;
                }
                Thread.Sleep(50);
            }
        }

        private int CalculateAudioPlayTime(RawSourceWaveStream waveStream, string text)
        {
            if (waveStream.Length > 0)
            {
                double bytesPerSecond = waveStream.WaveFormat.SampleRate *
                                      waveStream.WaveFormat.Channels *
                                      (waveStream.WaveFormat.BitsPerSample / 8);

                double durationSeconds = waveStream.Length / bytesPerSecond;
                return (int)(durationSeconds * 1000) + 2000;
            }
            else
            {
                return CalculateWaitTime(text);
            }
        }

        private int CalculateWaitTime(string text)
        {
            if (string.IsNullOrEmpty(text)) return 5000;
            return 3000 + (text.Length * 100);
        }

        private Process CreatePiperProcess()
        {
            var config = new PiperConfiguration()
            {
                Model = _model!,
                UseCuda = false,
                ExecutableLocation = PiperDownloader.DefaultPiperExecutableLocation,
                WorkingDirectory = PiperDownloader.DefaultPiperLocation
            };

            return new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = config.ExecutableLocation,
                    Arguments = config.BuildArguments(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = config.WorkingDirectory,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                }
            };
        }

        private void DisposeResources()
        {
            // Limpiar recursos si es necesario
            // En esta implementación, los recursos se crean y destruyen por cada reproducción
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                DisposeResources();
            }
        }
    }
}