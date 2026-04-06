using InfoSystem_v2.Helpers;
using InfoSystem_v2.Models;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;
using System.Net.NetworkInformation;

namespace InfoSystem_v2.Services
{
    /// <summary>
    /// Obtiene informacion completa del dispositivo con optimizaciones de rendimiento
    /// </summary>
    public class DeviceInfoService : IDisposable
    {
        #region Cache y Estado
        // ✅ CACHE PARA DATOS ESTÁTICOS
        private static readonly Lazy<Dictionary<string, string>> _cachedDiskTypes =
            new Lazy<Dictionary<string, string>>(() => HardwareHelper.HDD.TipoDiscoDuro());

        private static readonly Lazy<string> _cachedOSRoot =
            new Lazy<string>(() => HardwareHelper.HDD.ParticionPrincipal());

        // ✅ CONFIGURACIÓN DE INTERVALOS DE ACTUALIZACIÓN
        private static readonly Dictionary<HardwareType, TimeSpan> _updateIntervals = new()
        {
            { HardwareType.Network, TimeSpan.FromMilliseconds(500) },    // 2 veces por segundo
            { HardwareType.Cpu, TimeSpan.FromMilliseconds(1000) },       // 1 vez por segundo
            { HardwareType.GpuNvidia, TimeSpan.FromMilliseconds(2000) }, // 0.5 veces por segundo
            { HardwareType.GpuAmd, TimeSpan.FromMilliseconds(2000) },
            { HardwareType.GpuIntel, TimeSpan.FromMilliseconds(2000) },
            { HardwareType.Storage, TimeSpan.FromSeconds(5) },           // 0.2 veces por segundo
        };

        // ✅ CACHE DE ÚLTIMAS ACTUALIZACIONES
        private static readonly Dictionary<HardwareType, DateTime> _lastHardwareUpdates = new();

        private bool _disposed = false;
        #endregion

        #region Métodos Públicos
        public static List<NET> NET()
        {
            var redes = new List<NET>();

            // ✅ USAR EL NUEVO HardwareMonitorEngine INSTANCE
            if (!HardwareMonitorEngine.Instance.IsEnabled)
                return redes;

            // ✅ OBTENER TODOS LOS HARDWARE Y FILTRAR
            var allHardware = HardwareMonitorEngine.Instance.GetAllHardware();
            var networkHardware = allHardware.Where(h => h.HardwareType == HardwareType.Network);

            foreach (var hardware in networkHardware)
            {
                // ✅ ACTUALIZAR SOLO SI ES NECESARIO
                if (ShouldUpdateHardware(hardware))
                {
                    HardwareMonitorEngine.Instance.UpdateSpecificHardware(HardwareType.Network);
                    _lastHardwareUpdates[hardware.HardwareType] = DateTime.Now;
                }

                var modelNet = new NET();
                modelNet.NombreRed = hardware.Name;

                // ✅ OBTENER INTERFAZ DE RED (optimizado)
                var interfaceRed = HardwareHelper.NET.NetworkInterfaceAdapter(hardware.Name);
                if (interfaceRed != null)
                {
                    var properties = interfaceRed.GetIPProperties();

                    // ✅ BUSCAR IP IPv4 MÁS EFICIENTEMENTE
                    var ipv4Address = properties.UnicastAddresses
                        .FirstOrDefault(unicast =>
                            unicast.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    modelNet.IP = ipv4Address?.Address.ToString() ?? "";
                    modelNet.MAC = interfaceRed.GetPhysicalAddress().ToString();
                    modelNet.Dispositivo = interfaceRed.Description ?? "";
                    modelNet.TipoInterface = interfaceRed.NetworkInterfaceType.ToString();
                    modelNet.VelocidadDispositivo = interfaceRed.Speed.ToString();
                    modelNet.Estado = interfaceRed.OperationalStatus.ToString();
                }

                // ✅ PROCESAR SENSORES UNA SOLA VEZ
                ProcessNetworkSensors(hardware, modelNet);
                redes.Add(modelNet);
            }

            return redes;
        }

        public static GPU GPU()
        {
            var gpu = new GPU();

            // ✅ USAR EL NUEVO HardwareMonitorEngine INSTANCE
            if (!HardwareMonitorEngine.Instance.IsEnabled)
                return gpu;

            // ✅ OBTENER TODOS LOS HARDWARE Y FILTRAR GPUS
            var allHardware = HardwareMonitorEngine.Instance.GetAllHardware();
            var gpuHardwareList = allHardware.Where(h =>
                h.HardwareType == HardwareType.GpuNvidia ||
                h.HardwareType == HardwareType.GpuAmd ||
                h.HardwareType == HardwareType.GpuIntel);

            bool gpuDedicadaEncontrada = false;

            foreach (var hardware in gpuHardwareList)
            {
                // ✅ ACTUALIZAR SOLO SI ES NECESARIO
                if (ShouldUpdateHardware(hardware))
                {
                    HardwareMonitorEngine.Instance.UpdateSpecificHardware(hardware.HardwareType);
                    _lastHardwareUpdates[hardware.HardwareType] = DateTime.Now;
                }

                if (!gpuDedicadaEncontrada &&
                    (hardware.HardwareType == HardwareType.GpuNvidia ||
                     hardware.HardwareType == HardwareType.GpuAmd))
                {
                    // ✅ GPU DEDICADA (prioridad)
                    gpu.Nombre = hardware.Name;
                    gpu.Fisico = true;
                    gpu.MemoriaIntegrada = HardwareHelper.GPU.VideoRAM;
                    gpu.Caption = HardwareHelper.GPU.VideoSubNombre;
                    gpu.NameAlter = HardwareHelper.GPU.VideoNombre;

                    ProcessDedicatedGpuSensors(hardware, gpu);
                    gpuDedicadaEncontrada = true;
                }
                else if (!gpuDedicadaEncontrada && hardware.HardwareType == HardwareType.GpuIntel)
                {
                    // ✅ GPU INTEGRADA (solo si no hay dedicada)
                    gpu.Nombre = hardware.Name;
                    gpu.Fisico = false;
                    gpu.MemoriaIntegrada = HardwareHelper.GPU.VideoRAM;
                    ProcessIntegratedGpuSensors(hardware, gpu);
                }
            }
            return gpu;
        }

        public static List<Storage> Storage()
        {
            var storages = new List<Storage>();

            // ✅ USAR EL NUEVO HardwareMonitorEngine INSTANCE
            if (!HardwareMonitorEngine.Instance.IsEnabled)
                return storages;

            // ✅ USAR CACHE PARA DATOS ESTÁTICOS
            var diskTypes = _cachedDiskTypes.Value;
            var osRoot = _cachedOSRoot.Value;

            // ✅ OBTENER TODOS LOS HARDWARE Y FILTRAR ALMACENAMIENTO
            var allHardware = HardwareMonitorEngine.Instance.GetAllHardware();
            var storageHardwareList = allHardware.Where(h => h.HardwareType == HardwareType.Storage);

            foreach (var hardware in storageHardwareList)
            {
                // ✅ ACTUALIZAR SOLO SI ES NECESARIO (cada 5 segundos)
                if (ShouldUpdateHardware(hardware))
                {
                    HardwareMonitorEngine.Instance.UpdateSpecificHardware(HardwareType.Storage);
                    _lastHardwareUpdates[hardware.HardwareType] = DateTime.Now;
                }

                if (hardware is AbstractStorage storageHardware && storageHardware.DriveInfos.Length > 0)
                {
                    var driveInfo = storageHardware.DriveInfos[0];

                    // ✅ CALCULAR ESPACIOS
                    var usedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace;

                    // ✅ BUSCAR SENSOR DE TEMPERATURA MÁS EFICIENTEMENTE
                    var temperatureSensor = hardware.Sensors
                        .FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Value.HasValue);

                    var storage = new Storage
                    {
                        Temperatura = temperatureSensor?.Value != null ? (float?)temperatureSensor.Value.Value : null,
                        EspacioLibre = CommonHelper.NormalizeFileSize(driveInfo.TotalFreeSpace),
                        EspacioUsado = CommonHelper.NormalizeFileSize(usedSpace),
                        Formato = driveInfo.DriveFormat,
                        TamañoTotal = CommonHelper.NormalizeFileSize(driveInfo.TotalSize),
                        Unidad = driveInfo.Name,
                        Fijado = driveInfo.DriveType.ToString(),
                        Modelo = hardware.Name,
                        OSRoot = osRoot,
                        Etiqueta = driveInfo.VolumeLabel,
                        Tipo = diskTypes.GetValueOrDefault(hardware.Name, "Unspecified"),
                        Principal = driveInfo.Name.StartsWith(osRoot, StringComparison.OrdinalIgnoreCase)
                    };

                    storages.Add(storage);
                }
            }

            return storages;
        }

        public static CPU CPU()
        {
            var cpuInfo = new CPU();
            var bios = new SMBios();

            // ✅ OBTENER INFORMACIÓN DEL BIOS (usar el primer procesador)
            var firstProcessor = bios.Processors.FirstOrDefault();
            if (firstProcessor != null)
            {
                cpuInfo.Velocidad_Maxima = firstProcessor.MaxSpeed.ToString();
                cpuInfo.Velocidad_Actual = firstProcessor.CurrentSpeed.ToString();
                cpuInfo.Familia = firstProcessor.Family.ToString();
                cpuInfo.Nucleos = firstProcessor.CoreCount.ToString();
                cpuInfo.Socket = firstProcessor.Socket.ToString();
                cpuInfo.Hilos = firstProcessor.ThreadCount.ToString();
            }

            // ✅ USAR EL NUEVO HardwareMonitorEngine INSTANCE
            if (!HardwareMonitorEngine.Instance.IsEnabled)
                return cpuInfo;

            // ✅ OBTENER INFORMACIÓN DEL HARDWARE
            var cpuHardware = HardwareMonitorEngine.Instance.GetHardwareByType(HardwareType.Cpu);
            if (cpuHardware != null)
            {
                // ✅ ACTUALIZAR SOLO SI ES NECESARIO
                if (ShouldUpdateHardware(cpuHardware))
                {
                    HardwareMonitorEngine.Instance.UpdateSpecificHardware(HardwareType.Cpu);
                    _lastHardwareUpdates[cpuHardware.HardwareType] = DateTime.Now;
                }

                cpuInfo.Modelo = cpuHardware.Name;

                // ✅ PROCESAR SENSORES CON LINQ MÁS EFICIENTE
                ProcessCpuSensors(cpuHardware, cpuInfo);
            }

            return cpuInfo;
        }

        public static List<RAM> RAM()
        {
            var ramList = new List<RAM>();
            var bios = new SMBios();

            // ✅ OBTENER CAPACIDAD MÁXIMA UNA SOLA VEZ
            var maxRam = HardwareHelper.RAM.Maxima();

            foreach (var memoryDevice in bios.MemoryDevices)
            {
                // ✅ SOLO AGREGAR MÓDULOS VÁLIDOS
                if (memoryDevice.Size > 0)
                {
                    var ram = new RAM
                    {
                        Velocidad = memoryDevice.Speed.ToString(),
                        Tipo = memoryDevice.Type.ToString(),
                        SizeModulo = CommonHelper.NormalizeFileSize(memoryDevice.Size * 1024L).Replace("MB", "GB"),
                        Posicion = memoryDevice.BankLocator,
                        RAM_Total = maxRam
                    };
                    ramList.Add(ram);
                }
            }

            return ramList;
        }

        public static MotherBoard Motherboard()
        {
            var bios = new SMBios();
            return new MotherBoard
            {
                Manufacturer = bios.Board.ManufacturerName ?? "Desconocido",
                Modelo = bios.Board.ProductName ?? "Desconocido"
            };
        }
        #endregion

        #region Métodos Auxiliares Privados
        // ✅ MÉTODO AUXILIAR PARA PROCESAR SENSORES DE RED
        private static void ProcessNetworkSensors(IHardware hardware, NET networkModel)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;

                // ✅ SWITCH MÁS EFICIENTE QUE MÚLTIPLES IF-ELSE
                switch (sensor.SensorType)
                {
                    case SensorType.Data:
                        switch (sensor.Name)
                        {
                            case "Data Uploaded":
                                networkModel.DatosSubidos = (float?)sensor.Value.Value;
                                break;
                            case "Data Downloaded":
                                networkModel.DatosDescargados = (float?)sensor.Value.Value;
                                break;
                        }
                        break;
                    case SensorType.Throughput:
                        switch (sensor.Name)
                        {
                            case "Download Speed":
                                networkModel.VelocidadDescarga = (float?)sensor.Value.Value;
                                break;
                            case "Upload Speed":
                                networkModel.VelocidadSubida = (float?)sensor.Value.Value;
                                break;
                            case "Network Utilization":
                                networkModel.CargaPorcentual = (float?)sensor.Value.Value;
                                break;
                        }
                        break;
                }
            }
        }

        // ✅ MÉTODO AUXILIAR PARA GPU DEDICADA
        private static void ProcessDedicatedGpuSensors(IHardware hardware, GPU gpu)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;

                switch (sensor.SensorType)
                {
                    case SensorType.Temperature when sensor.Name == "GPU Core":
                    case SensorType.Factor when sensor.Name == "GPU Core":
                        gpu.Temperatura = (float?)sensor.Value.Value;
                        break;
                    case SensorType.Clock:
                        // ✅ DISTINGUIR ENTRE CLOCK CORE Y MEMORY
                        if (sensor.Name.Contains("Core") || sensor.Name.Contains("GPU"))
                            gpu.ClockCore = (float?)sensor.Value.Value;
                        else if (sensor.Name.Contains("Memory"))
                            gpu.ClockMemory = (float?)sensor.Value.Value;
                        break;
                    case SensorType.SmallData:
                        ProcessGpuMemorySensor(sensor, gpu);
                        break;
                }
            }
        }

        // ✅ MÉTODO AUXILIAR PARA GPU INTEGRADA
        private static void ProcessIntegratedGpuSensors(IHardware hardware, GPU gpu)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.SmallData &&
                    hardware.Identifier.ToString().Contains("integrated"))
                {
                    gpu.Detalle = sensor.Name;
                    gpu.MemoriaUsada = (float?)sensor.Max;
                    break; // ✅ BREAK TEMPRANO
                }
            }
        }

        // ✅ SOLUCIÓN: Conversión explícita a float?
        private static void ProcessGpuMemorySensor(ISensor sensor, GPU gpu)
        {
            const double conversionFactor = 1000000;

            if (!sensor.Value.HasValue) return;

            switch (sensor.Name)
            {
                case "GPU Memory Total":
                    gpu.MemoriaTotal = (float?)(sensor.Value.Value * conversionFactor);
                    break;
                case "GPU Memory Free":
                    gpu.MemoriaLibre = (float?)(sensor.Value.Value * conversionFactor);
                    break;
                case "GPU Memory Used":
                    gpu.MemoriaUsada = (float?)(sensor.Value.Value * conversionFactor);
                    break;
            }
        }

        // ✅ MÉTODO AUXILIAR PARA PROCESAR SENSORES CPU
        private static void ProcessCpuSensors(IHardware cpuHardware, CPU cpuInfo)
        {
            // ✅ OBTENER SENSORES CON VALORES UNA SOLA VEZ
            var sensors = cpuHardware.Sensors.Where(s => s.Value.HasValue).ToList();

            // ✅ TEMPERATURAS
            foreach (var sensor in sensors.Where(s => s.SensorType == SensorType.Temperature))
            {
                cpuInfo.Temperatura[sensor.Name] = (float?)sensor.Value.Value;
            }

            // ✅ CARGAS
            foreach (var sensor in sensors.Where(s => s.SensorType == SensorType.Load))
            {
                cpuInfo.Hilos_Carga[sensor.Name] = (float?)sensor.Value.Value;

                // ✅ ASIGNAR CARGA GENERAL
                if (sensor.Name.Equals("CPU Total", StringComparison.OrdinalIgnoreCase))
                {
                    cpuInfo.CargaGeneral = (float?)sensor.Value.Value;
                }
            }

            // ✅ SI NO HAY CARGA GENERAL, CALCULAR PROMEDIO
            if (!cpuInfo.CargaGeneral.HasValue && cpuInfo.Hilos_Carga.Count > 0)
            {
                cpuInfo.CargaGeneral = (float?)cpuInfo.Hilos_Carga.Values.Average();
            }
        }

        // ✅ MÉTODO PARA CONTROLAR ACTUALIZACIONES
        private static bool ShouldUpdateHardware(IHardware hardware)
        {
            if (hardware == null) return false;

            if (!_lastHardwareUpdates.TryGetValue(hardware.HardwareType, out var lastUpdate))
                return true;

            if (!_updateIntervals.TryGetValue(hardware.HardwareType, out var interval))
                return true;

            return DateTime.Now - lastUpdate > interval;
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                _lastHardwareUpdates.Clear();
                _disposed = true;
            }
        }
        #endregion
    }
}