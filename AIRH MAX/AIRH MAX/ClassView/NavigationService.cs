using System.Diagnostics;

namespace AIRH_MAX.ClassView
{
    public interface INavigationService
    {
        void OpenUrl(string url);
        void ShowLicenseDialog();
    }

    public class NavigationService : INavigationService
    {
        public void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL no puede estar vacía");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url.Trim(),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Nota: El manejo de errores con MessageBox debe hacerse en la UI
                throw new InvalidOperationException("No se pudo abrir el enlace. Asegúrate de tener conexión a internet.", ex);
            }
        }

        public void ShowLicenseDialog()
        {
            var licence = new VerifyLi.Activador();
            licence.ShowDialog();
        }
    }
}