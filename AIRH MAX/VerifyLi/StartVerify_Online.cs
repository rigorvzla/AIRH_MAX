// Licenciado/StartVerify_Online.cs
using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace VerifyLi
{
    public class StartVerify_Online
    {
        public static void Check(string _token, string _device, string URL, string Key)
        {
            Engrane.TokenTelegramBot = _token;
            Engrane.DeviceTelegram = Convert.ToInt32(_device);
            Engrane.API_BASE_URL = URL;
            Engrane.API_KEY = Key;

            try
            {
                if (!KeyManager.AccesoInternet())
                {
                    Debug.WriteLine("❌ No hay conexión a internet para validar.");
                    MessageBox.Show(
                        "No hay conexión a internet para validar la licencia de AIRH MAX.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!KeyManager.VerificarAcceso())
                {
                    Activador activador = new();
                    activador.ShowDialog();
                    Environment.Exit(0);
                }
                else
                {
                    Debug.WriteLine("✅ Verificación de licencia exitosa.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error crítico en verificación: {ex.Message}");
                MessageBox.Show(
                    "Error crítico al verificar la licencia. Contacte al soporte.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}