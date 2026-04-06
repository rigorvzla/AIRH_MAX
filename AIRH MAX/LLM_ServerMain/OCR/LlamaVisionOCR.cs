using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLM_ServerMain.OCR
{
    /// <summary>
    /// Clase principal para el servicio OCR con Llama Vision
    /// </summary>
    public class LlamaVisionOCR : IDisposable
    {
        #region Constantes por defecto

        public const int DEFAULT_PORT = 8080;
        public const int DEFAULT_THREADS = 4;
        public const int DEFAULT_CONTEXT_SIZE = 2048;
        public const int DEFAULT_BATCH_SIZE = 256;
        public const int DEFAULT_UBATCH_SIZE = 128;
        public const string DEFAULT_CACHE_TYPE = "q8_0";
        public const string DEFAULT_FLASH_ATTN = "auto";
        public const bool DEFAULT_MLOCK = true;
        public const int DEFAULT_SERVER_STARTUP_DELAY = 3000;
        public const string DEFAULT_PROMPT = "Extrae todo el texto de esta imagen. Devuelve SOLO el texto extraído, sin explicaciones adicionales.";

        #endregion

        #region Propiedades de configuración

        /// <summary>Ruta a llama-server.exe</summary>
        public string LlamaServerPath { get; set; } = string.Empty;

        /// <summary>Ruta al archivo .gguf del modelo</summary>
        public string ModelPath { get; set; } = string.Empty;

        /// <summary>Ruta al archivo mmproj.gguf</summary>
        public string MmprojPath { get; set; } = string.Empty;

        /// <summary>Prompt para el OCR</summary>
        public string Prompt { get; set; } = DEFAULT_PROMPT;

        /// <summary>Puerto del servidor</summary>
        public int Port { get; set; } = DEFAULT_PORT;

        /// <summary>Número de hilos de CPU</summary>
        public int Threads { get; set; } = DEFAULT_THREADS;

        /// <summary>Tamaño de contexto</summary>
        public int ContextSize { get; set; } = DEFAULT_CONTEXT_SIZE;

        /// <summary>Batch size</summary>
        public int BatchSize { get; set; } = DEFAULT_BATCH_SIZE;

        /// <summary>Ubatch size</summary>
        public int UbatchSize { get; set; } = DEFAULT_UBATCH_SIZE;

        /// <summary>Tipo de caché K/V (q8_0, q4_0, f16)</summary>
        public string CacheType { get; set; } = DEFAULT_CACHE_TYPE;

        /// <summary>Flash Attention (on, off, auto)</summary>
        public string FlashAttn { get; set; } = DEFAULT_FLASH_ATTN;

        /// <summary>Habilitar mlock</summary>
        public bool Mlock { get; set; } = DEFAULT_MLOCK;

        /// <summary>Delay de inicio del servidor en ms</summary>
        public int ServerStartupDelay { get; set; } = DEFAULT_SERVER_STARTUP_DELAY;

        /// <summary>Carpeta para guardar los archivos TXT con GUID</summary>
        public string OutputFolder { get; set; } = string.Empty;

        /// <summary>Guardar copia junto a la imagen original</summary>
        public bool SaveAlongsideImage { get; set; } = false;

        /// <summary>Habilitar logging detallado</summary>
        public bool Verbose { get; set; } = false;

        #endregion

        private Process? _serverProcess;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public LlamaVisionOCR()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        #region Métodos públicos principales

        /// <summary>
        /// Valida que todos los archivos necesarios existan
        /// </summary>
        public bool ValidateFiles(out string? errorMessage)
        {
            if (!File.Exists(LlamaServerPath))
            {
                errorMessage = $"No se encuentra llama-server.exe: {LlamaServerPath}";
                return false;
            }

            if (!File.Exists(ModelPath))
            {
                errorMessage = $"No se encuentra el modelo: {ModelPath}";
                return false;
            }

            if (!File.Exists(MmprojPath))
            {
                errorMessage = $"No se encuentra mmproj: {MmprojPath}";
                return false;
            }

            if (!string.IsNullOrEmpty(OutputFolder) && !Directory.Exists(OutputFolder))
            {
                try
                {
                    Directory.CreateDirectory(OutputFolder);
                }
                catch (Exception ex)
                {
                    errorMessage = $"No se puede crear la carpeta de salida: {ex.Message}";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Inicia el servidor llama-server
        /// </summary>
        public async Task<bool> StartServerAsync(CancellationToken cancellationToken = default)
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                return true; // Ya está corriendo
            }

            var argsList = new List<string>
            {
                $"-m \"{ModelPath}\"",
                $"--mmproj \"{MmprojPath}\"",
                "--host localhost",
                $"--port {Port}",
                $"-t {Threads}",
                "-ngl 0",
                $"--ctx-size {ContextSize}",
                $"--cache-type-k {CacheType}",
                $"--cache-type-v {CacheType}",
                $"--flash-attn {FlashAttn}",
                $"--batch-size {BatchSize}",
                $"--ubatch-size {UbatchSize}"
            };

            if (Mlock)
            {
                argsList.Add("--mlock");
            }

            string arguments = string.Join(" ", argsList);

            var processInfo = new ProcessStartInfo
            {
                FileName = LlamaServerPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _serverProcess = new Process { StartInfo = processInfo };
            _serverProcess.Start();

            if (Verbose)
            {
                _serverProcess.OutputDataReceived += (s, e) => Console.WriteLine($"[SERVER] {e.Data}");
                _serverProcess.ErrorDataReceived += (s, e) => Console.WriteLine($"[SERVER ERR] {e.Data}");
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();
            }

            // Esperar a que el servidor esté listo
            await WaitForServerReadyAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Detiene el servidor llama-server
        /// </summary>
        public async Task StopServerAsync()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
                await _serverProcess.WaitForExitAsync();
                _serverProcess.Dispose();
                _serverProcess = null;
            }
        }

        /// <summary>
        /// Procesa una sola imagen y devuelve el texto extraído
        /// </summary>
        public async Task<OcrResult> ProcessImageAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var result = new OcrResult { ImagePath = imagePath, Success = false };

            try
            {
                if (!File.Exists(imagePath))
                {
                    result.ErrorMessage = $"No se encuentra la imagen: {imagePath}";
                    return result;
                }

                result.Guid = Guid.NewGuid();
                result.FileSize = new FileInfo(imagePath).Length;

                // Codificar imagen
                string base64Image = ConvertImageToBase64(imagePath);

                // Enviar solicitud OCR
                string ocrText = await SendOcrRequestAsync(base64Image, cancellationToken);

                // Limpiar resultado
                result.ExtractedText = CleanResponse(ocrText);
                result.Success = true;
                result.ProcessedAt = DateTime.Now;

                // Guardar archivo en la carpeta de salida
                if (!string.IsNullOrEmpty(OutputFolder))
                {
                    result.OutputFile = Path.Combine(OutputFolder, $"{result.Guid}.txt");
                    await File.WriteAllTextAsync(result.OutputFile, result.ExtractedText, Encoding.UTF8, cancellationToken);
                }

                // Guardar junto a la imagen original
                if (SaveAlongsideImage)
                {
                    result.SidecarFile = Path.Combine(
                        Path.GetDirectoryName(imagePath) ?? ".",
                        $"{Path.GetFileNameWithoutExtension(imagePath)}_ocr_{result.Guid}.txt");
                    await File.WriteAllTextAsync(result.SidecarFile, result.ExtractedText, Encoding.UTF8, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Procesa múltiples imágenes y devuelve los resultados
        /// </summary>
        public async Task<List<OcrResult>> ProcessImagesAsync(IEnumerable<string> imagePaths, CancellationToken cancellationToken = default)
        {
            var results = new List<OcrResult>();
            var distinctImages = imagePaths.Distinct().Where(File.Exists).ToList();

            foreach (var imagePath in distinctImages)
            {
                var result = await ProcessImageAsync(imagePath, cancellationToken);
                results.Add(result);

                if (Verbose)
                {
                    Console.WriteLine($"Procesada: {Path.GetFileName(imagePath)} - {(result.Success ? "OK" : "ERROR")}");
                }
            }

            return results;
        }

        /// <summary>
        /// Procesa todas las imágenes de una carpeta
        /// </summary>
        public async Task<List<OcrResult>> ProcessFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"No se encuentra la carpeta: {folderPath}");
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
            var images = new List<string>();

            foreach (var ext in extensions)
            {
                images.AddRange(Directory.GetFiles(folderPath, $"*{ext}", SearchOption.TopDirectoryOnly));
                images.AddRange(Directory.GetFiles(folderPath, $"*{ext.ToUpper()}", SearchOption.TopDirectoryOnly));
            }

            return await ProcessImagesAsync(images, cancellationToken);
        }

        /// <summary>
        /// Procesa una imagen y devuelve SOLO el texto (método simplificado)
        /// </summary>
        public async Task<string> ExtractTextAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var result = await ProcessImageAsync(imagePath, cancellationToken);
            return result.Success ? result.ExtractedText : string.Empty;
        }

        /// <summary>
        /// Genera un archivo de resumen con todos los resultados
        /// </summary>
        public async Task<string> GenerateSummaryAsync(IEnumerable<OcrResult> results, string summaryFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== RESUMEN OCR - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            sb.AppendLine($"Modelo: {Path.GetFileName(ModelPath)}");
            sb.AppendLine($"Total imágenes: {results.Count()}");
            sb.AppendLine($"Procesadas correctamente: {results.Count(r => r.Success)}");
            sb.AppendLine($"Con errores: {results.Count(r => !r.Success)}");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();

            foreach (var result in results)
            {
                sb.AppendLine($"--- Imagen: {Path.GetFileName(result.ImagePath)} ---");
                sb.AppendLine($"  GUID: {result.Guid}");
                sb.AppendLine($"  Éxito: {(result.Success ? "Sí" : "No")}");
                sb.AppendLine($"  Tamaño: {result.FileSize} bytes");
                sb.AppendLine($"  Procesado: {result.ProcessedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"  Archivo resultado: {result.OutputFile ?? "No guardado"}");

                if (!result.Success)
                {
                    sb.AppendLine($"  Error: {result.ErrorMessage}");
                }
                else
                {
                    sb.AppendLine($"  Texto extraído (primeros 200 chars):");
                    sb.AppendLine($"  {result.ExtractedText.Substring(0, Math.Min(200, result.ExtractedText.Length))}...");
                }
                sb.AppendLine();
            }

            sb.AppendLine(new string('=', 60));
            sb.AppendLine($"=== FIN DEL PROCESAMIENTO - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

            await File.WriteAllTextAsync(summaryFilePath, sb.ToString(), Encoding.UTF8);
            return summaryFilePath;
        }

        #endregion

        #region Métodos privados

        private async Task WaitForServerReadyAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(ServerStartupDelay, cancellationToken);

            int maxRetries = 10;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"http://localhost:{Port}/health", cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
                catch { }
                await Task.Delay(1000, cancellationToken);
            }
            throw new TimeoutException("No se pudo conectar con llama-server");
        }

        private async Task<string> SendOcrRequestAsync(string base64Image, CancellationToken cancellationToken)
        {
            var request = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image_url", image_url = new { url = base64Image } },
                            new { type = "text", text = Prompt }
                        }
                    }
                },
                max_tokens = 1024,
                temperature = 0.1
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:{Port}/v1/chat/completions", content, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error del servidor: {response.StatusCode} - {responseJson}");
            }

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        private string ConvertImageToBase64(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string extension = Path.GetExtension(imagePath).ToLower();
            string mimeType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "image/jpeg"
            };
            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }

        private string CleanResponse(string response)
        {
            string cleaned = Regex.Replace(response, @"<think>.*?</think>", "", RegexOptions.Singleline);
            cleaned = cleaned.TrimStart('\n', '\r');
            if (cleaned.StartsWith("instant"))
                cleaned = cleaned.Substring(7).TrimStart();
            return cleaned.Trim();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    _serverProcess.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Resultado del procesamiento OCR
    /// </summary>
    public class OcrResult
    {
        public string ImagePath { get; set; } = string.Empty;
        public Guid Guid { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OutputFile { get; set; }
        public string? SidecarFile { get; set; }
        public long FileSize { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}