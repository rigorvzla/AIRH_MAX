using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIRH_MAX.ClassView
{
    internal class Eventos
    {
        public static string SaludoInicial()
        {
            DateTime fecha = DateTime.Now;
            if (fecha.Hour < 12) return "Buenos días " + Engrane.File_Config().Usuario;
            else if (fecha.Hour < 18) return "Buenas tardes " + Engrane.File_Config().Usuario;
            else return "Buenas noches " + Engrane.File_Config().Usuario;
        }

        public static string SaludoFinal()
        {
            DateTime fecha = DateTime.Now;
            if (fecha.Hour < 12) return "Feliz día " + Engrane.File_Config().Usuario;
            else if (fecha.Hour < 18) return "Feliz tarde " + Engrane.File_Config().Usuario;
            else return "Feliz noche " + Engrane.File_Config().Usuario;
        }

        public static void FelizNavidad(ImageBrush imageBrush)
        {
            if (DateTime.Now.Month == 12 && DateTime.Now.Day == 25)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Saludos\n¡¡Feliz Navidad!! te desea RigorVzla", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            if (DateTime.Now.Month == 1 && DateTime.Now.Day == 1)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Saludos\n¡¡Feliz Año Nuevo!! te desea RigorVzla", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            CambiarImagenSiFechaEspecial(imageBrush);
        }

        private static void CambiarImagenSiFechaEspecial(ImageBrush imageBrush)
        {
            var hoy = DateTime.Now.Date;

            string nombreRecurso;

            if (hoy.Month == 12)
            {
                nombreRecurso = "AIRH_NAV.png";
            }
            else
            {
                nombreRecurso = "AIRH.png";
            }

            imageBrush.ImageSource = new BitmapImage(
                new Uri($"pack://application:,,,/PNG/{nombreRecurso}", UriKind.Absolute));
        }
    }
}
