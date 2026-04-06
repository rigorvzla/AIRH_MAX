using AIRH_MAX.Views;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Automation;
using System.Xml;
using Application = System.Windows.Application;

namespace AIRH_MAX.ClassView
{
    internal class SolicitudOnline
    {
        private static void BuscarNovedad(string lastNovedades, string lastVersion)
        {
            if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() != lastVersion)
            {
                Application app = Application.Current;
                if (app != null)
                {
                    app.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(
                                lastNovedades + Environment.NewLine +
                                Environment.NewLine +
                                "¿Desea descargarla?",
                                "Nueva Version: " + lastVersion,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                Engrane.EXE("https://bit.ly/AV-AIRH");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
        }

        public static void ApiConfigServiceGet()
        {
            if (!SolicitudOnline.AccesoInternet())
            {
                Debug.WriteLine("ApiConfigServiceGet: Falló en conexión a internet");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var apiConfig = await ApiConfigService.GetConfigAsync();
                    BuscarNovedad(apiConfig.Novedad, apiConfig.Version);
                }
                catch (Exception a)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(a.Message, "AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }).Wait();
        }

        public static string ObtenerClima()
        {
            if (!AccesoInternet())
            {
                return string.Empty;
            }

            string Clima = string.Empty;
            try
            {
                MainWindow.NotificacionEvent.MensajeBox = "Un segundo";
                string CityName = Engrane.File_Config().AbreviacionPais;
                string Estado = Engrane.File_Config().Provincia;
                string lang = "es";
                string key = "94ec574ab84fe48ea0babf9d8cea051f";
                string url = "http://api.openweathermap.org/data/2.5/weather?q=" + Estado + "," + CityName + "&appid=" + key + "&lang=" + lang + "&units=" + "metric" + "&mode=" + "xml";
                XmlDocument Document = new XmlDocument();
                Document.Load(url);

                string temperatura = Document.DocumentElement.SelectSingleNode("temperature").Attributes["value"].Value;
                string nubes = Document.DocumentElement.SelectSingleNode("weather").Attributes["value"].Value;
                Clima = "La temperatura de " + Engrane.File_Config().Pais + " en la región " + Estado + " es de " + temperatura + " Grados centigrados." + Environment.NewLine + "Tipo de clima: " + nubes;

                return Clima;
            }
            catch
            {
                MainWindow.NotificacionEvent.Log = Clima;
                return "Información del clima, no disponible";
            }
        }

        public static string GetWebURL()
        {
            AutomationElement element = AutomationElement.RootElement;
            System.Windows.Automation.Condition condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);

            AutomationElement browserAddressBar = element.FindFirst(TreeScope.Descendants, condition);

            string ret = string.Empty;

            if (browserAddressBar != null)
            {
                ValuePattern valuePattern = browserAddressBar.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;

                if (valuePattern != null)
                {
                    ret = valuePattern.Current.Value;
                }

            }
            else
            {
                MainWindow.NotificacionEvent.Log = "No se encontro la barra de direcciones del navegador activo.";
                ret = "No se encontró la barra de direcciones del navegador activo.";
            }

            return ret;
        }

        public static bool AccesoInternet()
        {
            try { Dns.GetHostEntry("www.google.com"); return true; }
            catch { return false; }
        }
    }
}