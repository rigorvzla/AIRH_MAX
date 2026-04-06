using InfoSystem_v2.Helpers;
using InfoSystem_v2.Models;

namespace InfoSystem_v2.Services
{
    public class SystemInfoService
    {
        /// <summary>
        /// Obtiene informacion completa del Sistema Operativo
        /// </summary> 
        public static Windows Windows()
        {
            return new Windows()
            {
                Modelo = SystemHelper.ModeloWindows(),
                Arquitectura = SystemHelper.ArquitectureSO(),
                Sistema = SystemHelper.CaptionSO(),
                Version = SystemHelper.VersionWindows(),
                NumCompilacion = SystemHelper.CompilacionSO(),
                FrameNetwork = SystemHelper.NetFramework(),
                DotNet = SystemHelper.DotNet().Split("\r\n")[0],
            };
        }

        /// <summary>
        /// Obtiene informacion completa del DirectX
        /// </summary> 
        public static DirectX DirectX()
        {
            return new DirectX()
            {
                 Directx = SystemHelper.DirectX()
            };
        }

        /// <summary>
        /// Obtiene informacion especifica del DirectX
        /// </summary> 
        public static string DirectX_Value(string value)
        {
            return SystemHelper.DirectX_Value(value);
        }
    }
}
