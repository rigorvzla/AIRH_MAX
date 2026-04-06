using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AIRH_MAX.ClassView.APIs
{
    public class EmailService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string API_BASE_URL = "https://airh-max-api-py.onrender.com";        

        public class EmailRequest
        {
            public string to { get; set; }
            public string subject { get; set; }
            public string body { get; set; }
        }

        public class EmailResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string to { get; set; }
            public string subject { get; set; }
        }

        public static async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailRequest = new EmailRequest
                {
                    to = to,
                    subject = subject,
                    body = body
                };

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);

                var json = JsonSerializer.Serialize(emailRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{API_BASE_URL}/api/email/send", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var emailResponse = JsonSerializer.Deserialize<EmailResponse>(responseJson);

                return emailResponse?.success ?? false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error enviando correo: {ex.Message}");
                return false;
            }
        }
    }
}
