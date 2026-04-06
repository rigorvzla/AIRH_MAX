// AIRH_MAX/ClassView/Services/ConvertMedia.cs
using AIRH_MAX.Views;
using FFMpegCore;
using ImageMagick;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace AIRH_MAX.ClassView.Services
{
    internal class ConvertMedia
    {
        // ======================
        // Conversión a MP3
        // ======================
        public async Task ConvertToMP3(string sourceName, IProgress<string> itemProgress, IProgress<double> progress)
        {
            if (string.IsNullOrEmpty(sourceName) ||
                (!ClassView.List.FormatList.videoExtensions.Contains(Path.GetExtension(sourceName).ToLowerInvariant()) &&
                 !ClassView.List.FormatList.audioExtensions.Contains(Path.GetExtension(sourceName).ToLowerInvariant())))
            {
                MainWindow.NotificacionEvent.MensajeBox = "Archivo no valido";
                return;
            }

            itemProgress?.Report($"Convirtiendo\n{sourceName}");

            // Configurar ruta de FFmpeg
            GlobalFFOptions.Configure(options => options.BinaryFolder = Path.Combine(
                Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "x64"));

            try
            {
                var mediaInfo = await FFProbe.AnalyseAsync(sourceName);
                var duration = mediaInfo.Duration;

                string outputPath = Path.ChangeExtension(sourceName, ".mp3");

                await FFMpegArguments
                    .FromFileInput(sourceName)
                    .OutputToFile(outputPath, true, options => options
                        .WithAudioCodec("libmp3lame")
                        .WithConstantRateFactor(2)
                        .WithFastStart())
                    .NotifyOnProgress(p => progress?.Report(p), duration)
                    .ProcessAsynchronously();

                itemProgress?.Report($"Guardado\n{outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al convertir a MP3:\n{ex.Message}", "MAX", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Opcional: re-lanzar si el llamador debe manejarlo
            }
        }

        // ======================
        // Conversión a MP4
        // ======================
        public async Task ConvertToMP4(string sourceName, IProgress<string> itemProgress, IProgress<double> progress)
        {
            if (string.IsNullOrEmpty(sourceName) ||
                !ClassView.List.FormatList.videoExtensions.Contains(Path.GetExtension(sourceName).ToLowerInvariant()))
            {
                MainWindow.NotificacionEvent.MensajeBox = "Archivo no valido";
                return;
            }

            itemProgress?.Report($"Convirtiendo\n{sourceName}");

            GlobalFFOptions.Configure(options => options.BinaryFolder = Path.Combine(
                Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "x64"));

            try
            {
                var mediaInfo = await FFProbe.AnalyseAsync(sourceName);
                var duration = mediaInfo.Duration;

                string outputPath = Path.ChangeExtension(sourceName, ".mp4");

                await FFMpegArguments
                    .FromFileInput(sourceName)
                    .OutputToFile(outputPath, true, options => options
                        .WithVideoCodec("libx264")
                        .WithAudioCodec("aac")
                        .WithFastStart())
                    .NotifyOnProgress(p => progress?.Report(p), duration)
                    .ProcessAsynchronously();

                itemProgress?.Report($"Guardado\n{outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al convertir a MP4:\n{ex.Message}", "MAX", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        // ======================
        // Conversión a JPG (desde PDF, TIFF, etc.)
        // ======================
        public async Task ConvertToJPG(string sourceName, IProgress<string> itemProgress = null)
        {
            if (string.IsNullOrEmpty(sourceName) ||
                !ClassView.List.FormatList.imageExtensions.Contains(Path.GetExtension(sourceName).ToLowerInvariant()))
            {
                MainWindow.NotificacionEvent.MensajeBox = "Archivo no valido";
                return;
            }

            itemProgress?.Report($"Convirtiendo imagen\n{sourceName}");

            try
            {
                string outputPath = Path.ChangeExtension(sourceName, ".jpg");

                using (var collection = new MagickImageCollection())
                {
                    await collection.ReadAsync(sourceName);
                    // Opcional: optimizar calidad
                    foreach (var image in collection)
                    {
                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 90;
                    }
                    await collection.WriteAsync(outputPath);
                }

                itemProgress?.Report($"Guardado\n{outputPath}");
                MainWindow.NotificacionEvent.MensajeBox = "Conversión finalizada";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al convertir a JPG:\n{ex.Message}", "MAX", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}