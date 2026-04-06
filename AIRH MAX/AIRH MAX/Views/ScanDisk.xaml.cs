using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX.Views
{
    public partial class ScanDisk : Window
    {
        string nameIA;
        double[] pbDatos;

        public ScanDisk(string NameIA)
        {
            InitializeComponent();
            nameIA = NameIA;
            Task.Run(Scandisk);
        }

        private void ChkdskFinal()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("chkdsk"))
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private double[] ObtenerValoresEtapaYTotal(string input)
        {
            // Expresión regular para capturar los valores de Etapa y Total
            string pattern = @"Etapa:\s*(\d+)%.*Total:\s*(\d+)%";

            // Buscar coincidencias
            Match match = Regex.Match(input, pattern);

            // Array para almacenar los valores de Etapa y Total
            double[] resultados = new double[2];

            if (match.Success)
            {
                // Capturar los valores de Etapa y Total
                resultados[0] = int.Parse(match.Groups[1].Value); // Etapa
                resultados[1] = int.Parse(match.Groups[2].Value); // Total

                return resultados;
            }

            // Si no hay coincidencias, retornar null
            return null;
        }

        private void Scandisk()
        {
            try
            {
                // Obtener todos los discos listos para ser escaneados
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).ToList();

                foreach (var drive in drives)
                {
                    try
                    {
                        Dispatcher.Invoke(() => AddMessageToLog($"Escaneando {drive.Name}..."));

                        // Ejecutar chkdsk
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "chkdsk.exe",
                                Arguments = $"{drive.Name.Remove(2)} /f /r /x",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                Verb = "runas"
                            }
                        };

                        process.OutputDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                pbDatos = ObtenerValoresEtapaYTotal(e.Data) ?? new double[] { 0, 0 };
                                UpdateProgress(e.Data, pbDatos[0], pbDatos[1]);
                            }
                        };

                        process.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                AddMessageToLog($"[ERROR] {e.Data}");
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        Task.Delay(3000).Wait();
                        process.StandardInput.WriteLine("S");
                        process.WaitForExit();

                        AddMessageToLog($"Escaneo completado para {drive.Name}");
                        ChkdskFinal();
                    }
                    catch (Exception ex)
                    {
                        AddMessageToLog($"Error al escanear {drive.Name}: {ex.Message}");
                        ChkdskFinal();
                    }
                }

                UpdateProgress("Escaneo completado.", 0, 0);
            }
            catch (Exception ex)
            {
                AddMessageToLog($"Error general: {ex.Message}");
            }
        }

        private void AddMessageToLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ViewModel = new Models.YouTubeModel
                    {
                        Nombre_IA = nameIA,
                        Tiempo = DateTime.Now.ToShortTimeString(),
                        Mensaje = message,
                        ImageSource = "/PNG/AIRH.png"
                    };
                Dispatcher.Invoke(() => lbItems.Items.Add(ViewModel));
                Dispatcher.Invoke(() => lbItems.ScrollIntoView(lbItems.Items[^1]));
            });
        }

        private void UpdateProgress(string status, double etapa, double total)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Dispatcher.Invoke(() => pbEtapa.Value = etapa);
                Dispatcher.Invoke(() => pbTotal.Value = total);
                Dispatcher.Invoke(() => AddMessageToLog($"{status}"));
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChkdskFinal();
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ChkdskFinal();
        }
    }
}
