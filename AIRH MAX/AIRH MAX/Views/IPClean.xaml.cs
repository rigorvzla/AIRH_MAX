using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace AIRH_MAX.Views
{
    public partial class IPClean : Window
    {
        string nameIA;

        public IPClean(string NameIA)
        {
            InitializeComponent();
            nameIA = NameIA;
            Task.Run(IPReset);
        }

        void AddMessage(string mensaje)
        {
            var viewModel = new Models.YouTubeModel
            {
                Nombre_IA = nameIA,
                Tiempo = DateTime.Now.ToShortTimeString(),
                Mensaje = mensaje,
                ImageSource = "/PNG/AIRH.png"
            };
            Dispatcher.Invoke(() =>
            {
                lbItems.Items.Add(viewModel);
                lbItems.Items.MoveCurrentToLast();
                lbItems.ScrollIntoView(lbItems.Items.CurrentItem);
            });
        }

        public void IPReset()
        {
            try
            {
                void RunIpConfig(string args, Action<int> updateProgress)
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "IpConfig.exe",
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            // Detectar automáticamente la codificación de la consola
                            StandardOutputEncoding = Console.OutputEncoding,
                            StandardErrorEncoding = Console.OutputEncoding
                        }
                    };

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            AddMessage(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            AddMessage(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }

                // Actualizar el ProgressBar en el hilo de la interfaz de usuario
                Action<int> updateProgress = p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Dispatcher.Invoke(() => pbDown.Value = p);
                    });
                };

                // Ejecutar los comandos con actualización de progreso
                RunIpConfig("/release", updateProgress);
                updateProgress?.Invoke(50);
                RunIpConfig("/renew", updateProgress);
                updateProgress?.Invoke(100);

                AddMessage("Se ha reiniciado la dirección IP");
            }
            catch (Exception ex)
            {
                AddMessage($"Error al reiniciar la dirección IP: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
