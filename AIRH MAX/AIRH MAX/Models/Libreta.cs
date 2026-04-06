namespace AIRH_MAX.ClassView.ViewModel
{
    public class Libreta
    {
        public class Evento
        {
            public int Id { get; set; } 
            public string EventoNombre { get; set; }
            public string Recordar { get; set; }
            public string Fecha { get; set; }
            public string Hora { get; set; }
            public string Ruta { get; set; }
            public string Accion { get; set; }
            public bool AlarmaExacta { get; set; }
        }
    }
}
