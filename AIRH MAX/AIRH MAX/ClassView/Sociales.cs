using AIRH_MAX.Views;

namespace AIRH_MAX.ClassView.ViewModel
{
    internal class Sociales
    {

        private static readonly Random _rnd = new Random();

        public static void Saludos()
        {
            var saludos = new[]
            {
                    $"Dime, {Engrane.File_Config().Usuario}.",
                    "¿Qué necesitas?",
                    "¿En qué puedo ayudarte?",
                    "¡Hola! ¿Cómo puedo asistirte hoy?",
                    "¡Bienvenido! Estoy aquí para lo que necesites.",
                    "Hola, ¿qué puedo hacer por ti?",
                    "¡Hola! ¿En qué puedo ser útil?",
                    "¿Algo en lo que pueda ayudarte?",
                    "¡Hola! Aquí estoy para lo que necesites."
               };

            MainWindow.NotificacionEvent.MensajeBox = ObtenerFraseAleatoria(saludos);
        }

        public static void Despedidas()
        {
            var despedidas = new[]
            {
                    $"Adiós, {Engrane.File_Config().Usuario}.",
                    "Hasta luego.",
                    "Nos vemos pronto.",
                    "¡Hasta pronto!",
                    "Hasta la próxima vez.",
                    "¡Hasta luego, amigo!",
                    "¡Hasta la vista!",
                    "¡Cuídate mucho!",
                    "¡Que tengas un gran día!",
                    "¡Nos vemos, cuídate!",
                    "¡Que todo te vaya bien!",
                    "¡Hasta pronto y ten un buen día!"
                };

            MainWindow.NotificacionEvent.MensajeBox = ObtenerFraseAleatoria(despedidas);
        }

        private static string ObtenerFraseAleatoria(string[] frases)
        {
            int index = _rnd.Next(0, frases.Length);
            return frases[index];
        }
    }
}
