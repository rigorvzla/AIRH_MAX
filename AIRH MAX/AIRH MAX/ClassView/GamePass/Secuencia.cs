namespace AIRH_MAX.ClassView.GamePass
{
    class Secuencia
    {
        public static void Comandos(string speech)
        {
            switch (speech)
            {
                case "activar_secuencia_juego":
                    Autokey.AutokeyTimer();
                    break;

                case "desactivar_secuencia_juego":
                    Autokey.DetenerAutokey();
                    break;
            }
        }

        public static void StartSecuencia()
        {
            Autokey.AutokeyTimer(true);
        }
    }
}