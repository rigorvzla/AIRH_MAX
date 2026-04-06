using AIRH_MAX.Models;
using System.Diagnostics;

namespace AIRH_MAX.ClassView.DebugMode
{
    internal class ErrorObserver
    {
        public static void DebugResponse(ModelResponse[] responses)
        {
            string message = "Datos del Modelo:\n\n";
            foreach (var response in responses)
            {
                message += $"Model Response: {response.model_response ?? "N/A"}\n";
                message += $"User Command: {response.user_command ?? "N/A"}\n";

                if (response.user_command_extra != null)
                {
                    message += $"User Command Extra:\n";
                    message += $"  - A: {response.user_command_extra.command_a ?? "N/A"}\n";
                    message += $"  - B: {response.user_command_extra.command_b ?? "N/A"}\n";
                    message += $"  - C: {response.user_command_extra.command_c ?? "N/A"}\n";
                }
                else
                {
                    message += "User Command Extra: No disponible\n";
                }

                message += new string('-', 50) + "\n";
            }

            Debug.WriteLine(Environment.NewLine + "Comandos detectados: " + responses.Count());
            Debug.WriteLine(Environment.NewLine + message);
        }

        public static void DebugResponse(string responses)
        {
            string message = "Datos del request:\n\n" + responses;     
            Debug.WriteLine(Environment.NewLine + message);
        }
    }
}
