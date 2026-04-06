using InfoSystem_v2.Helpers;
using InfoSystem_v2.Models;
using LibreHardwareMonitor.Hardware;
using System.Diagnostics;

namespace InfoSystem_v2.Services
{
    /// <summary>
    /// Obtiene informacion puntual del dispositivo para su observación con optimizaciones de rendimiento
    /// </summary>
    public class MonitorService : IDisposable
    {
        #region Cache y Estado
        private static readonly Dictionary<Type, (object data, DateTime timestamp)> _monitorCache = new();
        private static readonly Dictionary<Type, TimeSpan> _cacheDurations = new()
        {
            { typeof(DeviceMonitor.CPU), TimeSpan.FromMilliseconds(500) },      // 2 veces/segundo
            { typeof(DeviceMonitor.RAM), TimeSpan.FromMilliseconds(1000) },     // 1 vez/segundo
            { typeof(DeviceMonitor.RED), TimeSpan.FromMilliseconds(250) },      // 4 veces/segundo
            { typeof(List<Storage>), TimeSpan.FromSeconds(2) },                // 0.5 veces/segundo
            { typeof(GPU), TimeSpan.FromMilliseconds(2000) }                   // 0.5 veces/segundo
        };

        private static SMBios _cachedBios;
        private static SMBios CachedBios => _cachedBios ??= new SMBios();

        private bool _disposed = false;
        #endregion

        #region CPU Monitor Optimizado
        public static DeviceMonitor.CPU CPU()
        {
            var cacheKey = typeof(DeviceMonitor.CPU);

            // ✅ VERIFICAR CACHE PRIMERO
            if (TryGetCachedValue(cacheKey, out DeviceMonitor.CPU cachedCpu))
                return cachedCpu;

            var cpuMonitor = new DeviceMonitor.CPU();

            try
            {
                // ✅ CARGAR DATOS BÁSICOS DEL CPU
                LoadBasicCpuInfo(cpuMonitor);

                // ✅ PROCESAR SENSORES DE HARDWARE
                ProcessCpuHardwareSensors(cpuMonitor);

                // ✅ CALCULAR TEMPERATURA GENERAL SI NO SE ENCONTRÓ
                CalculateFallbackTemperature(cpuMonitor);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en MonitorService.CPU: {ex.Message}");
            }

            // ✅ GUARDAR EN CACHE
            CacheValue(cacheKey, cpuMonitor);
            return cpuMonitor;
        }

        private static void LoadBasicCpuInfo(DeviceMonitor.CPU cpuMonitor)
        {
            // ✅ CARGAR PORCENTAJE DE USO
            cpuMonitor.CargaGeneral = HardwareHelper.CPU.CargaPorcentual;

            // ✅ OBTENER INFORMACIÓN DEL BIOS (USAR CACHE)
            var firstProcessor = CachedBios.Processors.FirstOrDefault();
            if (firstProcessor != null)
            {
                cpuMonitor.MHzTotal = firstProcessor.CurrentSpeed;
                cpuMonitor.MHzUsado = cpuMonitor.MHzTotal * cpuMonitor.CargaGeneral / 100;
            }
        }

        private static void ProcessCpuHardwareSensors(DeviceMonitor.CPU cpuMonitor)
        {
            // ✅ USAR EL NUEVO HardwareMonitorEngine
            var cpuHardware = HardwareMonitorEngine.Instance.GetHardwareByType(HardwareType.Cpu);
            if (cpuHardware == null) return;

            // ✅ ACTUALIZAR HARDWARE (CONTROLADO POR EL ENGINE)
            HardwareMonitorEngine.Instance.UpdateSpecificHardware(HardwareType.Cpu);

            // ✅ PROCESAR SENSORES DE UNA SOLA VEZ
            var sensors = cpuHardware.Sensors.Where(s => s.Value.HasValue).ToList();

            // ✅ TEMPERATURAS
            ProcessTemperatureSensors(sensors, cpuMonitor);

            // ✅ CARGAS
            ProcessLoadSensors(sensors, cpuMonitor);
        }

        private static void ProcessTemperatureSensors(List<ISensor> sensors, DeviceMonitor.CPU cpuMonitor)
        {
            var temperatureSensors = sensors.Where(s =>
                s.SensorType == SensorType.Temperature ||
                s.SensorType == SensorType.Factor);

            foreach (var sensor in temperatureSensors)
            {
                cpuMonitor.Temperaturas_Nucleos[sensor.Name] = (float?)sensor.Value.Value;

                // ✅ DETECTAR TEMPERATURA GENERAL AUTOMÁTICAMENTE
                if (sensor.Name.Equals("Core Average", StringComparison.OrdinalIgnoreCase) ||
                    sensor.Name.Equals("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase) ||
                    sensor.Name.Equals("CPU Package", StringComparison.OrdinalIgnoreCase))
                {
                    cpuMonitor.TemperaturaGeneral = Math.Round(sensor.Value.Value, 0);
                }
            }
        }

        private static void ProcessLoadSensors(List<ISensor> sensors, DeviceMonitor.CPU cpuMonitor)
        {
            var loadSensors = sensors.Where(s => s.SensorType == SensorType.Load);

            foreach (var sensor in loadSensors)
            {
                cpuMonitor.Hilos_Carga[sensor.Name] = (float?)sensor.Value.Value;
            }
        }

        private static void CalculateFallbackTemperature(DeviceMonitor.CPU cpuMonitor)
        {
            // ✅ CALCULAR TEMPERATURA GENERAL SI NO SE ENCONTRÓ
            if (cpuMonitor.TemperaturaGeneral == 0 && cpuMonitor.Temperaturas_Nucleos.Count > 0)
            {
                cpuMonitor.TemperaturaGeneral = Math.Round(
                    cpuMonitor.Temperaturas_Nucleos.Values.Average() ?? 0, 0);
            }
        }
        #endregion

        #region RAM Monitor Optimizado
        public static DeviceMonitor.RAM RAM()
        {
            var cacheKey = typeof(DeviceMonitor.RAM);

            // ✅ VERIFICAR CACHE
            if (TryGetCachedValue(cacheKey, out DeviceMonitor.RAM cachedRam))
                return cachedRam;

            var monitor = new DeviceMonitor.RAM();

            try
            {
                // ✅ CARGAR DATOS DE MEMORIA
                var actual = HardwareHelper.RAM.Actual();
                var total = HardwareHelper.RAM.Total();
                var disponible = HardwareHelper.RAM.Disponible();

                monitor.Actual = actual;
                monitor.Total = total;
                monitor.Dispobible = disponible;
                monitor.Porcentaje = total > 0 ? (actual / total * 100) : 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en MonitorService.RAM: {ex.Message}");
            }

            // ✅ GUARDAR EN CACHE
            CacheValue(cacheKey, monitor);
            return monitor;
        }
        #endregion

        #region Storage Monitor Optimizado
        public static List<Storage> HDD()
        {
            var cacheKey = typeof(List<Storage>);

            // ✅ VERIFICAR CACHE
            if (TryGetCachedValue(cacheKey, out List<Storage> cachedStorage))
                return cachedStorage;

            var storage = DeviceInfoService.Storage();

            // ✅ GUARDAR EN CACHE
            CacheValue(cacheKey, storage);
            return storage;
        }
        #endregion

        #region Network Monitor Optimizado
        public static DeviceMonitor.RED RED()
        {
            var cacheKey = typeof(DeviceMonitor.RED);

            // ✅ VERIFICAR CACHE (RED SE ACTUALIZA MÁS FRECUENTE)
            if (TryGetCachedValue(cacheKey, out DeviceMonitor.RED cachedRed))
                return cachedRed;

            var dev = new DeviceMonitor.RED();

            try
            {
                // ✅ BUSCAR PRIMERA RED ACTIVA
                var redes = DeviceInfoService.NET();
                var activeNetwork = redes.FirstOrDefault(device =>
                    device.VelocidadDescarga > 0 || device.VelocidadSubida > 0);

                if (activeNetwork != null)
                {
                    // ✅ COPIAR SOLO PROPIEDADES NECESARIAS
                    dev.VelocidadDescarga = activeNetwork.VelocidadDescarga;
                    dev.VelocidadSubida = activeNetwork.VelocidadSubida;
                    dev.CargaPorcentual = activeNetwork.CargaPorcentual;
                    dev.Estado = activeNetwork.Estado;
                    dev.NombreRed = activeNetwork.NombreRed;
                    dev.TipoInterface = activeNetwork.TipoInterface;
                    dev.Dispositivo = activeNetwork.Dispositivo;
                    dev.VelocidadDispositivo = activeNetwork.VelocidadDispositivo;
                    dev.DatosDescargados = activeNetwork.DatosDescargados;
                    dev.DatosSubidos = activeNetwork.DatosSubidos;
                    dev.MAC = activeNetwork.MAC;
                    dev.IP = HardwareHelper.NET.GetActiveIP(); // ✅ CACHE INTERNO EN HardwareHelper
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en MonitorService.RED: {ex.Message}");
            }

            // ✅ GUARDAR EN CACHE
            CacheValue(cacheKey, dev);
            return dev;
        }
        #endregion

        #region GPU Monitor Optimizado
        public static GPU GPU()
        {
            var cacheKey = typeof(GPU);

            // ✅ VERIFICAR CACHE
            if (TryGetCachedValue(cacheKey, out GPU cachedGpu))
                return cachedGpu;

            var gpu = DeviceInfoService.GPU();

            // ✅ GUARDAR EN CACHE
            CacheValue(cacheKey, gpu);
            return gpu;
        }
        #endregion

        #region Sistema de Cache
        private static bool TryGetCachedValue<T>(Type cacheKey, out T cachedValue)
        {
            cachedValue = default;

            if (_monitorCache.TryGetValue(cacheKey, out var cache) &&
                DateTime.Now - cache.timestamp < _cacheDurations[cacheKey])
            {
                cachedValue = (T)cache.data;
                return true;
            }

            return false;
        }

        private static void CacheValue<T>(Type cacheKey, T value)
        {
            _monitorCache[cacheKey] = (value, DateTime.Now);
        }

        public static void ClearCache()
        {
            _monitorCache.Clear();
        }

        public static void ClearCache(Type specificType)
        {
            _monitorCache.Remove(specificType);
        }
        #endregion

        #region Métodos de Utilidad
        public static Dictionary<Type, TimeSpan> GetCacheDurations()
        {
            return new Dictionary<Type, TimeSpan>(_cacheDurations);
        }

        public static void SetCacheDuration(Type monitorType, TimeSpan duration)
        {
            if (_cacheDurations.ContainsKey(monitorType))
            {
                _cacheDurations[monitorType] = duration;
            }
        }

        public static TimeSpan GetTimeSinceLastUpdate(Type monitorType)
        {
            if (_monitorCache.TryGetValue(monitorType, out var cache))
            {
                return DateTime.Now - cache.timestamp;
            }
            return TimeSpan.MaxValue;
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            if (!_disposed)
            {
                ClearCache();
                _cachedBios = null;
                _disposed = true;
            }
        }
        #endregion
    }
}