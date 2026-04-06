using AIRH_MAX.ClassView.IA;
using AIRH_MAX.Views;
using ImageMagick;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;

namespace AIRH_MAX.ClassView.ViewModel
{

    internal class Multimedia
    {

        public static void ConvertToJpg(string entrada, string salida)
        {
            try
            {
                MagickImageCollection collection = new MagickImageCollection(entrada);
                collection.Write(salida);
            }
            catch (Exception a)
            {
                MainWindow.NotificacionEvent.Log = "Archivo no valido";
            }
        }

        static string pathRecIn = string.Empty;
        static string pathRecOut = string.Empty;

        public static void GrabadoraOn()
        {
            try
            {
                string path = Path.GetTempPath() + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".wav";
                Engrane.audioRecorder.Filename = path;
                Engrane.audioRecorder.StartRecording();
                pathRecIn = path;
                pathRecOut = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".wav";
            }
            catch (Exception e)
            {
                MainWindow.NotificacionEvent.Log = e.Message;
            }
        }

        public static void GrabadoraOff()
        {
            Engrane.audioRecorder.IsRecording = false;
            Engrane.audioRecorder.StopRecording();
            File.Copy(pathRecIn, RutasAbsolutas.NotasVoz + "\\" + pathRecOut);
        }

        public static void GrabadoraOn_PC()
        {
            Engrane.capture = new WasapiLoopbackCapture();

            string path = RutasAbsolutas.NotasVoz + "\\" + "PC_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".wav";

            var writer = new WaveFileWriter(path, Engrane.capture.WaveFormat);

            Engrane.capture.DataAvailable += (s, a) =>
            {
                if (writer != null)
                {
                    writer.Write(a.Buffer, 0, a.BytesRecorded);
                }
            };

            Engrane.capture.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
            };

            Engrane.capture.StartRecording();
        }

        public static void GrabadoraOff_PC()
        {
            Engrane.capture.StopRecording();
            MainWindow.NotificacionEvent.MensajeBox = "Grabación finalizada.";
        }

        public static bool GrabadoraPC_State()
        {
            if (Engrane.capture == null)
            {
                return false;
            }
            else if (Engrane.capture.CaptureState.ToString().Equals("Stopped"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string DeviceReproduccion()
        {
            var enumerator = new MMDeviceEnumerator();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            return defaultDevice.FriendlyName;
        }

        public static async Task<string> ImageToText(string rutaImagen)
        {
            // 3. Validar extensiones permitidas
            string extension = Path.GetExtension(rutaImagen).ToLower();

            var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            if (!extensionesPermitidas.Contains(extension))
            {
                return $"Error: Formato de archivo no permitido. Extensiones válidas: {string.Join(", ", extensionesPermitidas)}";
            }

            // 4. Validar que el archivo no esté vacío
            var fileInfo = new FileInfo(rutaImagen);
            if (fileInfo.Length == 0)
            {
                return $"Error: El archivo '{Path.GetFileName(rutaImagen)}' está vacío";
            }

            // 5. Validar tamaño máximo (ej: 50MB)
            const long maxSizeMB = 50;
            if (fileInfo.Length > maxSizeMB * 1024 * 1024)
            {
                return $"Error: El archivo excede el tamaño máximo de {maxSizeMB}MB";
            }

            // 6. Si pasa todas las validaciones, procesar OCR
            try
            {
                return await OCR_Platform.ExtractTextAsync(rutaImagen, RutasAbsolutas.ITT);
            }
            catch (Exception ex)
            {
                return $"Error al procesar la imagen: {ex.Message}";
            }
        }

        private static CancellationTokenSource _cancellationTokenSource;
        private static Task _grabacionTask;
        private static Process _ffmpegProcess;
        private static bool _estaGrabando = false;

        public static async Task IniciarFilmacion(string outputPath)
        {
            if (_estaGrabando) return;

            // Asegurar que el archivo de salida tenga extensión .mp4
            if (!Path.HasExtension(outputPath))
            {
                outputPath = Path.ChangeExtension(outputPath, ".mp4");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _estaGrabando = true;

            _grabacionTask = Task.Run(async () =>
            {
                try
                {
                    var ffmpegPath = Path.Combine(Environment.CurrentDirectory, "x64", "ffmpeg.exe");

                    var processInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-f gdigrab -framerate 30 -i desktop -c:v libx264 -crf 29 -preset fast -an \"{outputPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true, // ¡IMPORTANTE! Para enviar comandos
                        WorkingDirectory = Environment.CurrentDirectory
                    };

                    Debug.WriteLine($"Ejecutando: {processInfo.FileName} {processInfo.Arguments}");

                    _ffmpegProcess = new Process { StartInfo = processInfo };

                    // Configurar eventos para capturar salida
                    _ffmpegProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Debug.WriteLine($"FFmpeg: {e.Data}");
                            if (e.Data.Contains("frame="))
                            {
                                Debug.WriteLine("✓ Grabación en progreso...");
                            }
                        }
                    };

                    _ffmpegProcess.EnableRaisingEvents = true;
                    _ffmpegProcess.Exited += (sender, e) =>
                    {
                        Debug.WriteLine($"Proceso FFmpeg terminado con código: {_ffmpegProcess.ExitCode}");
                    };

                    _ffmpegProcess.Start();
                    _ffmpegProcess.BeginErrorReadLine();

                    Debug.WriteLine("Proceso FFmpeg iniciado, grabando...");

                    // Esperar a que termine o sea cancelado
                    await _ffmpegProcess.WaitForExitAsync(_cancellationTokenSource.Token);

                    Debug.WriteLine("Grabación completada exitosamente");
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Grabación cancelada por el usuario");
                    await FinalizarGrabacionCorrectamente();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error durante la grabación: {ex.Message}");
                }
                finally
                {
                    _ffmpegProcess?.Close();
                    _ffmpegProcess?.Dispose();
                    _ffmpegProcess = null;
                    _estaGrabando = false;
                    Debug.WriteLine("Estado de grabación: Finalizado");
                    MainWindow.NotificacionEvent.MensajeBoxMute = $"Grabación guardada:\n{outputPath}";
                }
            });

            await Task.Delay(1000);
        }

        // Método para finalizar FFmpeg correctamente
        private static async Task FinalizarGrabacionCorrectamente()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                try
                {
                    Debug.WriteLine("Enviando señal 'q' para finalizar FFmpeg correctamente...");

                    // Enviar 'q' para que FFmpeg finalice correctamente
                    await _ffmpegProcess.StandardInput.WriteLineAsync("q");
                    await _ffmpegProcess.StandardInput.FlushAsync();

                    // Esperar a que procese (máximo 3 segundos)
                    var timeoutTask = Task.Delay(3000);
                    var exitTask = _ffmpegProcess.WaitForExitAsync();

                    var completedTask = await Task.WhenAny(exitTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Debug.WriteLine("Timeout, forzando cierre de FFmpeg...");
                        _ffmpegProcess.Kill();
                        await _ffmpegProcess.WaitForExitAsync();
                    }
                    else
                    {
                        Debug.WriteLine("FFmpeg finalizado correctamente");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al finalizar FFmpeg: {ex.Message}");
                    // Forzar cierre si falla el método correcto
                    _ffmpegProcess?.Kill();
                    await _ffmpegProcess?.WaitForExitAsync();
                }
            }
        }

        public static async Task DetenerFilmar()
        {
            if (!_estaGrabando || _cancellationTokenSource == null) return;

            try
            {
                Debug.WriteLine("Deteniendo grabación...");
                _cancellationTokenSource.Cancel();

                // Esperar a que la tarea termine (máximo 5 segundos)
                if (_grabacionTask != null && !_grabacionTask.IsCompleted)
                {
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny(_grabacionTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Debug.WriteLine("Timeout al detener la grabación, forzando cierre...");
                        _ffmpegProcess?.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al detener la grabación: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _estaGrabando = false;
                Debug.WriteLine("Grabación detenida");
            }
        }
        public static void MatarProcesoForzoso()
        {
            try
            {
                Debug.WriteLine("Cerrando aplicación - Matando proceso FFmpeg...");

                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill(true);
                    Debug.WriteLine("Proceso FFmpeg terminado forzosamente");
                }

                // También cancelar cualquier tarea en curso
                _cancellationTokenSource?.Cancel();

                // Limpiar recursos
                _ffmpegProcess?.Close();
                _ffmpegProcess?.Dispose();
                _ffmpegProcess = null;
                _estaGrabando = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al matar proceso: {ex.Message}");
            }
        }

        public static bool FilmarPC_State()
        {
            return _estaGrabando;
        }
    }
}
