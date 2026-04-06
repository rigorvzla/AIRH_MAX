using AIRH_MAX.ClassView;
using AIRH_MAX.Models;
using System.Net.Http;
using System.Text.Json;

public class ApiConfigService
{
    private static readonly HttpClient httpClient = new HttpClient();
    public static async Task<ConfigResponse> GetConfigAsync()
    {
        try
        {    
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-api-key", Engrane.API_KEY);

            var response = await httpClient.GetAsync($"{Engrane.API_BASE_URL}/api/config/all");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ConfigResponse>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo configuración: {ex.Message}");
            return null;
        }
    }
}