// Services/CompressionService.cs
using SharpSevenZip;
using System.IO;

namespace AIRH_MAX.ClassView.Services
{
    public interface ICompressionProgress
    {
        void UpdateMessage(string message);
        void UpdateFileProgress(double percentage);
        void UpdateTotalProgress(double percentage);
    }


    public class CompressionService
    {
        private readonly ICompressionProgress _progress;
        private readonly string _executablePath;

        public CompressionService(ICompressionProgress progress)
        {
            _progress = progress;
            _executablePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        }

        public async Task CompressDirectoryAsync(string sourcePath)
        {
            try
            {
                ValidateDirectory(sourcePath);
                ConfigureLibrary();

                var compressor = CreateCompressor();
                SetupCompressorEvents(compressor);

                await compressor.CompressDirectoryAsync(sourcePath, sourcePath + ".7z");
                _progress.UpdateMessage("Compresion Finalizada");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public async Task CompressFileAsync(string sourcePath)
        {
            try
            {
                ValidateFile(sourcePath);
                ConfigureLibrary();

                var compressor = CreateCompressor();
                SetupCompressorEvents(compressor);

                var outputPath = Path.Combine(Path.GetDirectoryName(sourcePath),
                                            Path.GetFileNameWithoutExtension(sourcePath) + ".7z");
                await compressor.CompressFilesAsync(outputPath, sourcePath);
                _progress.UpdateMessage("Compresion Finalizada");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public async Task ExtractArchiveAsync(string sourcePath, string password = "")
        {
            try
            {
                ValidateArchive(sourcePath);
                ConfigureLibrary();

                var extractor = new SharpSevenZipExtractor(sourcePath, password: password);
                extractor.EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous;

                extractor.FileExtractionStarted += (s, e) =>
                {
                    _progress.UpdateMessage($"Extrayendo {e.FileInfo.FileName}");
                    _progress.UpdateFileProgress(e.PercentDone);
                };

                extractor.Extracting += (s, e) =>
                {
                    _progress.UpdateTotalProgress(e.PercentDone);
                };

                extractor.ExtractionFinished += (s, e) =>
                {
                    _progress.UpdateMessage("Extraccion Terminada");
                };

                var directoryName = Path.GetFileNameWithoutExtension(sourcePath);
                var directoryPath = Path.GetDirectoryName(sourcePath);
                Directory.CreateDirectory(Path.Combine(directoryPath, directoryName));

                await extractor.ExtractArchiveAsync(Path.Combine(directoryPath, directoryName));
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private SharpSevenZipCompressor CreateCompressor()
        {
            var compressor = new SharpSevenZipCompressor();
            compressor.CompressionMethod = CompressionMethod.Lzma;
            compressor.CompressionLevel = CompressionLevel.Ultra;
            compressor.CompressionMode = CompressionMode.Create;
            compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
            compressor.EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous;
            compressor.FastCompression = false;
            compressor.EncryptHeaders = true;
            compressor.ScanOnlyWritable = true;
            return compressor;
        }

        private void SetupCompressorEvents(SharpSevenZipCompressor compressor)
        {
            compressor.FileCompressionStarted += (s, e) =>
            {
                _progress.UpdateMessage($"Comprimiendo {e.FileName}");
                _progress.UpdateFileProgress(e.PercentDone);
            };

            compressor.Compressing += (s, e) =>
            {
                _progress.UpdateTotalProgress(e.PercentDone);
            };

            compressor.CompressionFinished += (s, e) =>
            {
                _progress.UpdateMessage("Compresion Finalizada");
            };
        }

        private void ConfigureLibrary()
        {
            SharpSevenZipBase.SetLibraryPath(Path.Combine(_executablePath, "x64", "7z.dll"));
        }

        private void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path) || string.IsNullOrEmpty(path))
                throw new ArgumentException("Directorio no válido");
        }

        private void ValidateFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("Archivo no existe");

            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            if (ext == ".mp3")
                throw new ArgumentException("Archivo MP3 no válido para compresión");
        }

        private void ValidateArchive(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            if (ext != ".zip" && ext != ".7z" && ext != ".rar")
                throw new ArgumentException("Archivo no válido para extracción");
        }

        private void HandleError(Exception ex)
        {
            _progress.UpdateMessage($"Error: {ex.Message}\n- No tiene permisos para escribir en este directorio.");
            throw ex; // Re-lanzar para que la View maneje el cierre
        }
    }
}