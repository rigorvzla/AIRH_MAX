namespace LLM_ServerMain.OCR
{
    /// <summary>
    /// Métodos de extensión y conveniencia para usar la librería
    /// </summary>
    public static class LlamaVisionOCRExtensions
    {
        /// <summary>
        /// Configura la librería usando un objeto de opciones (similar a los argumentos del CLI)
        /// </summary>
        public static LlamaVisionOCR ConfigureFromOptions(this LlamaVisionOCR ocr, VisionOCROptions options)
        {
            ocr.LlamaServerPath = options.LlamaServerPath;
            ocr.ModelPath = options.ModelPath;
            ocr.MmprojPath = options.MmprojPath;
            ocr.Prompt = options.Prompt ?? LlamaVisionOCR.DEFAULT_PROMPT;
            ocr.Port = options.Port;
            ocr.Threads = options.Threads;
            ocr.ContextSize = options.ContextSize;
            ocr.BatchSize = options.BatchSize;
            ocr.UbatchSize = options.UbatchSize;
            ocr.CacheType = options.CacheType ?? LlamaVisionOCR.DEFAULT_CACHE_TYPE;
            ocr.FlashAttn = options.FlashAttn ?? LlamaVisionOCR.DEFAULT_FLASH_ATTN;
            ocr.Mlock = options.Mlock;
            ocr.ServerStartupDelay = options.ServerStartupDelay;
            ocr.OutputFolder = options.OutputFolder ?? string.Empty;
            ocr.SaveAlongsideImage = options.SaveAlongsideImage;
            ocr.Verbose = options.Verbose;
            return ocr;
        }

        /// <summary>
        /// Procesa una imagen y devuelve el resultado como JSON
        /// </summary>
        public static async Task<string> ProcessImageToJsonAsync(this LlamaVisionOCR ocr, string imagePath, CancellationToken cancellationToken = default)
        {
            var result = await ocr.ProcessImageAsync(imagePath, cancellationToken);
            return System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Opciones de configuración (similar a los argumentos del CLI)
    /// </summary>
    public class VisionOCROptions
    {
        public string LlamaServerPath { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public string MmprojPath { get; set; } = string.Empty;
        public string? Prompt { get; set; }
        public int Port { get; set; } = LlamaVisionOCR.DEFAULT_PORT;
        public int Threads { get; set; } = LlamaVisionOCR.DEFAULT_THREADS;
        public int ContextSize { get; set; } = LlamaVisionOCR.DEFAULT_CONTEXT_SIZE;
        public int BatchSize { get; set; } = LlamaVisionOCR.DEFAULT_BATCH_SIZE;
        public int UbatchSize { get; set; } = LlamaVisionOCR.DEFAULT_UBATCH_SIZE;
        public string? CacheType { get; set; }
        public string? FlashAttn { get; set; }
        public bool Mlock { get; set; } = LlamaVisionOCR.DEFAULT_MLOCK;
        public int ServerStartupDelay { get; set; } = LlamaVisionOCR.DEFAULT_SERVER_STARTUP_DELAY;
        public string? OutputFolder { get; set; }
        public bool SaveAlongsideImage { get; set; }
        public bool Verbose { get; set; }
    }
}