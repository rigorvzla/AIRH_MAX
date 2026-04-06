using AIRH_MAX.Properties;
using InputSimulator_RV;
using System.Windows.Threading;

namespace AIRH_MAX.ClassView.GamePass
{
    class Autokey
    {
        static Dictionary<string, int> autokey_db;
        public static DispatcherTimer TimerAccion;
        static int counter = 0;
        static public bool secuen = false;
        static private int indiceSecuencia = 0;
        static private List<KeyValuePair<string, int>> teclasOrdenadas;
        static private object lockObject = new object();
        static private bool isDisposing = false;

        public static void AutokeyTimer(bool secuencia = false)
        {
            if (isDisposing) return;

            secuen = secuencia;

            try
            {
                // Verificar si la aplicación todavía está ejecutándose
                if (System.Windows.Application.Current == null ||
                    System.Windows.Application.Current.Dispatcher == null)
                {
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isDisposing) return;

                    autokey_db = DB_Lite.GamerAutokey();
                    counter = 0;
                    indiceSecuencia = 0;

                    // MANTENER EL ORDEN ORIGINAL de la base de datos
                    teclasOrdenadas = autokey_db.ToList(); // Sin OrderBy

                    // Detener timer si ya está corriendo
                    TimerAccion?.Stop();

                    TimerAccion = new DispatcherTimer(DispatcherPriority.Background)
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };

                    TimerAccion.Tick -= AutokeyTimer_Tick;
                    TimerAccion.Tick += AutokeyTimer_Tick;

                    TimerAccion.Start();
                }, DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AutokeyTimer: {ex.Message}");
            }
        }

        private static void AutokeyTimer_Tick(object sender, EventArgs e)
        {
            if (isDisposing) return;

            try
            {
                // Verificar si la aplicación todavía está ejecutándose
                if (System.Windows.Application.Current == null ||
                    System.Windows.Application.Current.Dispatcher == null)
                {
                    DetenerAutokey();
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isDisposing) return;

                    lock (lockObject)
                    {
                        EvaluacionTeclas();
                        counter++;

                        if (counter > GetMaxInterval() && !secuen)
                        {
                            counter = 0;
                        }
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AutokeyTimer_Tick: {ex.Message}");
                DetenerAutokey();
            }
        }

        private static void EvaluacionTeclas()
        {
            if (isDisposing) return;

            try
            {
                if (secuen)
                {
                    EvaluarModoSecuencia();
                }
                else
                {
                    EvaluarModoNormal();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en EvaluacionTeclas: {ex.Message}");
            }
        }

        private static void EvaluarModoSecuencia()
        {
            if (indiceSecuencia < teclasOrdenadas?.Count)
            {
                var teclaActual = teclasOrdenadas[indiceSecuencia];

                // En modo secuencia, ejecutar cuando el contador alcanza el tiempo de esta tecla
                if (counter >= teclaActual.Value)
                {
                    if (Engrane.keyMapping.TryGetValue(teclaActual.Key, out var keyCode))
                    {
                        try
                        {
                            InputSimulator.Keyboard.PRESSKEY_ID((int)keyCode, Settings.Default.idProcess2);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error en EvaluarModoSecuencia: {ex.Message}");
                        }

                        // Mover a la siguiente tecla y reiniciar contador
                        indiceSecuencia++;
                        counter = 0;

                        // Reiniciar ciclo si llegamos al final
                        if (indiceSecuencia >= teclasOrdenadas.Count)
                        {
                            indiceSecuencia = 0;
                        }
                    }
                }
            }
        }

        private static void EvaluarModoNormal()
        {
            if (autokey_db == null) return;

            foreach (var item in autokey_db)
            {
                if (counter == item.Value && Engrane.keyMapping.TryGetValue(item.Key, out var keyCode))
                {
                    try
                    {
                        InputSimulator.Keyboard.PRESSKEY_ID((int)keyCode, Settings.Default.idProcess2);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en EvaluarModoNormal: {ex.Message}");
                    }
                }
            }

            if (counter > GetMaxInterval())
            {
                counter = 0;
            }
        }

        private static int GetMaxInterval()
        {
            return autokey_db?.Values.Count > 0 ? autokey_db.Values.Max() : 0;
        }

        public static void DetenerAutokey()
        {
            if (isDisposing) return;

            try
            {
                // Detener sin usar Dispatcher para evitar problemas durante cierre
                lock (lockObject)
                {
                    TimerAccion?.Stop();
                    counter = 0;
                    indiceSecuencia = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DetenerAutokey: {ex.Message}");
            }
        }

        public static void ReiniciarSecuencia()
        {
            if (isDisposing) return;

            try
            {
                // Verificar si la aplicación todavía está ejecutándose
                if (System.Windows.Application.Current == null ||
                    System.Windows.Application.Current.Dispatcher == null)
                {
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isDisposing) return;

                    counter = 0;
                    indiceSecuencia = 0;

                    if (TimerAccion?.IsEnabled == true)
                    {
                        TimerAccion.Stop();
                        TimerAccion.Start();
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ReiniciarSecuencia: {ex.Message}");
            }
        }

        public static void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;

            try
            {
                // Detener el timer sin usar Dispatcher
                TimerAccion?.Stop();

                // Remover el event handler de manera segura
                if (TimerAccion != null)
                {
                    TimerAccion.Tick -= AutokeyTimer_Tick;
                }

                TimerAccion = null;
                autokey_db?.Clear();
                teclasOrdenadas?.Clear();
                counter = 0;
                indiceSecuencia = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Dispose de Autokey: {ex.Message}");
            }
        }
    }
}