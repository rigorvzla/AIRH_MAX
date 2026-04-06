using Discord;
using Discord.Webhook;

namespace AIRH_MAX.ClassView.Services
{
    public static class DiscordMessage
    {
        public static async Task SendEmbedAsync(Models.DiscordMessage message, Theme.MessageDiscord.Type Type)
        {
            using (var client = new DiscordWebhookClient(message.WebhookUrl))
            {
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = Theme.MessageDiscord.GetReportType(Type),
                        IconUrl = "https://i.postimg.cc/xTmkZb6q/1771525426c9d7.png"
                    },
                    Title = "Asistente: " + message.Asistente,
                    Url = null,
                    Description = message.Mensaje,
                    Color = Theme.MessageDiscord.GetColor(Type),
                    Footer = new EmbedFooterBuilder { Text = "Att: " + message.Usuario }
                };

                // Envío asíncrono
                await client.SendMessageAsync(
                    embeds: new[] { embed.Build() }
                );
            }
        }
    }
}
