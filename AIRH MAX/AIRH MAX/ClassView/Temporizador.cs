using AIRH_MAX.Models;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX.ClassView
{
    internal class Temporizador
    {
        public static System.Timers.Timer Timer = new System.Timers.Timer(1000);

        public class Reminder
        {
            public string Id { get; set; }
            public string Message { get; set; }
            public DateTime DueDate { get; set; }
        }

        private static void VerificarRecordatorios()
        {
            var now = DateTime.Now;

            foreach (var reminder in DB_Lite.ObtenerRecordatorios())
            {
                if (reminder.DueDate <= now)
                {
                    DB_Lite.Eliminar("Temporizador", reminder.Id);

                    Application.Current?.Dispatcher.Invoke(() =>
                        Xceed.Wpf.Toolkit.MessageBox.Show(
                            $"¡Recordatorio!\n{reminder.Message}\n\nFecha: {reminder.DueDate}",
                            "AV-AIRH MAX",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information));
                }
            }
        }

        public static void StartTimer()
        {
            if (!DB_Lite.ObtenerRecordatorios().Any())
            {
                if (Timer.Enabled)
                {
                    Timer.Stop();
                }
                return;
            }

            if (Timer.Enabled) return;

            Timer.Elapsed += (_, _) => VerificarRecordatorios();
            Timer.AutoReset = true;
            Timer.Start();
        }


        public static void RegistroTemporizador(ModelResponseExtra extra)
        {
            if (string.IsNullOrEmpty(extra.command_a) || string.IsNullOrEmpty(extra.command_b) || string.IsNullOrEmpty(extra.command_c))
            {
                Views.MainWindow.NotificacionEvent.Log = "Error: Los valores extraídos son inválidos o incompletos. (Temporizador)";
                return;
            }

            DB_Lite.InsertarTemporizador(extra.command_a, extra.command_b, extra.command_c);
            StartTimer();
        }
    }
}