using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Diagnostics;
using System.IO;

namespace AIRH_MAX.ClassView
{
    internal class WebStore
    {
        public static async Task GuardarWeb()
        {
            try
            {
                string url = SolicitudOnline.GetWebURL();

                // Primera validación: campos vacíos y mensajes de error
                if (string.IsNullOrWhiteSpace(url) || url.Contains("No se encontró"))
                {
                    Views.MainWindow.NotificacionEvent.Log = "❌ URL no válida o no se detectó navegador activo.";
                    return;
                }

                // Asegurar que la URL tenga esquema ANTES de validar
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                // Ahora validar la URL bien formada CON el esquema
                if (!Uri.IsWellFormedUriString(url.Trim(), UriKind.Absolute))
                {
                    Views.MainWindow.NotificacionEvent.Log = "❌ URL no válida o no se detectó navegador activo.";
                    return;
                }

                Debug.WriteLine("Verificando/Descargando Chromium...");
                Views.MainWindow.NotificacionEvent.MensajeBoxMute = "Verificando/Descargando Chromium...";
                await new BrowserFetcher().DownloadAsync();

                Debug.WriteLine("Iniciando navegador headless...");
                Views.MainWindow.NotificacionEvent.MensajeBoxMute = "Guardando Web...";
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                // Obtener el título real de la página para nombrar el archivo
                string titulo = await page.GetTitleAsync();
                string nombreArchivo = SanitizeFileName(titulo ?? "pagina_web");

                // Definir rutas
                string carpetaDestino = RutasAbsolutas.Web; 
                Directory.CreateDirectory(carpetaDestino); // Asegurar que la carpeta exista

                string rutaPdf = Path.Combine(carpetaDestino, $"{nombreArchivo}.pdf");
                //string rutaPng = Path.Combine(carpetaDestino, $"{nombreArchivo}.png");

                // Guardar archivos
                //await page.ScreenshotAsync(rutaPng);
                await page.PdfAsync(rutaPdf, new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "10mm",
                        Bottom = "10mm",
                        Left = "10mm",
                        Right = "10mm"
                    }
                });

                //Debug.WriteLine($"✅ Archivos guardados:\n - {rutaPdf}\n - {rutaPng}");
                Views.MainWindow.NotificacionEvent.MensajeBoxMute = $"✅ Archivo guardado:\n - {rutaPdf}";
                Views.MainWindow.NotificacionEvent.MensajeBox = $"Página guardada";
            }
            catch (Exception ex)
            {
                string mensaje = $"❌ Error al guardar página: {ex.Message}";
                Debug.WriteLine(mensaje);
                Views.MainWindow.NotificacionEvent.Log = mensaje;
            }
        }

        // 🔹 Función auxiliar para limpiar nombres de archivo
        private static string SanitizeFileName(string nombre)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(c, '_');
            }
            nombre = System.Text.RegularExpressions.Regex.Replace(nombre.Trim(), @"\s+", " ");
            return string.IsNullOrWhiteSpace(nombre) ? "pagina_web" : nombre;
        }
    }
}
