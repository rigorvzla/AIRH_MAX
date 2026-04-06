using InfoSystem_v2.Services;
using MAX.ClassView;
using MAX.Views;

namespace MAX
{
    internal class StartMAX
    {
        public static void Start(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                App.Current?.Shutdown();
                return;
            }

            string command = args[0];

            if (command == "TC")
            {

                HardwareMonitorEngine.Instance.MonitorOpen();
                var temp = MonitorService.CPU().TemperaturaGeneral;
                HardwareMonitorEngine.Instance.MonitorClose();
                VozAsistente.SpeakTalk("La temperatura del cpu es de " + Math.Round(temp, 0).ToString() + " grados centígrados");

            }
            else if (command == "TG")
            {
                // ✅ USAR EL SINGLETON INSTANCE
                HardwareMonitorEngine.Instance.MonitorOpen();
                var gpu = MonitorService.GPU();
                var temp = gpu.Temperatura;
                HardwareMonitorEngine.Instance.MonitorClose();

                if (temp.HasValue)
                {
                    VozAsistente.SpeakTalk("La temperatura del gpu es de " + Math.Round(temp.Value, 0).ToString() + " grados centígrados");
                }
                else
                {
                    VozAsistente.SpeakTalk("No se pudo obtener la temperatura del gpu");
                }

            }
            else if (command == "INFO")
            {
                // 👇 Nombre único del mutex (debe ser el mismo en todas las instancias)
                const string MutexName = "Global\\MAX_INFO_INSTANCE_MUTEX";

                // Usamos 'using' para asegurar la liberación
                using (var mutex = new Mutex(false, MutexName))
                {
                    try
                    {
                        // Intentamos adquirir el mutex (esperamos 0 ms → no bloquea)
                        if (!mutex.WaitOne(0, false))
                        {
                            // Ya hay otra instancia abierta → salir silenciosamente
                            return;
                        }

                        // Solo si obtuvimos el mutex, abrimos la ventana
                        SystemPack infoSystem = new();
                        infoSystem.ShowDialog(); // Modal, bloquea hasta que se cierre
                    }
                    catch
                    {
                        // Opcional: manejar errores (ej. fallo al crear ventana)
                        // Pero no queremos que se abra otra instancia
                        throw;
                    }
                    finally
                    {
                        // Liberamos el mutex (aunque no es estrictamente necesario al salir del proceso,
                        // pero es buena práctica)
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        catch (ApplicationException)
                        {
                            // Puede lanzar excepción si el mutex ya fue liberado,
                            // pero en este caso no es crítico porque el proceso se cierra
                        }
                    }
                }
            }

            Environment.Exit(0);
        }
    }
}