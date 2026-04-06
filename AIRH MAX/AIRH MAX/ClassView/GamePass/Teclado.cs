using AIRH_MAX.Properties;
using InputSimulator_RV;

namespace AIRH_MAX.ClassView.GamePass
{
    class Teclado
    {
        public static string Comandos(string speech)
        {
            string[] consulta = DB_Lite.ConsultaComandoGamer(speech.ToLower());

            // Validar que la consulta no sea nula o vacía
            if (consulta == null || consulta.Length < 3)
            {
                Views.MainWindow.NotificacionEvent.Log = "Consulta inválida o incompleta";
                return string.Empty;
            }


            // Verificar si el comando coincide con el reconocimiento de voz
            if (speech == consulta[1] && Engrane.keyMapping.ContainsKey(consulta[0]))
            {
                ProcesarComando(Engrane.keyMapping[consulta[0]], consulta[2]);
            }
            return string.Empty;
        }

        private static void ProcesarComando(InputSimulator.Keyboard.VirtualKeyShort key, string delay)
        {
            if (delay == "0")
            {
                InputSimulator.Keyboard.PRESSKEY_ID((int)key, Settings.Default.idProcess);
            }
            else
            {
                AccionPulsar(key.ToString());
                Task.Delay(TimeSpan.FromSeconds(Convert.ToInt32(delay))).Wait();
                AccionSoltar(key.ToString());
            }
        }

        private static void AccionPulsar(string Letra)
        {
            if (Engrane.keyMapping.TryGetValue(Letra, out var keyCode))
            {
                InputSimulator.Keyboard.KEYDOWN_ID((int)keyCode, Settings.Default.idProcess);
            }
            else
            {
               Views.MainWindow.NotificacionEvent.Log = $"Tecla no reconocida: {Letra}";
            }
        }

        private static void AccionSoltar(string Letra)
        {
            if (Engrane.keyMapping.TryGetValue(Letra, out var keyCode))
            {
                InputSimulator.Keyboard.KEYUP_ID((int)keyCode, Settings.Default.idProcess);
            }
            else
            {
                Views.MainWindow.NotificacionEvent.Log = $"Tecla no reconocida: {Letra}";
            }
        }
    }
}