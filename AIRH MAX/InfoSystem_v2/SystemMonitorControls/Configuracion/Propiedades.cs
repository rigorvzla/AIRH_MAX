using SystemMonitorControls.Properties;

namespace SystemMonitorControls.Configuracion
{
    internal class Propiedades
    {
        public class Estado
        {
            public static bool HDD { get => Settings.Default.EncendidoHDD; }
            public static bool CPU { get => Settings.Default.EncendidoCPU; }
            public static bool GPU { get => Settings.Default.EncendidoGPU; }
            public static bool RAM { get => Settings.Default.EncendidoRAM; }
            public static bool RED { get => Settings.Default.EncendidoRED; }
        }

        public class GuardarPropiedad
        {
            public static void CPU(bool encender, string recurso, string temperatura)
            {
                Settings.Default.RecursoCPU = recurso;
                Settings.Default.TemperaturaCPU = temperatura;
                Settings.Default.EncendidoCPU = encender;
                Settings.Default.Save();
            }

            public static void HDD(bool encender, string recurso, string temperatura)
            {
                Settings.Default.RecursoHDD = recurso;
                Settings.Default.TemperaturaHDD = temperatura;
                Settings.Default.EncendidoHDD = encender;
                Settings.Default.Save();
            }

            public static void GPU(bool encender, string recurso, string temperatura)
            {
                Settings.Default.TemperaturaGPU = temperatura;
                Settings.Default.RecursoNVIDIA = recurso;
                Settings.Default.EncendidoGPU = encender;
                Settings.Default.Save();
            }

            public static void RAM(bool encender, string recurso)
            {
                Settings.Default.RecursoRAM = recurso;
                Settings.Default.EncendidoRAM = encender;
                Settings.Default.Save();
            }

            public static void RED(bool encender, string recurso)
            {
                Settings.Default.BajadaRED = recurso.Split(' ')[0];
                Settings.Default.SubidaRED = recurso.Split(' ')[1];
                Settings.Default.EncendidoRED = encender;
                Settings.Default.Save();
            }

            public static void Descarga_TestRED(string recurso)
            {
                Settings.Default.Vel_DescargaRED = recurso;
                Settings.Default.Save();
            }

            public static void Subida_TestRED(string recurso)
            {
                Settings.Default.Vel_SubidaRED = recurso;
                Settings.Default.Save();
            }
        }

        public class CargarPropiedad
        {
            public static List<string> CPU()
            {
                List<string> list = new List<string>();
                list.Add(Settings.Default.RecursoCPU);
                list.Add(Settings.Default.TemperaturaCPU);
                return list;
            }

            public static List<string> HDD()
            {
                List<string> list = new List<string>();
                list.Add(Settings.Default.RecursoHDD);
                list.Add(Settings.Default.TemperaturaHDD);
                return list;
            }

            public static List<string> GPU_NVIDIA()
            {
                List<string> list = new List<string>();
                list.Add(Settings.Default.TemperaturaGPU);
                list.Add(Settings.Default.RecursoNVIDIA);
                return list;
            }

            public static List<string> RED()
            {
                List<string> list = new List<string>();
                list.Add(Settings.Default.BajadaRED);
                list.Add(Settings.Default.SubidaRED);
                return list;
            }

            public static string RAM()
            {
                return Settings.Default.RecursoRAM;
            }

            public static string Vel_DescargaRED()
            {
                if (Settings.Default.Vel_DescargaRED.Equals(string.Empty))
                {
                    return "0";
                }
                else
                {
                    return Settings.Default.Vel_DescargaRED;
                }
            }

            public static string Vel_SubidaRED()
            {
                if (Settings.Default.Vel_SubidaRED.Equals(string.Empty))
                {
                    return "0";
                }
                else
                {
                    return Settings.Default.Vel_SubidaRED;
                }
            }
        }
    }
}
