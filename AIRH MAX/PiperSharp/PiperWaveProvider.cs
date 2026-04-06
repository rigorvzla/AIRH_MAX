using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using PiperSharp.Models;

namespace PiperSharp
{
    public class PiperWaveProvider : IWaveProvider, IDisposable
    {
        public PiperConfiguration Configuration { get; set; }
        public bool Started { get; private set; } = false;
        public bool IsProcessing { get; private set; } = false;

        private Process _process;
        private RawSourceWaveStream? _internalAudioStream;
        private readonly object _lockObject = new object();

        public event Action<string>? InferenceStarted;
        public event Action<string>? InferenceCompleted;
        public event Action<string>? InferenceError;

        public PiperWaveProvider(PiperConfiguration configuration)
        {
            Configuration = configuration;
            _process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = configuration.ExecutableLocation,
                    Arguments = configuration.BuildArguments(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = configuration.WorkingDirectory,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                },
            };
            WaveFormat = new WaveFormat((int)(configuration.Model.Audio?.SampleRate ?? 16000), 1);
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (Started) return;

                _process.Start();

                // Leer errores en segundo plano
                _ = Task.Run(ReadErrorStream);

                _internalAudioStream = new RawSourceWaveStream(_process.StandardOutput.BaseStream, WaveFormat);
                Started = true;
            }
        }

        private async Task ReadErrorStream()
        {
            try
            {
                var error = await _process.StandardError.ReadToEndAsync();
                if (!string.IsNullOrEmpty(error))
                {
                    InferenceError?.Invoke(error);
                }
            }
            catch
            {
                // Ignorar errores en la lectura del error stream
            }
        }

        public Task WaitForExit(CancellationToken token = default(CancellationToken))
        {
            return _process.WaitForExitAsync(token);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (!Started)
                throw new ApplicationException("Piper process not initialized!");

            lock (_lockObject)
            {
                return _internalAudioStream!.Read(buffer, offset, count);
            }
        }

        public async Task InferPlayback(string text, CancellationToken token = default(CancellationToken))
        {
            if (!Started)
                throw new ApplicationException("Piper process not initialized!");

            lock (_lockObject)
            {
                if (IsProcessing)
                    throw new InvalidOperationException("Already processing text");

                IsProcessing = true;
            }

            try
            {
                InferenceStarted?.Invoke(text);
                await _process.StandardInput.WriteLineAsync(text.ToUtf8().AsMemory(), token);
                await _process.StandardInput.FlushAsync();
                InferenceCompleted?.Invoke(text);
            }
            finally
            {
                lock (_lockObject)
                {
                    IsProcessing = false;
                }
            }
        }

        // Método síncrono para inferencia
        public void InferPlaybackSync(string text)
        {
            if (!Started)
                throw new ApplicationException("Piper process not initialized!");

            lock (_lockObject)
            {
                if (IsProcessing)
                    throw new InvalidOperationException("Already processing text");

                IsProcessing = true;
            }

            try
            {
                InferenceStarted?.Invoke(text);
                _process.StandardInput.WriteLine(text.ToUtf8());
                _process.StandardInput.Flush();
                InferenceCompleted?.Invoke(text);
            }
            finally
            {
                lock (_lockObject)
                {
                    IsProcessing = false;
                }
            }
        }

        public WaveFormat WaveFormat { get; }

        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
                _process.WaitForExit(1000);
            }
            _process.Dispose();
            _internalAudioStream?.Dispose();
        }
    }
}