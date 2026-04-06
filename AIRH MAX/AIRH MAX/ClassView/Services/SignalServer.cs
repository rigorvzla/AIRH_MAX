using AIRH_MAX.ClassView.IA;
using AIRH_MAX.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.IO;

namespace AIRH_MAX.ClassView.Services
{
    internal class SignalServer
    {
        static private Process? _signalRProcess;
        static private HubConnection? _hubConnection;
        private const string SERVER_EXE_NAME = "VoskSignalRServer.exe";
        public static async Task StartSignalRServerAsync()
        {
            KillExistingServer();
            // 1. Iniciar el proceso del servidor
            string serverPath = Path.Combine(Environment.CurrentDirectory, "Signal", SERVER_EXE_NAME);
            if (!File.Exists(serverPath))
            {
                MainWindow.NotificacionEvent.Log = "❌ Servidor no encontrado";
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            _signalRProcess = new Process { StartInfo = startInfo };
            _signalRProcess.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ParseConsoleLine(e.Data);
            };
            _signalRProcess.Start();
            _signalRProcess.BeginOutputReadLine();

            // 2. Esperar un poco para que el servidor se inicie
            await Task.Delay(2000);

            if (_signalRProcess?.HasExited == true)
            {
                var error = _signalRProcess.StandardError.ReadToEnd();
                Debug.WriteLine($"❌ El servidor falló al iniciar. Error: {error}");
                return;
            }
            // 3. ⭐ Conectarse como cliente al mismo servidor ⭐
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/voice")
                    .Build();

                await _hubConnection.StartAsync();
                Debug.WriteLine("✅ Cliente conectado al servidor SignalR");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error al conectar cliente: {ex.Message}");
            }
        }

        private static void ParseConsoleLine(string line)
        {
            try
            {
                if (line.StartsWith("[Conexion]:"))
                {
                    var deviceId = line["[Conexion]:".Length..];
                    // 👉 ¡Nuevo dispositivo conectado!

                   App.Current.Dispatcher.Invoke(()=> MainWindow.NotificacionEvent.Log = $"🔌 Conectado: {deviceId}");
                    // Opcional: añadir a una lista de dispositivos

                }
                else if (line.StartsWith("[Desconexion]:"))
                {
                    var deviceId = line["[Desconexion]:".Length..];
                    App.Current.Dispatcher.Invoke(() => MainWindow.NotificacionEvent.Log = $"❌ Desconectado: {deviceId}");
                }
                else if (line.StartsWith("[Reconocido]:"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var text = parts[1];
                       Modulo.ProcesarComandoVoz(text);
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguear error de parsing
                App.Current.Dispatcher.Invoke(() => MainWindow.NotificacionEvent.Log = $"⚠️ Error al parsear: {ex.Message}");
            }
        }


        public static async void SendResponseToDevice(string text)
        {
            // Suponiendo que tienes una conexión SignalR activa en _hubConnection
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastSpeakText", text);
            }
        }

        /// <summary>
        /// Cierra el servidor al salir de la aplicación.
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                if (_signalRProcess != null && !_signalRProcess.HasExited)
                {
                    _signalRProcess.Kill();
                    _signalRProcess.WaitForExit(2000);
                    _signalRProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cerrar el servidor: {ex.Message}");
            }
            finally
            {
                _signalRProcess = null;
            }

            // Opcional: asegurar que no quede ningún residuo
            KillExistingServer();
        }


        /// <summary>
        /// Mata cualquier instancia previa del servidor antes de iniciarlo.
        /// </summary>
        public static void KillExistingServer()
        {
            var existingProcesses = Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(SERVER_EXE_NAME));

            foreach (var proc in existingProcesses)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit(2000); // Esperar hasta 2 segundos
                    proc.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"No se pudo terminar el proceso {proc.Id}: {ex.Message}");
                }
            }
        }
    }
}

