using LLM_ServerMain.OCR;
using System.IO;

namespace AIRH_MAX.ClassView.IA
{
    internal class OCR_Platform : IDisposable
    {
        //private LlamaVisionOCR? _ocr;
        //private bool _isProcessing = false;
        //private readonly object _lock = new object();

        /// <summary>
        /// Procesa una imagen (inicia servidor, procesa, detiene servidor)
        /// </summary>
        public static async Task<string> ExtractTextAsync(string imagePath, string output)
        {
            // Validar que la imagen existe
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"No se encuentra la imagen: {imagePath}");
            }

            // Crear nueva instancia para este proceso
            using var ocr = CreateOCRInstance(output);

            try
            {
                // Validar configuración
                if (!ocr.ValidateFiles(out var error))
                {
                    throw new Exception($"Error de configuración: {error}");
                }

                // Iniciar servidor (solo para este proceso)
                await ocr.StartServerAsync();

                // Procesar imagen
                var result = await ocr.ProcessImageAsync(imagePath);

                if (!result.Success)
                {
                    throw new Exception($"Error en OCR: {result.ErrorMessage}");
                }

                return result.ExtractedText;
            }
            finally
            {
                // Detener servidor y liberar recursos
                await ocr.StopServerAsync();
                // ocr se destruye automáticamente por el using
            }
        }

        /// <summary>
        /// Procesa múltiples imágenes en lote (inicia servidor una sola vez)
        /// </summary>
        public static async Task<List<OcrResult>> ExtractTextBatchAsync(IEnumerable<string> imagePaths, string output)
        {
            var imagesList = new List<string>();
            foreach (var path in imagePaths)
            {
                if (File.Exists(path))
                    imagesList.Add(path);
            }

            if (imagesList.Count == 0)
            {
                throw new Exception("No se encontraron imágenes válidas para procesar");
            }

            using var ocr = CreateOCRInstance(output);

            try
            {
                if (!ocr.ValidateFiles(out var error))
                {
                    throw new Exception($"Error de configuración: {error}");
                }

                await ocr.StartServerAsync();

                return await ocr.ProcessImagesAsync(imagesList);
            }
            finally
            {
                await ocr.StopServerAsync();
            }
        }

        private static LlamaVisionOCR CreateOCRInstance(string output)
        {
            return new LlamaVisionOCR
            {
                LlamaServerPath = Environment.CurrentDirectory + @"\llm\LLMTools\llama-server.exe",
                ModelPath = Environment.CurrentDirectory + @"\llm\ocr\qwen3.5vl-0.8B-ImageExplainer-GGUF-Q8_0.gguf",
                MmprojPath = Environment.CurrentDirectory + @"\llm\ocr\mmproj-qwen3.5vl-0.8B-ImageExplainer-GGUF-Q8_0.gguf",
                OutputFolder = output,
                Verbose = false
            };
        }

        public void Dispose()
        {
            // No hay recursos persistentes que liberar porque todo es bajo demanda
        }
    }
}