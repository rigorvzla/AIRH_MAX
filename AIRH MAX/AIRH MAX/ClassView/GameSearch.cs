using System.Net.Http;
using System.Text.Json;

namespace AIRH_MAX.ClassView
{
    internal class GameSearch
    {
        static public async Task ObtenerJuegosEpicGratis(string url = "https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions?locale=es-ES")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; EpicSteamChecker/.NET)");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                var elements = root
                    .GetProperty("data")
                    .GetProperty("Catalog")
                    .GetProperty("searchStore")
                    .GetProperty("elements");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🎁 JUEGOS GRATIS EN EPIC GAMES\n");
                Console.ResetColor();

                var encontrados = false;

                foreach (var item in elements.EnumerateArray())
                {
                    var title = item.GetProperty("title").GetString();

                    if (item.TryGetProperty("promotions", out var promotionsElement) &&
                        promotionsElement.ValueKind != JsonValueKind.Null)
                    {
                        var promotionalOffers = promotionsElement
                            .GetProperty("promotionalOffers")
                            .EnumerateArray();

                        foreach (var promo in promotionalOffers)
                        {
                            foreach (var offer in promo.GetProperty("promotionalOffers").EnumerateArray())
                            {
                                var discountPercent = offer
                                    .GetProperty("discountSetting")
                                    .GetProperty("discountPercentage")
                                    .GetInt32();

                                if (discountPercent == 0)
                                {
                                    var startDate = offer.GetProperty("startDate").GetDateTime().ToShortDateString();
                                    var endDate = offer.GetProperty("endDate").GetDateTime().ToShortDateString();

                                    var slug = item
                                        .GetProperty("catalogNs")
                                        .GetProperty("mappings")
                                        .EnumerateArray()
                                        .FirstOrDefault()
                                        .GetProperty("pageSlug")
                                        .GetString() ?? "no-disponible";

                                    var gameUrl = $"https://store.epicgames.com/es-ES/p/{slug.Trim()}";

                                    Views.MainWindow.NotificacionEvent.MensajeBox = $"🎮 {title}\nDisponible desde: {startDate}\nFinaliza: {endDate}\nEnlace: {gameUrl}";
                                    encontrados = true;
                                }
                            }
                        }
                    }
                }

                if (!encontrados)
                {
                    Console.WriteLine("No hay juegos gratuitos esta semana en Epic Games.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo juegos de Epic Games: {ex.Message}\n");
            }
        }

        public static async Task ObtenerOfertasSteam(string url = "https://store.steampowered.com/api/featuredcategories")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; EpicSteamChecker/.NET)");
                var response = await httpClient.GetStringAsync(url);
                using var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("specials", out var specialsElement) ||
                    !specialsElement.TryGetProperty("items", out var itemsElement))
                {
                    Console.WriteLine("❌ No se encontró la sección 'specials' en Steam.\n");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("🔥 OFERTAS DESTACADAS EN STEAM (>50% OFF)\n");
                Console.ResetColor();

                var foundAny = false;

                foreach (var item in itemsElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("name", out var nameElement)) continue;

                    var name = nameElement.GetString() ?? "Desconocido";
                    if (!item.TryGetProperty("discounted", out var discountedElement) || !discountedElement.GetBoolean()) continue;
                    if (!item.TryGetProperty("discount_percent", out var discountPercentElement)) continue;

                    var discountPercent = discountPercentElement.GetInt32();
                    if (discountPercent <= 50) continue;

                    foundAny = true;

                    // Precio
                    var finalPrice = item.TryGetProperty("final_price", out var fp) ? fp.GetInt32() : 0;
                    var currency = item.TryGetProperty("currency", out var curr) ? curr.GetString() : "USD";
                    var priceFormatted = $"{(finalPrice / 100.0):F2} {currency}";

                    // Fecha de expiración
                    var expiryText = "Indefinido";
                    if (item.TryGetProperty("discount_expiration", out var expElement))
                    {
                        var timestamp = expElement.GetInt64();
                        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
                        expiryText = expiryDate.ToString("dd-MM-yyyy");
                    }

                    // ID del juego
                    var appId = item.TryGetProperty("id", out var idElement) ? idElement.GetInt32().ToString() : "0";
                    var gameUrl = $"https://store.steampowered.com/app/{appId}";

                    // Mostrar con colores
                    if (discountPercent == 100)
                    {
                        Views.MainWindow.NotificacionEvent.MensajeBox = $"🎉 ¡GRATIS! {name}\nPrecio final: {priceFormatted}";
                    }
                    else
                    {
                        Views.MainWindow.NotificacionEvent.MensajeBox = $"🎉 ¡🎁 {discountPercent}% OFF: {name}\nPrecio final: {priceFormatted}\nOferta hasta: {expiryText}\nEnlace: {gameUrl}";
                    }
                }

                if (!foundAny)
                {
                    Console.WriteLine("No se encontraron juegos en Steam con más del 50% de descuento.\n");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"❌ Error de red con Steam: {httpEx.Message}\n");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"❌ Error al procesar JSON de Steam: {jsonEx.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado en Steam: {ex.Message}\n");
            }
        }
    }
}
