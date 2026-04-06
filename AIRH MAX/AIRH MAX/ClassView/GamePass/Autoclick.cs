using InputSimulator_RV;
using System.Timers;
using System.Windows.Threading;

namespace AIRH_MAX.ClassView.GamePass
{
    public class Autoclick
    {
        public static System.Timers.Timer TimerCursor = new();
        private static int currentIndex = 0;
        private static int totalRepeticiones = 0;
        private static Dispatcher dispatcher;
        private static object lockObject = new object();
        private static bool isDisposing = false;

        // Configuración del autoclick
        public static int PosX { get; set; }
        public static int PosY { get; set; }
        public static string TipoClick { get; set; } = "Izquierdo";
        public static string BotonMouse { get; set; } = "Click";
        public static int IntervaloMilisegundos { get; set; } = 1000;

        static Autoclick()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void IniciarAutoclick(int repeticiones)
        {
            if (isDisposing) return;

            try
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (isDisposing) return;

                    lock (lockObject)
                    {
                        if (TimerCursor.Enabled)
                        {
                            throw new InvalidOperationException("El modo autoclick ya está activo");
                        }

                        if (PosX == 0 || PosY == 0)
                        {
                            throw new InvalidOperationException("Seleccione la posición del ratón primero");
                        }

                        totalRepeticiones = repeticiones;
                        currentIndex = 0;

                        // Configurar timer
                        TimerCursor.Interval = IntervaloMilisegundos;
                        TimerCursor.Elapsed -= TimerCursor_Elapsed;
                        TimerCursor.Elapsed += TimerCursor_Elapsed;

                        // Posicionar cursor inicial
                        InputSimulator.MouseController.Base.SetCursorPos(PosX, PosY);

                        TimerCursor.Start();
                    }
                }, DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en IniciarAutoclick: {ex.Message}");
            }
        }

        public static void DetenerAutoclick()
        {
            if (isDisposing) return;

            try
            {
                // Detener sin usar Dispatcher para evitar problemas durante cierre
                lock (lockObject)
                {
                    TimerCursor?.Stop();
                    currentIndex = 0;
                    totalRepeticiones = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DetenerAutoclick: {ex.Message}");
            }
        }

        private static void TimerCursor_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isDisposing) return;

            try
            {
                // Verificar si la aplicación todavía está ejecutándose
                if (System.Windows.Application.Current == null ||
                    System.Windows.Application.Current.Dispatcher == null ||
                    !System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    DetenerAutoclick();
                    return;
                }

                dispatcher.Invoke(() =>
                {
                    if (isDisposing) return;

                    lock (lockObject)
                    {
                        if (!TimerCursor.Enabled || isDisposing) return;

                        // Establecer posición del cursor
                        InputSimulator.MouseController.Base.SetCursorPos(PosX, PosY);

                        // Ejecutar acción de clic
                        EjecutarClick();

                        currentIndex++;

                        // Verificar si se alcanzó el límite de repeticiones
                        if (currentIndex >= totalRepeticiones)
                        {
                            DetenerAutoclick();
                            OnAutoclickCompletado?.Invoke();
                        }
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en TimerCursor_Elapsed: {ex.Message}");
                DetenerAutoclick();
            }
        }

        private static void EjecutarClick()
        {
            if (isDisposing) return;

            try
            {
                Action clickAction = BotonMouse switch
                {
                    "Click" => TipoClick switch
                    {
                        "Izquierdo" => InputSimulator.MouseController.Click.Izquierdo,
                        "Derecho" => InputSimulator.MouseController.Click.Derecho,
                        "Medio" => InputSimulator.MouseController.Click.Medio,
                        _ => InputSimulator.MouseController.Click.Izquierdo
                    },
                    "Doble Click" => TipoClick switch
                    {
                        "Izquierdo" => () =>
                        {
                            InputSimulator.MouseController.Click.Izquierdo();
                            InputSimulator.MouseController.Click.Izquierdo();
                        }
                        ,
                        "Derecho" => () =>
                        {
                            InputSimulator.MouseController.Click.Derecho();
                            InputSimulator.MouseController.Click.Derecho();
                        }
                        ,
                        "Medio" => () =>
                        {
                            InputSimulator.MouseController.Click.Medio();
                            InputSimulator.MouseController.Click.Medio();
                        }
                        ,
                        _ => () =>
                        {
                            InputSimulator.MouseController.Click.Izquierdo();
                            InputSimulator.MouseController.Click.Izquierdo();
                        }
                    },
                    _ => InputSimulator.MouseController.Click.Izquierdo
                };

                clickAction?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en EjecutarClick: {ex.Message}");
            }
        }

        public static void ConfigurarIntervalo(int intervalo, string unidad)
        {
            if (isDisposing) return;

            var timeUnitsToMilliseconds = new Dictionary<string, Func<int, int>>
            {
                { "Dias", days => days * 24 * 60 * 60 * 1000 },
                { "Hora", hours => hours * 60 * 60 * 1000 },
                { "Minutos", minutes => minutes * 60 * 1000 },
                { "Segundos", seconds => seconds * 1000 },
                { "Milisegundos", milliseconds => milliseconds }
            };

            if (timeUnitsToMilliseconds.ContainsKey(unidad))
            {
                IntervaloMilisegundos = timeUnitsToMilliseconds[unidad](intervalo);
            }
            else
            {
                throw new ArgumentException("Unidad de tiempo no válida");
            }
        }

        public static void Comandos(string speech)
        {
            if (isDisposing) return;

            switch (speech)
            {
                case "activar_autoclick_juego":
                    try
                    {
                        IniciarAutoclick(1);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en Comandos activar_autoclick_juego: {ex.Message}");
                    }
                    break;

                case "desactivar_autoclick_juego":
                    try
                    {
                        DetenerAutoclick();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en Comandos desactivar_autoclick_juego: {ex.Message}");
                    }
                    break;
            }
        }

        // Evento para notificar cuando se completa el autoclick
        public static event Action OnAutoclickCompletado;

        // Método para limpiar recursos
        public static void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;

            try
            {
                // Detener el timer sin usar Dispatcher
                TimerCursor?.Stop();

                // Remover el event handler de manera segura
                if (TimerCursor != null)
                {
                    TimerCursor.Elapsed -= TimerCursor_Elapsed;
                    TimerCursor.Dispose();
                }

                TimerCursor = null;
                OnAutoclickCompletado = null;
                currentIndex = 0;
                totalRepeticiones = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Dispose de Autoclick: {ex.Message}");
            }
        }
    }
}