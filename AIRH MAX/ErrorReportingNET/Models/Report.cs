namespace ErrorReportingNET.Models
{
    public class Report
    {
        public string WebhookUrl { get; set; }
        public string Descripcion { get; set; }
        public string Producto { get; set; }
        public string Version { get; set; }
        public string Dispositivo { get; set; }
        public string Red { get; set; }
        public string Gps { get; set; }


        public Report(string webhookUrl, string descripcion, string producto, string version, string dispositivo, string red, string gps)
        {
            WebhookUrl = webhookUrl;
            Descripcion = descripcion;
            Producto = producto;
            Version = version;
            Dispositivo = dispositivo;
            Red = red;
            Gps = gps;
        }

        public Report() { }
    }
}
