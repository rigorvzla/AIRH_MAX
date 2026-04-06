using System.IO;

namespace AIRH_MAX.ClassView
{
    internal class RutasAbsolutasCFG
    {
        public static string Perfil_CFG(string usuario) =>
      Path.Combine(Environment.CurrentDirectory, "Perfiles", usuario.ToLower());

        public static string Configuraciones_CFG(string usuario) =>
            Path.Combine(Environment.CurrentDirectory, "Perfiles", usuario.ToLower(), "CFG");

        public static string ITT_CFG(string usuario) =>
            Path.Combine(Environment.CurrentDirectory, "Perfiles", usuario.ToLower(), "ITT");

        public static string Email_CFG(string usuario) =>
            Path.Combine(Environment.CurrentDirectory, "Perfiles", usuario.ToLower(), "Email");
    }

    internal class RutasAbsolutas
    {
        public static string Perfil
        {
            get { return Path.Combine(Environment.CurrentDirectory, "Perfiles"); }
        }

        public static string Configuraciones
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "CFG");
            }
        }

        public static string Web
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "Web");
            }
        }

        public static string ITT
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "ITT");
            }
        }

        public static string Email
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "Email");
            }
        }

        public static string GameFolder
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "PerfilGamer");
            }
        }

        public static string NotasVoz
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "Notas de Voz");
            }
        }

        public static string Filmador
        {
            get
            {
                return Path.Combine(Perfil, Engrane.user.ToLower(), "Filmador");
            }
        }
    }
}
