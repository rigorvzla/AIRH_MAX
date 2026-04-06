using System.Text.Json.Serialization;

namespace VerifyLi.Models
{
    public class LicenseResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("acceso_permitido")]
        public bool AccesoPermitido { get; set; }

        [JsonPropertyName("existe")]
        public bool Existe { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("licencia")]
        public string Licencia { get; set; }

        [JsonPropertyName("fecha_activacion")]
        public string FechaActivacion { get; set; }

        [JsonPropertyName("fecha_vencimiento")]
        public string FechaVencimiento { get; set; }
    }

    public class RegisterDeviceResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("licencia")]
        public string Licencia { get; set; }
    }

    public class ActivateLicenseResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }
    }

    public class LicenseRequest
    {
        [JsonPropertyName("dispositivo_id")]
        public string DispositivoId { get; set; }
    }
}
