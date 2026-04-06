using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIRH_MAX.ClassView
{
    public static class NetworkInterfaceProvider
    {
        /// <summary>
        /// Método principal que devuelve la interfaz de red activa como objeto PhysicalNetworkInterface
        /// </summary>
        /// <returns>Objeto PhysicalNetworkInterface con toda la información</returns>
        public static PhysicalNetworkInterface GetNetworkInterfaceObject()
        {
            return NetworkInterfaceDetector.GetActiveInternetInterface();
        }

        /// <summary>
        /// Versión con parámetro para incluir solo interfaces con gateway
        /// </summary>
        /// <param name="requireGateway">True para solo interfaces con gateway (internet)</param>
        /// <returns>Objeto PhysicalNetworkInterface o null si no cumple criterios</returns>
        public static PhysicalNetworkInterface GetNetworkInterfaceObject(bool requireGateway = true)
        {
            var interfaceObj = NetworkInterfaceDetector.GetActiveInternetInterface();

            if (requireGateway && interfaceObj != null && !interfaceObj.HasGateway)
                return null;

            return interfaceObj;
        }

        /// <summary>
        /// Obtiene todas las interfaces físicas como lista de objetos
        /// </summary>
        public static List<PhysicalNetworkInterface> GetAllPhysicalInterfaces()
        {
            var interfaces = new List<PhysicalNetworkInterface>();
            var virtualAdapters = new HashSet<string>
        {
            "hamachi", "radmin", "hyper-v", "virtual", "teredo"
        };

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                string name = ni.Name.ToLower();
                string desc = ni.Description.ToLower();
                bool isVirtual = false;

                foreach (var keyword in virtualAdapters)
                {
                    if (name.Contains(keyword) || desc.Contains(keyword))
                    {
                        isVirtual = true;
                        break;
                    }
                }

                if (isVirtual) continue;

                var interfaceObj = CreateInterfaceObject(ni);
                if (interfaceObj != null)
                {
                    interfaces.Add(interfaceObj);
                }
            }

            return interfaces;
        }

        /// <summary>
        /// Obtiene interfaz por nombre específico
        /// </summary>
        /// <param name="interfaceName">Nombre de la interfaz (ej: "Ethernet", "Wi-Fi")</param>
        public static PhysicalNetworkInterface GetInterfaceByName(string interfaceName)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    return CreateInterfaceObject(ni);
                }
            }
            return null;
        }

        /// <summary>
        /// Obtiene interfaz por dirección MAC
        /// </summary>
        /// <param name="macAddress">Dirección MAC (con o sin separadores)</param>
        public static PhysicalNetworkInterface GetInterfaceByMAC(string macAddress)
        {
            string cleanMac = macAddress.Replace(":", "").Replace("-", "").ToUpper();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var mac = ni.GetPhysicalAddress()?.ToString();
                if (!string.IsNullOrEmpty(mac) && mac.ToUpper() == cleanMac)
                {
                    return CreateInterfaceObject(ni);
                }
            }
            return null;
        }

        /// <summary>
        /// Obtiene interfaz por dirección IP
        /// </summary>
        /// <param name="ipAddress">Dirección IPv4</param>
        public static PhysicalNetworkInterface GetInterfaceByIP(string ipAddress)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProperties = ni.GetIPProperties();
                foreach (var addr in ipProperties.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        addr.Address.ToString() == ipAddress)
                    {
                        return CreateInterfaceObject(ni);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Método auxiliar para crear el objeto desde NetworkInterface
        /// </summary>
        private static PhysicalNetworkInterface CreateInterfaceObject(NetworkInterface ni)
        {
            // Usar el método existente de NetworkInterfaceDetector o crear uno similar
            return NetworkInterfaceDetector.CreatePhysicalInterfaceInfo(ni, false);
        }
    }

    public static class GeolocationCoordinates
    {
        /// <summary>
        /// Obtiene las coordenadas (latitud, longitud) formateadas para Google Maps
        /// </summary>
        /// <returns>String en formato "latitud,longitud" o null si hay error</returns>
        public static string GetCoordinatesForGoogleMaps()
        {
            if (!GeolocationService.HasInternetAccess())
                return null;

            try
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();

                if (geoInfo != null && geoInfo.Success)
                {
                    // Formato: "latitud,longitud" (con punto decimal)
                    return $"{geoInfo.Lat},{geoInfo.Lon}";
                }
            }
            catch
            {
                // Silenciar errores
            }

            return null;
        }

        /// <summary>
        /// Obtiene coordenadas con formato específico para Google Maps URL
        /// </summary>
        /// <returns>String en formato "@lat,lon,z" donde z es zoom (opcional)</returns>
        public static string GetGoogleMapsCoordinates(int zoomLevel = 15)
        {
            var coords = GetCoordinatesForGoogleMaps();

            if (string.IsNullOrEmpty(coords))
                return null;

            // Formato para Google Maps URL: @lat,lon,z
            return $"{coords},{zoomLevel}z";
        }

        /// <summary>
        /// Obtiene URL completa de Google Maps con las coordenadas
        /// </summary>
        /// <param name="zoomLevel">Nivel de zoom (default 15)</param>
        /// <returns>URL completa para abrir en navegador</returns>
        public static string GetGoogleMapsUrl(int zoomLevel = 15)
        {
            var coords = GetGoogleMapsCoordinates(zoomLevel);

            if (string.IsNullOrEmpty(coords))
                return null;

            return $"https://www.google.com/maps/@{coords}?hl=es";
        }

        /// <summary>
        /// Obtiene coordenadas en formato para iframe/embed de Google Maps
        /// </summary>
        /// <returns>Coordenadas para embed maps</returns>
        public static string GetEmbedCoordinates()
        {
            var coords = GetCoordinatesForGoogleMaps();

            if (string.IsNullOrEmpty(coords))
                return null;

            // Para embed: "lat,lon"
            return coords;
        }

        /// <summary>
        /// Método específico para tu caso: "@coordenadasz?hl=es"
        /// </summary>
        public static string GetLocIP()
        {
            var coords = GetCoordinatesForGoogleMaps();

            if (string.IsNullOrEmpty(coords))
                return null;

            // Formato exacto que usas: @lat,lonz?hl=es
            // NOTA: Faltaba la coma después de las coordenadas en tu ejemplo
            return $"@{coords}z?hl=es";
        }

        /// <summary>
        /// Versión mejorada con validación
        /// </summary>
        public static string GetLocIPImproved()
        {
            if (!GeolocationService.HasInternetAccess())
                return null;

            try
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();

                if (geoInfo != null && geoInfo.Success && geoInfo.Lat != 0 && geoInfo.Lon != 0)
                {
                    // Formato para tu Engrane.EXE: @lat,lonz?hl=es
                    // Zoom level 15 por defecto (z15)
                    return $"@{geoInfo.Lat},{geoInfo.Lon}z15?hl=es";
                }
            }
            catch
            {
                // Log error si es necesario
            }

            return null;
        }
    }

    public class GeolocationInfo
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("query")]
        public string PublicIP { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("regionName")]
        public string RegionName { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("zip")]
        public string Zip { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("isp")]
        public string Isp { get; set; }

        [JsonPropertyName("org")]
        public string Org { get; set; }

        [JsonPropertyName("as")]
        public string As { get; set; }

        [JsonIgnore]
        public bool Success => Status == "success";

        public override string ToString()
        {
            if (!Success)
                return $"Error: {Message}";

            return $@"Informacion de Geolocalizacion
--------------------------------
IP Publica: {PublicIP}
Ubicacion: {City}, {RegionName}, {Country}
Coordenadas: {Lat}, {Lon}
Codigo Postal: {Zip}
Zona Horaria: {Timezone}
Proveedor: {Isp}
Organizacion: {Org}";
        }
    }

    public class PhysicalNetworkInterface
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Id { get; set; }
        public string IPv4Address { get; set; }
        public string IPv4Mask { get; set; }
        public List<string> AllIPv4Addresses { get; set; }
        public List<string> IPv6Addresses { get; set; }
        public string MACAddress { get; set; }
        public string RawMACAddress { get; set; }
        public string Gateway { get; set; }
        public bool HasGateway { get; set; }
        public List<string> DnsServers { get; set; }
        public string Speed { get; set; }
        public long SpeedBps { get; set; }
        public bool IsPhysical { get; set; }
        public bool IsPrimaryInternetInterface { get; set; }
        public bool SupportsIPv4 { get; set; }
        public bool SupportsIPv6 { get; set; }
        public bool SupportsMulticast { get; set; }
        public bool IsReceiveOnly { get; set; }

        public override string ToString()
        {
            return $@"Interfaz de Internet Principal:
-------------------------------
Nombre: {Name}
Descripcion: {Description}
MAC Address: {MACAddress}
IPv4: {IPv4Address} / {IPv4Mask}
Gateway: {Gateway}
Velocidad: {Speed}
Estado: {Status}
Tipo: {Type}";
        }
    }

    public static class NetworkInterfaceDetector
    {
        private static readonly HashSet<string> VirtualAdapters = new HashSet<string>
        {
            "hamachi", "radmin", "radmin vpn", "hyper-v", "virtual",
            "teredo", "tunneling", "microsoft virtual", "microsoft wi-fi direct",
            "vpn", "wan miniport", "vEthernet", "docker", "vmware",
            "virtualbox", "vbox", "wireguard", "openvpn", "loopback",
            "microsoft", "bluetooth", "pseudo", "tap-", "tapw", "npcap"
        };

        public static PhysicalNetworkInterface GetActiveInternetInterface()
        {
            PhysicalNetworkInterface result = null;

            string activeLocalIp = GetLocalIpForInternet();
            if (!string.IsNullOrEmpty(activeLocalIp))
            {
                result = FindInterfaceByIp(activeLocalIp, true);
                if (result != null) return result;
            }

            result = FindInterfaceByGateway();
            if (result != null) return result;

            return FindFirstInterfaceWithIp();
        }

        private static PhysicalNetworkInterface FindInterfaceByIp(string ip, bool isPrimary)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!IsValidInterface(ni)) continue;

                var ipProperties = ni.GetIPProperties();
                bool foundIp = false;

                foreach (var addr in ipProperties.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        addr.Address.ToString() == ip)
                    {
                        foundIp = true;
                        break;
                    }
                }

                if (foundIp)
                {
                    return CreatePhysicalInterfaceInfo(ni, isPrimary);
                }
            }
            return null;
        }

        private static PhysicalNetworkInterface FindInterfaceByGateway()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!IsValidInterface(ni)) continue;

                var ipProperties = ni.GetIPProperties();
                bool hasValidGateway = false;

                foreach (var gateway in ipProperties.GatewayAddresses)
                {
                    if (gateway.Address.AddressFamily == AddressFamily.InterNetwork &&
                        gateway.Address.ToString() != "0.0.0.0" &&
                        !gateway.Address.ToString().StartsWith("169.254"))
                    {
                        hasValidGateway = true;
                        break;
                    }
                }

                if (hasValidGateway)
                {
                    return CreatePhysicalInterfaceInfo(ni, false);
                }
            }
            return null;
        }

        private static PhysicalNetworkInterface FindFirstInterfaceWithIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!IsValidInterface(ni)) continue;

                var ipProperties = ni.GetIPProperties();
                bool hasIpv4 = false;

                foreach (var addr in ipProperties.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        hasIpv4 = true;
                        break;
                    }
                }

                if (hasIpv4)
                {
                    return CreatePhysicalInterfaceInfo(ni, false);
                }
            }
            return null;
        }

        private static bool IsValidInterface(NetworkInterface ni)
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                return false;

            return !IsVirtualInterface(ni);
        }

        private static bool IsVirtualInterface(NetworkInterface ni)
        {
            string name = ni.Name.ToLower();
            string description = ni.Description.ToLower();

            foreach (var keyword in VirtualAdapters)
            {
                if (name.Contains(keyword) || description.Contains(keyword))
                    return true;
            }

            var interfaceType = ni.NetworkInterfaceType;
            if (interfaceType == NetworkInterfaceType.Tunnel ||
                interfaceType == NetworkInterfaceType.Loopback ||
                interfaceType == NetworkInterfaceType.Unknown)
                return true;

            long speed = ni.Speed;
            if (speed == 0 || speed == -1 || speed == long.MaxValue)
                return true;

            if (description.Contains("miniport") && description.Contains("wan"))
                return true;

            return false;
        }

        private static string GetLocalIpForInternet()
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 53);
                    var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                    return localEndPoint?.Address.ToString();
                }
            }
            catch
            {
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("1.1.1.1", 53);
                        var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                        return localEndPoint?.Address.ToString();
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static PhysicalNetworkInterface CreatePhysicalInterfaceInfo(
            NetworkInterface ni, bool isPrimaryInternet)
        {
            var ipProperties = ni.GetIPProperties();

            var ipv4Addresses = new List<string>();
            var ipv4Masks = new List<string>();
            string primaryIpv4 = null;
            string primaryMask = null;

            foreach (var addr in ipProperties.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ip = addr.Address.ToString();
                    string mask = addr.IPv4Mask?.ToString();

                    ipv4Addresses.Add(ip);
                    if (mask != null) ipv4Masks.Add(mask);

                    if (primaryIpv4 == null)
                    {
                        primaryIpv4 = ip;
                        primaryMask = mask;
                    }
                }
            }

            var ipv6Addresses = new List<string>();
            foreach (var addr in ipProperties.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ipv6Addresses.Add(addr.Address.ToString());
                }
            }

            string gatewayIp = null;
            foreach (var gateway in ipProperties.GatewayAddresses)
            {
                if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    gatewayIp = gateway.Address.ToString();
                    break;
                }
            }

            var dnsServers = new List<string>();
            foreach (var dns in ipProperties.DnsAddresses)
            {
                dnsServers.Add(dns.ToString());
            }

            var macAddress = ni.GetPhysicalAddress()?.ToString();
            string formattedMac = FormatMacAddress(macAddress);

            return new PhysicalNetworkInterface
            {
                Name = ni.Name,
                Description = ni.Description,
                Type = GetInterfaceTypeString(ni.NetworkInterfaceType),
                Status = GetOperationalStatusString(ni.OperationalStatus),
                IPv4Address = primaryIpv4,
                IPv4Mask = primaryMask,
                AllIPv4Addresses = ipv4Addresses,
                IPv6Addresses = ipv6Addresses,
                MACAddress = formattedMac,
                RawMACAddress = macAddress,
                Speed = FormatSpeed(ni.Speed),
                SpeedBps = ni.Speed,
                Gateway = gatewayIp,
                HasGateway = gatewayIp != null && gatewayIp != "0.0.0.0",
                DnsServers = dnsServers,
                IsPhysical = true,
                IsPrimaryInternetInterface = isPrimaryInternet,
                SupportsIPv4 = ipv4Addresses.Count > 0,
                SupportsIPv6 = ipv6Addresses.Count > 0,
                Id = ni.Id,
                SupportsMulticast = ni.SupportsMulticast,
                IsReceiveOnly = ni.IsReceiveOnly
            };
        }

        private static string FormatMacAddress(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress) || macAddress.Length != 12)
                return macAddress;

            char[] chars = macAddress.ToCharArray();
            return $"{chars[0]}{chars[1]}:{chars[2]}{chars[3]}:{chars[4]}{chars[5]}:" +
                   $"{chars[6]}{chars[7]}:{chars[8]}{chars[9]}:{chars[10]}{chars[11]}";
        }

        private static string GetInterfaceTypeString(NetworkInterfaceType type)
        {
            switch (type)
            {
                case NetworkInterfaceType.Ethernet: return "Ethernet";
                case NetworkInterfaceType.Wireless80211: return "Wireless";
                case NetworkInterfaceType.GigabitEthernet: return "GigabitEthernet";
                case NetworkInterfaceType.FastEthernetFx: return "FastEthernet";
                case NetworkInterfaceType.FastEthernetT: return "FastEthernet";
                case NetworkInterfaceType.TokenRing: return "TokenRing";
                case NetworkInterfaceType.Fddi: return "FDDI";
                case NetworkInterfaceType.BasicIsdn: return "ISDN";
                case NetworkInterfaceType.PrimaryIsdn: return "ISDN";
                case NetworkInterfaceType.Ppp: return "PPP";
                case NetworkInterfaceType.Loopback: return "Loopback";
                case NetworkInterfaceType.Tunnel: return "Tunnel";
                case NetworkInterfaceType.Unknown: return "Unknown";
                default: return type.ToString();
            }
        }

        private static string GetOperationalStatusString(OperationalStatus status)
        {
            switch (status)
            {
                case OperationalStatus.Up: return "Up";
                case OperationalStatus.Down: return "Down";
                case OperationalStatus.Testing: return "Testing";
                case OperationalStatus.Unknown: return "Unknown";
                case OperationalStatus.Dormant: return "Dormant";
                case OperationalStatus.NotPresent: return "NotPresent";
                case OperationalStatus.LowerLayerDown: return "LowerLayerDown";
                default: return status.ToString();
            }
        }

        private static string FormatSpeed(long speed)
        {
            if (speed <= 0) return "Desconocido";

            if (speed >= 1000000000)
                return $"{(speed / 1000000000.0):0.##} Gbps";
            else if (speed >= 1000000)
                return $"{(speed / 1000000.0):0.##} Mbps";
            else if (speed >= 1000)
                return $"{(speed / 1000.0):0.##} Kbps";
            else
                return $"{speed} bps";
        }
    }

    public static class GeolocationService
    {
        private static readonly string[] PublicIpServices =
        {
            "https://api.ipify.org",
            "https://icanhazip.com",
            "https://checkip.amazonaws.com",
            "https://ifconfig.me/ip",
            "http://checkip.dyndns.org"
        };

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        public static bool HasInternetAccess(int timeoutMs = 3000)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                    var response = client.GetAsync("http://www.google.com/generate_204").GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetPublicIP()
        {
            if (!HasInternetAccess())
                return null;

            foreach (var service in PublicIpServices)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = DefaultTimeout;
                        var response = client.GetStringAsync(service).GetAwaiter().GetResult();
                        var ip = CleanIPResponse(response);

                        if (IsValidIP(ip))
                            return ip;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        public static GeolocationInfo GetGeolocationInfo()
        {
            if (!HasInternetAccess())
                return new GeolocationInfo
                {
                    Status = "fail",
                    Message = "Sin acceso a internet"
                };

            try
            {
                var publicIp = GetPublicIP();

                if (string.IsNullOrEmpty(publicIp))
                    return new GeolocationInfo
                    {
                        Status = "fail",
                        Message = "No se pudo obtener IP publica"
                    };

                string apiUrl = $"http://ip-api.com/json/{publicIp}" +
                               "?fields=status,message,country,countryCode,region,regionName," +
                               "city,zip,lat,lon,timezone,isp,org,as,query";

                using (var client = new HttpClient())
                {
                    client.Timeout = DefaultTimeout;
                    var jsonResponse = client.GetStringAsync(apiUrl).GetAwaiter().GetResult();

                    // Deserialización simplificada - compatible con ofuscación
                    var result = JsonSerializer.Deserialize<GeolocationInfo>(jsonResponse,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (result != null)
                        result.PublicIP = publicIp;

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new GeolocationInfo
                {
                    Status = "fail",
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public static string GetFormattedLocationInfo(string localIP = null)
        {
            var geoInfo = GetGeolocationInfo();

            if (geoInfo == null || !geoInfo.Success)
                return $"IP Local: {localIP ?? "No disponible"}";

            return $@"IP Local: {localIP ?? "No disponible"}
IP Publica: {geoInfo.PublicIP}
Pais: {geoInfo.Country}
Ciudad: {geoInfo.City}
Region: {geoInfo.RegionName}
ISP: {geoInfo.Isp}
Coordenadas: {geoInfo.Lat}, {geoInfo.Lon}";
        }

        private static string CleanIPResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            return response.Trim()
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("Current IP Address: ", "")
                .Replace("<!-- Hosting24 -->", "");
        }

        private static bool IsValidIP(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return false;

            return IPAddress.TryParse(ip, out _);
        }



    }

    public static class PhysicalNetworkUsed
    {
        public static string GetCompleteNetworkInfo()
        {
            var result = new System.Text.StringBuilder();

            result.AppendLine("=== DETECTOR DE RED Y GEOLOCALIZACION ===");
            result.AppendLine();

            // 1. Información de la interfaz de red
            result.AppendLine("1. Detectando interfaz de red activa...");
            var activeInterface = NetworkInterfaceDetector.GetActiveInternetInterface();

            if (activeInterface != null)
            {
                result.AppendLine();
                result.AppendLine(activeInterface.ToString());

                result.AppendLine();
                result.AppendLine("Informacion detallada:");
                result.AppendLine($"ID: {activeInterface.Id}");
                result.AppendLine($"MAC (RAW): {activeInterface.RawMACAddress}");
                result.AppendLine($"Todas IPv4: {string.Join(", ", activeInterface.AllIPv4Addresses)}");
                result.AppendLine($"DNS Servers: {string.Join(", ", activeInterface.DnsServers)}");
                result.AppendLine($"Es interfaz fisica: {activeInterface.IsPhysical}");
                result.AppendLine($"Soporta multicast: {activeInterface.SupportsMulticast}");
            }
            else
            {
                result.AppendLine("No se encontro ninguna interfaz de internet activa.");
            }

            result.AppendLine();
            result.AppendLine(new string('-', 50));
            result.AppendLine();

            // 2. Información de geolocalización
            result.AppendLine("2. Obteniendo informacion de geolocalizacion...");
            result.AppendLine();

            if (GeolocationService.HasInternetAccess())
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();

                if (geoInfo.Success)
                {
                    result.AppendLine(geoInfo.ToString());

                    result.AppendLine();
                    result.AppendLine(new string('-', 30));
                    result.AppendLine("Formato especifico:");
                    result.AppendLine(GeolocationService.GetFormattedLocationInfo(
                        activeInterface?.IPv4Address));
                }
                else
                {
                    result.AppendLine($"Error: {geoInfo.Message}");
                }
            }
            else
            {
                result.AppendLine("No hay acceso a internet para obtener geolocalizacion.");
            }

            result.AppendLine();
            result.AppendLine(new string('=', 50));

            return result.ToString();
        }

        // Versión más simple sin títulos decorativos
        public static string GetSimpleNetworkInfo()
        {
            var result = new System.Text.StringBuilder();

            // Información de red
            var activeInterface = NetworkInterfaceDetector.GetActiveInternetInterface();

            if (activeInterface != null)
            {
                result.AppendLine("=== INFORMACION DE RED ===");
                result.AppendLine($"Interfaz: {activeInterface.Name}");
                result.AppendLine($"MAC: {activeInterface.MACAddress}");
                result.AppendLine($"IP Local: {activeInterface.IPv4Address}");
                result.AppendLine($"Mascara: {activeInterface.IPv4Mask}");
                result.AppendLine($"Gateway: {activeInterface.Gateway}");
                result.AppendLine($"Velocidad: {activeInterface.Speed}");
                result.AppendLine();
            }

            // Información de geolocalización
            if (GeolocationService.HasInternetAccess())
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();

                if (geoInfo.Success)
                {
                    result.AppendLine("=== GEOLOCALIZACION ===");
                    result.AppendLine($"IP Publica: {geoInfo.PublicIP}");
                    result.AppendLine($"Pais: {geoInfo.Country}");
                    result.AppendLine($"Region: {geoInfo.RegionName}");
                    result.AppendLine($"Ciudad: {geoInfo.City}");
                    result.AppendLine($"ISP: {geoInfo.Isp}");
                }
            }

            return result.ToString();
        }

        // Método solo para información de red (sin geolocalización)
        public static string GetNetworkInterfaceInfo()
        {
            var activeInterface = NetworkInterfaceDetector.GetActiveInternetInterface();

            if (activeInterface == null)
                return "No se encontro interfaz de red activa.";

            var result = new System.Text.StringBuilder();

            result.AppendLine("INFORMACION DE INTERFAZ DE RED");
            result.AppendLine("==============================");
            result.AppendLine($"Nombre: {activeInterface.Name}");
            result.AppendLine($"Descripcion: {activeInterface.Description}");
            result.AppendLine($"Estado: {activeInterface.Status}");
            result.AppendLine($"Tipo: {activeInterface.Type}");
            result.AppendLine($"MAC Address: {activeInterface.MACAddress}");
            result.AppendLine($"IP Local: {activeInterface.IPv4Address}");
            result.AppendLine($"Mascara de Red: {activeInterface.IPv4Mask}");
            result.AppendLine($"Gateway: {activeInterface.Gateway}");
            result.AppendLine($"Velocidad: {activeInterface.Speed}");
            result.AppendLine($"DNS: {string.Join(", ", activeInterface.DnsServers)}");

            return result.ToString();
        }

        // Método solo para geolocalización
        public static string GetGeolocationInfoText()
        {
            if (!GeolocationService.HasInternetAccess())
                return "Sin acceso a internet.";

            var geoInfo = GeolocationService.GetGeolocationInfo();

            if (!geoInfo.Success)
                return $"Error: {geoInfo.Message}";

            var result = new System.Text.StringBuilder();

            result.AppendLine("INFORMACION DE GEOLOCALIZACION");
            result.AppendLine("===============================");
            result.AppendLine($"IP Publica: {geoInfo.PublicIP}");
            result.AppendLine($"Pais: {geoInfo.Country} ({geoInfo.CountryCode})");
            result.AppendLine($"Region: {geoInfo.RegionName}");
            result.AppendLine($"Ciudad: {geoInfo.City}");
            result.AppendLine($"Coordenadas: {geoInfo.Lat}, {geoInfo.Lon}");
            result.AppendLine($"ISP: {geoInfo.Isp}");
            result.AppendLine($"Zona Horaria: {geoInfo.Timezone}");

            return result.ToString();
        }

        // Método que combina ambas en un formato compacto
        public static string GetCombinedInfoCompact()
        {
            var result = new System.Text.StringBuilder();

            // Red
            var activeInterface = NetworkInterfaceDetector.GetActiveInternetInterface();
            if (activeInterface != null)
            {
                result.AppendLine("RED:");
                result.AppendLine($"  MAC: {activeInterface.MACAddress}");
                result.AppendLine($"  IP: {activeInterface.IPv4Address}");
                result.AppendLine($"  Gateway: {activeInterface.Gateway}");
                result.AppendLine();
            }

            // Geolocalización
            if (GeolocationService.HasInternetAccess())
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();
                if (geoInfo.Success)
                {
                    result.AppendLine("UBICACION:");
                    result.AppendLine($"  IP Publica: {geoInfo.PublicIP}");
                    result.AppendLine($"  Ubicacion: {geoInfo.City}, {geoInfo.RegionName}");
                    result.AppendLine($"  Pais: {geoInfo.Country}");
                    result.AppendLine($"  ISP: {geoInfo.Isp}");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Obtiene solo la información de red como string formateado
        /// </summary>
        public static string GetNetworkInfo()
        {
            var result = new StringBuilder();
            var activeInterface = NetworkInterfaceDetector.GetActiveInternetInterface();

            if (activeInterface != null)
            {
                result.AppendLine($"IP Local: {activeInterface.IPv4Address}");
                result.AppendLine($"Gateway: {activeInterface.Gateway}");
                result.AppendLine($"Interfaz: {activeInterface.Name}");
                result.AppendLine($"MAC: {activeInterface.MACAddress}");
                result.AppendLine($"Mascara: {activeInterface.IPv4Mask}");
                result.AppendLine($"Velocidad: {activeInterface.Speed}");
            }
            else
            {
                result.AppendLine("No se pudo obtener información de red");
            }

            return result.ToString();
        }

        /// <summary>
        /// Obtiene solo la información de geolocalización como string formateado
        /// </summary>
        public static string GetGeolocationInfo()
        {
            var result = new StringBuilder();

            if (GeolocationService.HasInternetAccess())
            {
                var geoInfo = GeolocationService.GetGeolocationInfo();

                if (geoInfo.Success)
                {
                    result.AppendLine($"IP Publica: {geoInfo.PublicIP}");
                    result.AppendLine($"Pais: {geoInfo.Country}");
                    result.AppendLine($"Region: {geoInfo.RegionName}");
                    result.AppendLine($"Ciudad: {geoInfo.City}");
                    result.AppendLine($"ISP: {geoInfo.Isp}");
                }
                else
                {
                    result.AppendLine("No se pudo obtener la geolocalización");
                }
            }
            else
            {
                result.AppendLine("Sin conexión a internet - No se puede geolocalizar");
            }

            return result.ToString();
        }
    }
}
