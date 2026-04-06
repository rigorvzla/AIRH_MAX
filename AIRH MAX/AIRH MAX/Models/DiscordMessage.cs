namespace AIRH_MAX.Models
{
    public class DiscordMessage
    {
        public string WebhookUrl { get; set; }
        public string Mensaje { get; set; }
        public string Asistente { get; set; }
        public string Usuario { get; set; }


        public DiscordMessage(string webhookUrl, string mensaje, string asistente, string usuario)
        {
            WebhookUrl = webhookUrl;
            Mensaje = mensaje;
            Asistente = asistente;
            Usuario = usuario;
        }

        public DiscordMessage() { }
    }
}
