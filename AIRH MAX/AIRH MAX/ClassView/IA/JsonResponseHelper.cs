using System.Diagnostics;

/// <summary>
/// Clase helper para procesar respuestas JSON de IA
/// </summary>
[System.Reflection.Obfuscation(Feature = "all", Exclude = true)]
public static class JsonResponseHelper
{
    /// <summary>
    /// Procesa la respuesta JSON de la IA y la convierte en ModelResponse[]
    /// </summary>
    public static List<AIRH_MAX.Models.ModelResponse> ProcessAIResponse(string jsonResponse)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonResponse) || jsonResponse.StartsWith("Error:"))
            {
                return null;
            }

            // Extraer el JSON interno entre el primer [ y el último ]
            string cleanJson = ExtractJsonArray(jsonResponse);

            if (string.IsNullOrEmpty(cleanJson))
            {
                Debug.WriteLine("❌ No se pudo extraer array JSON válido");
                return null;
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
            };

            return System.Text.Json.JsonSerializer.Deserialize<List<AIRH_MAX.Models.ModelResponse>>(cleanJson, options);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error procesando respuesta JSON: {ex.Message}");
            Debug.WriteLine($"📄 Respuesta original: {jsonResponse}");
            return null;
        }
    }

    /// <summary>
    /// Extrae todo el contenido entre el primer [ y el último ]
    /// </summary>
    private static string ExtractJsonArray(string jsonResponse)
    {
        try
        {
            // Encontrar la posición del primer corchete de apertura [
            int startIndex = jsonResponse.IndexOf('[');
            if (startIndex == -1)
            {
                Debug.WriteLine("❌ No se encontró '[' en la respuesta");
                return null;
            }

            // Encontrar la posición del último corchete de cierre ]
            int endIndex = jsonResponse.LastIndexOf(']');
            if (endIndex == -1 || endIndex <= startIndex)
            {
                Debug.WriteLine("❌ No se encontró ']' válido en la respuesta");
                return null;
            }

            // Extraer el substring desde [ hasta ]
            string jsonArray = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);

            Debug.WriteLine($"✅ JSON extraído: {jsonArray}");
            return jsonArray;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error extrayendo array JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Procesa la respuesta JSON y devuelve solo el primer elemento
    /// </summary>
    public static AIRH_MAX.Models.ModelResponse ProcessFirstAIResponse(string jsonResponse)
    {
        var responses = ProcessAIResponse(jsonResponse);
        return responses?.FirstOrDefault();
    }

    /// <summary>
    /// Extrae solo el texto de respuesta del primer elemento
    /// </summary>
    public static string ExtractResponseText(string jsonResponse)
    {
        var response = ProcessFirstAIResponse(jsonResponse);
        return response?.model_response ?? string.Empty;
    }

    /// <summary>
    /// Método para debuggear el proceso de extracción
    /// </summary>
    public static void DebugExtraction(string rawResponse)
    {
        Debug.WriteLine("=== RAW RESPONSE ===");
        Debug.WriteLine(rawResponse);
        Debug.WriteLine("====================");

        string extractedJson = ExtractJsonArray(rawResponse);
        Debug.WriteLine("=== EXTRACTED JSON ===");
        Debug.WriteLine(extractedJson ?? "NULL");
        Debug.WriteLine("======================");
    }
}