using System.Diagnostics;

namespace AIRH_MAX.ClassView
{
    internal class Py
    {
        public static readonly string Traductor = Environment.CurrentDirectory + "\\Addons\\Apps\\TranslateGG.exe";
        public static readonly string OCR = Environment.CurrentDirectory + "\\Addons\\Apps\\OCR.exe";
        public static readonly string NetWatcher = Environment.CurrentDirectory + "\\Addons\\Apps\\NetWatcher.exe";
        public static readonly string WhatsApp = Environment.CurrentDirectory + "\\Addons\\Apps\\WhatsSend.exe";

        public static async Task<string> Script_EXE(string PathScript, string Argumento = "")
        {
            try
            {
                var psi = new ProcessStartInfo();
                psi.FileName = PathScript;
                psi.Arguments = $@"""{Argumento}"""; // ✅ Quitar comillas extras
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true; // ✅ Agregar redirección de errores

                using var process = Process.Start(psi);
                if (process == null)
                {
                    return "Error: No se pudo iniciar el proceso";
                }

                // ✅ Leer salida de forma asíncrona
                string resultado = await process.StandardOutput.ReadToEndAsync();

                // ✅ Esperar que termine de forma asíncrona
                await process.WaitForExitAsync();

                return resultado.Replace("\r\n", "").Trim();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}