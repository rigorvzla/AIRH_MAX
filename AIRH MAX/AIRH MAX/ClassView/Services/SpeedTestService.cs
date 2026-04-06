// Services/SpeedTestService.cs
using NetPace.Core;
using NetPace.Core.Clients.Ookla;

namespace AIRH_MAX.ClassView.Services
{
    public interface ISpeedTestProgress
    {
        void UpdateMessage(string message);
        void UpdateProgress(double percentage);
    }

    public class SpeedTestService
    {
        private readonly ISpeedTestService _speedTester;
        private readonly ISpeedTestProgress _progress;

        public SpeedTestService(ISpeedTestProgress progress, ISpeedTestService speedTester = null)
        {
            _progress = progress;
            _speedTester = speedTester ?? new OoklaSpeedtest();
        }

        public async Task<string> RunSpeedTestAsync(string mode, CancellationToken cancellationToken = default)
        {
            try
            {
                IServer[] servers = null;
                IServer fastestServer = null;

                // Latencia (siempre se ejecuta)
                _progress.UpdateMessage("Buscando servidores...");
                _progress.UpdateProgress(0);
                servers = await _speedTester.GetServersAsync(cancellationToken);

                _progress.UpdateMessage("Calculando latencia...");
                var latencyResult = await _speedTester.GetFastestServerByLatencyAsync(servers, cancellationToken);
                fastestServer = latencyResult.Server;

                var pingMessage = $"{latencyResult.Server.Sponsor} - Latencia: {latencyResult.LatencyMilliseconds} ms";
                _progress.UpdateMessage(pingMessage);
                var pingResult = $"Tu latencia es de {latencyResult.LatencyMilliseconds} ms";

                // Modo específico
                switch (mode.ToUpper())
                {
                    case "D":
                        await RunDownloadTestAsync(fastestServer, cancellationToken);
                        break;
                    case "U":
                        await RunUploadTestAsync(fastestServer, cancellationToken);
                        break;
                    case "P":
                        // Solo latencia, ya completada
                        break;
                }

                return pingResult;
            }
            catch (Exception ex)
            {
                _progress.UpdateMessage($"Error: {ex.Message}");
                _progress.UpdateProgress(0);
                throw;
            }
        }

        private async Task RunDownloadTestAsync(IServer server, CancellationToken cancellationToken)
        {
            try
            {
                _progress.UpdateMessage("Descargando...");
                _progress.UpdateProgress(0);

                var progressReporter = new Progress<SpeedTestProgress>(p =>
        _progress.UpdateProgress(p.PercentageComplete));

                var result = await _speedTester.GetDownloadSpeedAsync(
                    server,
                    progressReporter,
                    cancellationToken);

                var speedStr = result.GetSpeedString(SpeedUnit.BitsPerSecond, SpeedUnitSystem.SI);
                _progress.UpdateMessage($"Velocidad de descarga: {speedStr}");
                _progress.UpdateProgress(100);
            }
            catch (Exception ex)
            {
                _progress.UpdateMessage($"Error en Descarga: {ex.Message}");
                _progress.UpdateProgress(0);
                throw;
            }
        }

        private async Task RunUploadTestAsync(IServer server, CancellationToken cancellationToken)
        {
            try
            {
                _progress.UpdateMessage("Subiendo...");
                _progress.UpdateProgress(0);

                var progressReporter = new Progress<SpeedTestProgress>(p =>
     _progress.UpdateProgress(p.PercentageComplete));

                var result = await _speedTester.GetUploadSpeedAsync(
                    server,
                    progressReporter,
                    cancellationToken);

                var speedStr = result.GetSpeedString(SpeedUnit.BitsPerSecond, SpeedUnitSystem.SI);
                _progress.UpdateMessage($"Velocidad de subida: {speedStr}");
                _progress.UpdateProgress(100);
            }
            catch (Exception ex)
            {
                _progress.UpdateMessage($"Error en Subida: {ex.Message}");
                _progress.UpdateProgress(0);
                throw;
            }
        }
    }
}