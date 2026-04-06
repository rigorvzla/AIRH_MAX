using AIRH_MAX.ClassView;
using AIRH_MAX.Views;
using FFMpegCore;
using FFMpegCore.Enums;
using System.IO;
using System.Windows;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Multimedia : System.Windows.Controls.UserControl
    {
        public Control_Multimedia()
        {
            InitializeComponent();
        }

        private async void btnCortarMultimedia_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos multimedia|*.mp4;*.avi;*.mkv;*.mov;*.mp3;*.wav";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string input = ofd.FileName;
                string output = Path.Combine(Path.GetDirectoryName(input),
                    Path.GetFileNameWithoutExtension(input) + "_cortado" + Path.GetExtension(input));

                // Obtener duración total del video
                var analysis = await FFProbe.AnalyseAsync(input);
                TimeSpan duration = analysis.Duration;

                var conversion = await FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, true, options => options
                        .WithCustomArgument($"-ss {MascaraI.Text} -to {MascaraF.Text} -vf scale={CbResolucion.Text}")
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithAudioCodec(AudioCodec.Aac))
                    .NotifyOnProgress(async (progress) =>
                    {
                        // progress está en porcentaje (0-100)
                        double progressPercentage = progress;
                        TimeSpan processedTime = TimeSpan.FromSeconds(duration.TotalSeconds * progress / 100);

                        await Dispatcher.InvokeAsync(() =>
                        {
                            progressBar.Value = progressPercentage;
                            // = $"{progressPercentage:F1}% - {processedTime:hh\\:mm\\:ss} / {duration:hh\\:mm\\:ss}";
                        });
                    }, duration)
                    .ProcessAsynchronously();

                await Dispatcher.InvokeAsync(() =>
                {
                    MainWindow.NotificacionEvent.MensajeBox = "Corte completado";
                    MainWindow.NotificacionEvent.MensajeBoxMute = $"Guardado:\n{output}";
                });
            }
        }

        private async void btnGifCrear_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Seleccionar Archivo";
            ofd.Filter = "(Video *.*)|*.rmvb;*.mp4;*.mkv;*.avi;*.mov;*.mpg;*.mpeg;*.webm;*.3gp;*.3g2;*.3gpp;*.flv;*.wmv;*.rm";

            if ((ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                try
                {
                    string input = ofd.FileName;
                    string inicio = MascaraI.Text;
                    string final = MascaraF.Text;
                    string resolucion = CbResolucion.Text;
                    string destFile = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input) + ".gif");

                    // Validar campos
                    if (string.IsNullOrWhiteSpace(inicio) || string.IsNullOrWhiteSpace(final))
                    {
                        MainWindow.NotificacionEvent.Log = "Por favor, ingresa los tiempos de inicio y final";
                        ofd.Dispose();
                        return;
                    }

                    // Obtener información del video para el progreso
                    var mediaInfo = await FFProbe.AnalyseAsync(input);
                    TimeSpan startTime = TimeSpan.Parse(inicio);
                    TimeSpan endTime = TimeSpan.Parse(final);
                    TimeSpan duration = endTime - startTime;

                    // Configurar progreso
                    Action<double> progressHandler = (percent) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            progressBar.Value = percent;

                            if (duration != TimeSpan.Zero)
                            {
                                TimeSpan processed = TimeSpan.FromSeconds(duration.TotalSeconds * percent / 100);
                            }
                        });
                    };

                    // Mantener tu mismo enfoque pero con FFMpegCore y progreso
                    await FFMpegArguments
                        .FromFileInput(input)
                        .OutputToFile(destFile, true, options => options
                            .WithCustomArgument($"-ss {inicio} -to {final} -pix_fmt rgb24 -r 10 -s {resolucion}"))
                        .NotifyOnProgress(progressHandler, duration)
                        .ProcessAsynchronously();

                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainWindow.NotificacionEvent.MensajeBox = "GIF creado exitosamente";
                        MainWindow.NotificacionEvent.MensajeBoxMute = $"GIF guardado en: {destFile}";
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainWindow.NotificacionEvent.Log = $"Error al crear GIF: {ex.Message}";
                    });
                }
                finally
                {
                    ofd.Dispose();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
        }
    }
}
