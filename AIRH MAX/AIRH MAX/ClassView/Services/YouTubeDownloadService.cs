// Services/YouTubeDownloadService.cs
using FFMpegCore;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace AIRH_MAX.ClassView.Services
{
    public interface IYouTubeDownloadProgress
    {
        void UpdateDownloadProgress(double percentage);
        void UpdateConversionProgress(double percentage);
        void AddLogMessage(string message, BitmapImage thumbnail = null);
    }

    public class YouTubeDownloadService
    {
        private readonly IYouTubeDownloadProgress _progress;
        private readonly string _executablePath;

        public YouTubeDownloadService(IYouTubeDownloadProgress progress)
        {
            _progress = progress;
            _executablePath = Environment.CurrentDirectory;
        }

        public async Task DownloadAudioAsync(string idOrSearch, string outputPath, string nameIA, bool isSearch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var (video, streamInfo, thumbnail) = await GetVideoAndStreamAsync(idOrSearch, isSearch, true, cancellationToken);
                var cleanTitle = Support.ValidateIconName(video.Title);
                var tempFileName = Path.Combine(outputPath, $"{cleanTitle}.{streamInfo.Container.Name}");

                // Mostrar info del video
                var info = $"Titulo: {cleanTitle}\nAutor: {video.Author}\nDuración: {video.Duration}\nFormato: {streamInfo.Container.Name}\nPeso: {streamInfo.Size}\nBitrate: {streamInfo.Bitrate}";
                _progress.AddLogMessage(info, thumbnail);

                // Descargar
                await DownloadStreamAsync(streamInfo, tempFileName, cancellationToken);

                // Convertir a MP3
                _progress.AddLogMessage($"Convirtiendo: {streamInfo.Container.Name} a mp3", thumbnail);
                var mp3File = await ConvertToMP3Async(tempFileName);

                // Limpiar y mostrar resultado
                File.Delete(tempFileName);
                _progress.AddLogMessage(mp3File, thumbnail);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public async Task DownloadVideoAsync(string idOrSearch, string outputPath, string nameIA, bool isSearch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (isSearch)
                {
                    await DownloadVideoWithSeparateAudioAsync(idOrSearch, outputPath, nameIA, cancellationToken);
                }
                else
                {
                    await DownloadMuxedVideoAsync(idOrSearch, outputPath, nameIA, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private async Task<(Video video, IStreamInfo streamInfo, BitmapImage thumbnail)> GetVideoAndStreamAsync(
            string idOrSearch, bool isSearch, bool audioOnly, CancellationToken cancellationToken)
        {
            var client = new YoutubeClient();
            string videoId;

            if (isSearch)
            {
                _progress.AddLogMessage("Buscando y analizando el audio para descargar");
                videoId = await ClassView.YouTube.Search.SearchYouTubeVideo(idOrSearch);
            }
            else
            {
                if (!idOrSearch.Contains("youtube.com/watch?v"))
                    throw new ArgumentException("URL de YouTube no válida");
                videoId = idOrSearch;
            }

            var video = await client.Videos.GetAsync(videoId, cancellationToken);
            var manifest = await client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
            var streamInfo = audioOnly
                ? manifest.GetAudioOnlyStreams().GetWithHighestBitrate()
                : manifest.GetMuxedStreams().GetWithHighestVideoQuality();

            var thumbnailUrl = video.Thumbnails[0].Url.ToString().Split('?')[0];
            var thumbnail = new BitmapImage(new Uri(thumbnailUrl));

            return (video, streamInfo, thumbnail);
        }

        private async Task DownloadStreamAsync(IStreamInfo streamInfo, string fileName, CancellationToken cancellationToken)
        {
            var client = new YoutubeClient();
            var progress = new Progress<double>(p => _progress.UpdateDownloadProgress(p * 100));
            await client.Videos.Streams.DownloadAsync(streamInfo, fileName, progress, cancellationToken);
        }

        private async Task<string> ConvertToMP3Async(string inputPath)
        {
            ConfigureFFmpeg();
            var outputPath = Path.Combine(Path.GetDirectoryName(inputPath),
                                        Path.GetFileNameWithoutExtension(inputPath) + ".mp3");

            var progressHandler = new Action<double>(p => _progress.UpdateConversionProgress(p));
            TimeSpan duration = FFProbe.Analyse(inputPath).Duration;

            await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputPath, true, options => options
                    .WithAudioCodec("libmp3lame")
                    .WithConstantRateFactor(2)
                    .WithFastStart())
                .NotifyOnProgress(progressHandler, duration)
                .ProcessAsynchronously();

            return outputPath;
        }

        private async Task DownloadMuxedVideoAsync(string videoId, string outputPath, string nameIA, CancellationToken cancellationToken)
        {
            var client = new YoutubeClient();
            var video = await client.Videos.GetAsync(videoId, cancellationToken);
            var manifest = await client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
            var streamInfo = manifest.GetMuxedStreams().GetWithHighestVideoQuality();

            var cleanTitle = Support.ValidateIconName(video.Title);
            var fileName = Path.Combine(outputPath, $"{cleanTitle}.{streamInfo.Container.Name}");
            var thumbnailUrl = video.Thumbnails[0].Url.ToString().Split('?')[0];
            var thumbnail = new BitmapImage(new Uri(thumbnailUrl));

            var info = $"Titulo: {video.Title}\nAutor: {video.Author}\nDuración: {video.Duration}\nFormato: {streamInfo.Container.Name}\nPeso: {streamInfo.Size}\nResolución: {streamInfo.VideoResolution}\nCalidad de Video: {streamInfo.VideoQuality}";
            _progress.AddLogMessage(info, thumbnail);

            await DownloadStreamAsync(streamInfo, fileName, cancellationToken);

            _progress.AddLogMessage($"Descarga Finalizada.\nGuardado en: {outputPath}", thumbnail);
            _progress.AddLogMessage(fileName, thumbnail);
        }

        private async Task DownloadVideoWithSeparateAudioAsync(string searchQuery, string outputPath, string nameIA, CancellationToken cancellationToken)
        {
            _progress.AddLogMessage("Buscando y analizando el video para descargar");
            var videoId = await ClassView.YouTube.Search.SearchYouTubeVideo(searchQuery);

            var client = new YoutubeClient();
            var video = await client.Videos.GetAsync(videoId, cancellationToken);
            var manifest = await client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);

            var cleanTitle = Support.ValidateIconName(video.Title);
            var thumbnailUrl = video.Thumbnails[0].Url.ToString().Split('?')[0];
            var thumbnail = new BitmapImage(new Uri(thumbnailUrl));

            var videoStream = manifest.GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestVideoQuality();
            var audioStream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var videoFile = Path.Combine(outputPath, $"{cleanTitle}_tmpV.{videoStream.Container.Name}");
            var audioFile = Path.Combine(outputPath, $"{cleanTitle}_tmpA.{audioStream.Container.Name}");

            var info = $"Titulo: {video.Title}\nAutor: {video.Author}\nDuración: {video.Duration}\nFormato: {videoStream.Container.Name}\nPeso: {videoStream.Size}\nResolución: {videoStream.VideoResolution}\nCalidad de Video: {videoStream.VideoQuality}";
            _progress.AddLogMessage(info, thumbnail);

            await DownloadStreamAsync(videoStream, videoFile, cancellationToken);
            await DownloadStreamAsync(audioStream, audioFile, cancellationToken);

            var finalFile = await MuxVideoAsync(videoFile, audioFile, cleanTitle, outputPath);

            File.Delete(videoFile);
            File.Delete(audioFile);

            _progress.AddLogMessage(finalFile, thumbnail);
        }

        private async Task<string> MuxVideoAsync(string videoPath, string audioPath, string cleanTitle, string outputPath)
        {
            ConfigureFFmpeg();
            var finalPath = Path.Combine(outputPath, $"{cleanTitle}.mp4");

            var progressHandler = new Action<double>(p => _progress.UpdateConversionProgress(p));
            var duration = FFProbe.Analyse(videoPath).Duration;

            await FFMpegArguments
                .FromFileInput(videoPath).AddFileInput(audioPath)
                .OutputToFile(finalPath, true, options => options
                    .WithVideoBitrate(2000)
                    .WithVideoCodec("libx265")
                    .WithAudioCodec("aac")
                    .WithAudioBitrate(128)
                    .WithConstantRateFactor(28)
                    .WithFastStart())
                .NotifyOnProgress(progressHandler, duration)
                .ProcessAsynchronously();

            return finalPath;
        }

        private void ConfigureFFmpeg()
        {
            GlobalFFOptions.Configure(option => option.BinaryFolder = _executablePath);
        }

        private void HandleError(Exception ex)
        {
            _progress.AddLogMessage($"Error: {ex.Message}");
            throw ex;
        }
    }
}