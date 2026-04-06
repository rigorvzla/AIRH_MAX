using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Windows;
using VerifyLi.Models;
using MessageBox = System.Windows.MessageBox;

namespace VerifyLi
{
    internal class KeyManager
    {
        static PhysicalNetworkInterface GetPrimaryMacAddressMac = NetworkInterfaceProvider.GetNetworkInterfaceObject();
        public static string GetPrimaryMacAddress = GetPrimaryMacAddressMac.MACAddress;

        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Registra el equipo a la base de datos en modo Trial usando la API.
        /// </summary>
        private static async Task<RegisterDeviceResponse> RegistrarDispositivoAsync()
        {
            string dispositivoId = GetPrimaryMacAddress;
            if (string.IsNullOrEmpty(dispositivoId))
            {
                Debug.WriteLine("❌ No se pudo obtener la dirección MAC del equipo.");
                return new RegisterDeviceResponse { Success = false, Mensaje = "No se pudo obtener MAC" };
            }

            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);
                var registerRequest = new { dispositivo_id = dispositivoId };
                var json = JsonSerializer.Serialize(registerRequest, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{Engrane.API_BASE_URL}/api/device/register", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RegisterDeviceResponse>(responseJson);

                    Debug.WriteLine($"✅ Dispositivo registrado con éxito:");
                    Debug.WriteLine($"   MAC: {dispositivoId}");
                    Debug.WriteLine($"   Mensaje: {result.Mensaje}");

                    return result;
                }
                else
                {
                    Debug.WriteLine($"❌ Error al registrar dispositivo: {response.StatusCode}");
                    return new RegisterDeviceResponse { Success = false, Mensaje = $"Error: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al registrar el dispositivo: {ex.Message}");
                return new RegisterDeviceResponse { Success = false, Mensaje = ex.Message };
            }
        }

        /// <summary>
        /// Verifica si el equipo actual puede acceder usando la API.
        /// </summary>
        public static async Task<bool> VerificarAccesoAsync()
        {
            string dispositivoId = GetPrimaryMacAddress;
            if (string.IsNullOrEmpty(dispositivoId))
            {
                Debug.WriteLine("❌ No se pudo obtener la MAC del equipo.");
                MessageBox.Show("No se pudo identificar el equipo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Debug.WriteLine($"🔍 Verificando acceso para MAC: {dispositivoId}");

            try
            {
                // Usar la API para verificar licencia
                var licenseInfo = await VerificarLicenciaAPIAsync(dispositivoId).ConfigureAwait(false);

                if (licenseInfo == null)
                {
                    Debug.WriteLine("❌ No se pudo obtener respuesta del servidor.");
                    return false;
                }

                Debug.WriteLine($"📋 Respuesta licencia: {licenseInfo.Mensaje}");

                // ✅ Caso 1: Acceso permitido
                if (licenseInfo.AccesoPermitido)
                {
                    Debug.WriteLine("✅ Licencia activada y válida. Acceso permitido.");
                    return true;
                }

                // ✅ Caso 2: Dispositivo no registrado - Registrar automáticamente
                if (!licenseInfo.Existe || licenseInfo.Mensaje?.Contains("no registrado") == true)
                {
                    Debug.WriteLine("🆕 Dispositivo no registrado. Iniciando registro Trial...");

                    var registro = await RegistrarDispositivoAsync();
                    if (registro.Success)
                    {
                        Debug.WriteLine("✅ Registro Trial exitoso. Acceso permitido.");

                        // Esperar un momento y verificar nuevamente
                        await Task.Delay(1000);
                        var nuevaVerificacion = await VerificarLicenciaAPIAsync(dispositivoId);
                        return nuevaVerificacion?.AccesoPermitido == true;
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Error en registro Trial: {registro.Mensaje}");
                        return false;
                    }
                }

                // ❌ Caso 3: Fuera del periodo de prueba u otros errores
                Debug.WriteLine($"❌ Acceso denegado: {licenseInfo.Mensaje}");
                MessageBox.Show(licenseInfo.Mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al verificar acceso: {ex.Message}");
                MessageBox.Show($"Error al verificar licencia: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Método auxiliar para verificar licencia via API
        /// </summary>
        private static async Task<LicenseResponse> VerificarLicenciaAPIAsync(string dispositivoId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);
                var licenseRequest = new LicenseRequest { DispositivoId = dispositivoId };
                var json = JsonSerializer.Serialize(licenseRequest, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{Engrane.API_BASE_URL}/api/license/verify", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LicenseResponse>(responseJson);
                }
                else
                {
                    Debug.WriteLine($"❌ Error HTTP en verificación: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en verificación API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Activa la licencia usando la API.
        /// </summary>
        public static async Task<bool> ActivarLicenciaAsync(string licencia)
        {
            if (licencia == "XXXXX-XXXXX-XXXXX-XXXXX")
            {
                MessageBox.Show("Introduzca una licencia válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // ✅ Validación inicial
            if (string.IsNullOrWhiteSpace(licencia))
            {
                Debug.WriteLine("❌ Licencia no válida.");
                return false;
            }

            string dispositivoId = GetPrimaryMacAddress;
            if (string.IsNullOrEmpty(dispositivoId))
            {
                Debug.WriteLine("❌ No se pudo obtener la MAC del equipo.");
                return false;
            }

            Debug.WriteLine($"🔍 Intentando activar licencia: {licencia}");
            Debug.WriteLine($"🔗 Asociando a MAC: {dispositivoId}");

            try
            {
                if (!AccesoInternet())
                {
                    MessageBox.Show("Se requiere conexión a internet para activar la licencia.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);

                var activateRequest = new { dispositivo_id = dispositivoId, licencia = licencia.Trim() };
                var json = JsonSerializer.Serialize(activateRequest, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{Engrane.API_BASE_URL}/api/license/activate", content).ConfigureAwait(false); ;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActivateLicenseResponse>(responseJson);

                    if (result.Success)
                    {
                        Debug.WriteLine("✅ Licencia activada con éxito via API");
                        MessageBox.Show("Licencia activada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Error en activación: {result.Mensaje}");
                        MessageBox.Show(result.Mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"❌ Error HTTP en activación: {response.StatusCode}");
                    MessageBox.Show("Error al comunicarse con el servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al activar la licencia: {ex.Message}");
                MessageBox.Show($"Error al procesar la licencia: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Solicita una licencia usando la API.
        /// </summary>
        public static async Task<bool> SolicitarLicenciaAsync(string licencia)
        {
            // ✅ Validación inicial
            if (string.IsNullOrWhiteSpace(licencia))
            {
                Debug.WriteLine("❌ Licencia no válida.");
                return false;
            }

            string dispositivoId = GetPrimaryMacAddress;
            if (string.IsNullOrEmpty(dispositivoId))
            {
                Debug.WriteLine("❌ No se pudo obtener la MAC del equipo.");
                return false;
            }

            Debug.WriteLine($"🔍 Intentando solicitar licencia: {licencia}");
            Debug.WriteLine($"🔗 Asociando a MAC: {dispositivoId}");

            try
            {
                if (!AccesoInternet())
                {
                    MessageBox.Show("Se requiere conexión a internet para solicitar la licencia.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);
                var requestLicense = new { dispositivo_id = dispositivoId, licencia = licencia.Trim() };
                var json = JsonSerializer.Serialize(requestLicense, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{Engrane.API_BASE_URL}/api/license/request", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActivateLicenseResponse>(responseJson);

                    if (result.Success)
                    {
                        Debug.WriteLine("✅ Licencia solicitada con éxito via API");
                        MessageBox.Show("Licencia asignada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Error en solicitud: {result.Mensaje}");
                        MessageBox.Show(result.Mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"❌ Error HTTP en solicitud: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al solicitar la licencia: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Método síncrono de compatibilidad (para código existente)
        /// </summary>
        public static bool VerificarAcceso()
        {
            try
            {
                var task = VerificarAccesoAsync();
                task.Wait(); // Convertir async a sync
                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en verificación síncrona: {ex.Message}");
                return false;
            }
        }

        public static bool AccesoInternet()
        {
            try { Dns.GetHostEntry("www.google.com"); return true; }
            catch { return false; }
        }

        /// <summary>
        /// Método síncrono de compatibilidad para activación
        /// </summary>
        public static bool ActivarLicencia(string licencia)
        {
            try
            {
                var task = ActivarLicenciaAsync(licencia);
                task.Wait(); // Convertir async a sync
                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en activación síncrona: {ex.Message}");
                return false;
            }
        }
    }
}