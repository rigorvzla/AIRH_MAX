using AIRH_MAX.ClassView.DebugMode;
using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Models;
using AIRH_MAX.Views;
using System.Diagnostics;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX.ClassView.IA
{
    [System.Reflection.Obfuscation(Feature = "all", Exclude = true)]
    internal class Modulo
    {
        private static async Task<ModelResponse[]> EvaluacionIA(string message)
        {
            try
            {
                var request = await ServerPlataform.SendMessage(message); 

                ErrorObserver.DebugResponse(request);

                if (request.StartsWith("Error:"))
                {
                    MainWindow.NotificacionEvent.Log = request;
                    return null;
                }

                if (string.IsNullOrEmpty(request))
                {
                    return null;
                }

                // Debuggear la extracción (opcional)
                JsonResponseHelper.DebugExtraction(request);

                // Procesar la respuesta extrayendo el array JSON
                var requestValid = JsonResponseHelper.ProcessAIResponse(request);

                if (requestValid == null)
                {
                    Debug.WriteLine("⚠️ No se pudo procesar la respuesta JSON");
                    MainWindow.NotificacionEvent.Log = "Error procesando respuesta de IA";
                }

                return requestValid?.ToArray();
            }
            catch (Exception ex)
            {
                MainWindow.NotificacionEvent.Log = ex.Message;
                return null;
            }
        }

        public static async Task RequestIA(string message)
        {
            try
            {
                ModelResponse[] responses = await EvaluacionIA(message);

                if (responses == null || responses.Length == 0)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        Xceed.Wpf.Toolkit.MessageBox.Show("Sin respuesta del API de IA", "AV-AIRH MAX",
                            MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }

                ErrorObserver.DebugResponse(responses);

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    foreach (var item in responses)
                    {
                        var respuestaIA = await ComandosSpeak.Comandos_IA(item);
                        MainWindow.NotificacionEvent.MensajeBox = !string.IsNullOrEmpty(respuestaIA)
                            ? respuestaIA
                            : item.model_response;
                    }
                });
            }
            catch (Exception ex)
            {
                MainWindow.NotificacionEvent.Log = ex.Message;
            }
        }

        public async static void ProcesarComandoVoz(string e)
        {
            if (e == string.Empty) { return; }
            if (Engrane.File_IA().Activar == false)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Active en configuración la IA");
                return;
            }

            if (e == Engrane.File_Config().Asistente.ToLower()) { Views.MainWindow.avisoActivo.Activado(); Engrane.escucha = true; Sociales.Saludos(); return; }
            if (e == Engrane.File_Config().Despedida.ToLower()) { Views.MainWindow.avisoActivo.Desactivado(); Engrane.escucha = false; Sociales.Despedidas(); return; }
            if (Engrane.escucha == false) { return; }

            Views.MainWindow.NotificacionEvent.UserVoice = e;
            await Modulo.RequestIA(e);
        }


    }
}
