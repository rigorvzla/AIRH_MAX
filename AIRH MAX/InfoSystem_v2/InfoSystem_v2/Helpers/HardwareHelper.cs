using System.Management;
using System.Net.NetworkInformation;

namespace InfoSystem_v2.Helpers
{
    internal class HardwareHelper
    {
        internal class NET
        {
            public static string GetActiveIP()
            {
                string ip = "";
                foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (f.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties ipInterface = f.GetIPProperties();
                        if (ipInterface.GatewayAddresses.Count > 0)
                            foreach (UnicastIPAddressInformation unicastAddress in ipInterface.UnicastAddresses)
                            {
                                if (unicastAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && unicastAddress.IPv4Mask.ToString() != "0.0.0.0")
                                {
                                    ip = unicastAddress.Address.ToString();
                                    break;

                                }
                            }
                    }
                }
                return ip;
            }
                                 
            public static NetworkInterface NetworkInterfaceAdapter(string Adaptador)
            {
                NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();

                NetworkInterface regreso = null;

                foreach (NetworkInterface network in networks)
                {
                    if (network.Name == Adaptador /*&& network.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && network.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                        && network.OperationalStatus == OperationalStatus.Up*/)
                    {
                        regreso = network;
                        break;
                    }
                }

                return regreso;
            }
        }
        internal class GPU
        {
            public static string VideoRAM = CommonHelper.NormalizeFileSize(Convert.ToInt64(GET_DATA_WMI("Win32_VideoController", "AdapterRAM")));
            public static string VideoSubNombre = GET_DATA_WMI("Win32_VideoController", "Caption");
            public static string VideoNombre = GET_DATA_WMI("Win32_VideoController", "Name");
        }
        internal class CPU
        {
            public static double CargaPorcentual
            {
                get
                {
                    return GET_DATA_WMI_DOUBLE("Win32_Processor", "LoadPercentage");
                }
            }
        }
        internal class RAM
        {
            public static ulong Disponible()
            {
                ManagementObjectSearcher objMOS = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
                ManagementObjectCollection objMOC = objMOS.Get();

                ulong disponible = 0;
                foreach (ManagementObject objMO in objMOC)
                {
                    disponible = Convert.ToUInt64(objMO["FreePhysicalMemory"]);
                }
                return disponible;
            }

            public static ulong Total()
            {
                ManagementObjectSearcher objMOS = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                ManagementObjectCollection objMOC = objMOS.Get();

                ulong total = 0;
                foreach (ManagementObject objMO in objMOC)
                {
                    total = Convert.ToUInt64(objMO["TotalVisibleMemorySize"]);
                }
                return total;
            }

            public static ulong Actual()
            {
                return Total() - Disponible();
            }

            public static double Maxima()
            {
               return GET_DATA_WMI_DOUBLE("Win32_PhysicalMemory", "Capacity");
            }
        }
        internal class HDD
        {
            internal static Dictionary<string, string> TipoDiscoDuro()
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\microsoft\windows\storage");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM MSFT_PhysicalDisk");
                string type = "";

                scope.Connect();
                searcher.Scope = scope;
                Dictionary<string, string> datos = new Dictionary<string, string>();

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    switch (Convert.ToInt16(queryObj["MediaType"]))
                    {
                        case 1:
                            type = "Unspecified";
                            break;

                        case 3:
                            type = "HDD";
                            break;

                        case 4:
                            type = "SSD";
                            break;

                        case 5:
                            type = "SCM";
                            break;

                        default:
                            type = "Unspecified";
                            break;
                    }

                    datos.Add(queryObj["Model"].ToString(), type);
                }
                searcher.Dispose();
                return datos;
            }

            internal static string ParticionPrincipal()
            {
                string Principal = string.Empty;

                ManagementObjectSearcher UnidadWin = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject os in UnidadWin.Get())
                {
                    Principal = os["SystemDrive"].ToString();
                }
                return Principal;
            }
        }
        private static string GET_DATA_WMI(string Clase, string Propiedad, string PropiedadOpcional = null)
        {
            string Dispositivo = string.Empty;
            ManagementObjectSearcher objMOS = new ManagementObjectSearcher($"Select * FROM {Clase}");
            ManagementObjectCollection objMOC = objMOS.Get();

            foreach (ManagementObject objMO in objMOC)
            {

                if (Propiedad != null && PropiedadOpcional != null)
                {
                    if (objMO[PropiedadOpcional] != null)
                    {
                        Dispositivo = objMO[Propiedad].ToString() + " || " + objMO[PropiedadOpcional].ToString();
                    }
                }
                if (Propiedad != null && PropiedadOpcional == null)
                {
                    if (objMO[Propiedad] != null)
                    {
                        Dispositivo = objMO[Propiedad].ToString();
                    }
                }
            }
            return Dispositivo;
        }
        private static double GET_DATA_WMI_DOUBLE(string Clase, string Propiedad, string PropiedadOpcional = null)
        {
            double Dispositivo = 0;
            ManagementObjectSearcher objMOS = new ManagementObjectSearcher($"Select * FROM {Clase}");
            ManagementObjectCollection objMOC = objMOS.Get();

            foreach (ManagementObject objMO in objMOC)
            {
                if (Propiedad != null && PropiedadOpcional == null)
                {
                    if (objMO[Propiedad] != null)
                    {
                        Dispositivo += Convert.ToDouble(objMO[Propiedad]);
                    }
                }
            }
            return Dispositivo;
        }
    }
}
