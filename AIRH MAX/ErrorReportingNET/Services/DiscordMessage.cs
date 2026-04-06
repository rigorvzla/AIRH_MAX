using Discord;
using Discord.Webhook;
using ErrorReportingNET.Models;

namespace ErrorReportingNET.Services
{
    public static class DiscordMessage
    {
        public static async Task SendEmbedAsync(Report reporte, Message.Type Type) 
        {
            using (var client = new DiscordWebhookClient(reporte.WebhookUrl))
            {
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = Message.GetReportType(Type),
                        IconUrl = "https://i.postimg.cc/xTmkZb6q/1771525426c9d7.png"
                    },
                    Title = "Producto: " + reporte.Producto,
                    Url = null,
                    Description = reporte.Descripcion,
                    Color = Message.GetColor(Type),
                    Fields =
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Version",
                            Value = reporte.Version,
                            IsInline = false
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Dispositivo",
                            Value = reporte.Dispositivo,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Geolocalización",
                            Value = reporte.Gps,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Red",
                            Value = reporte.Red,
                            IsInline = true
                        }
                    },
                    Footer = new EmbedFooterBuilder { Text = "Dev RigorVzla" },
                    Timestamp = null //DateTimeOffset.Now
                };

                // Envío asíncrono
                await client.SendMessageAsync(
                    embeds: new[] { embed.Build() }
                );
            }
        }
    }
}
