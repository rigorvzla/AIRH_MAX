using LibreHardwareMonitor.Hardware;
using System.Diagnostics;

namespace InfoSystem_v2.Services
{
    public class HardwareMonitorEngine : IDisposable
    {
        #region Singleton Pattern
        private static readonly Lazy<HardwareMonitorEngine> _instance =
            new Lazy<HardwareMonitorEngine>(() => new HardwareMonitorEngine());

        public static HardwareMonitorEngine Instance => _instance.Value;
        #endregion

        #region Visitor Pattern Optimizado
        private class OptimizedUpdateVisitor : IVisitor
        {
            private readonly TimeSpan _minUpdateInterval;
            private readonly Dictionary<IHardware, DateTime> _lastUpdates = new();
            private readonly object _lockObject = new object();

            public OptimizedUpdateVisitor(TimeSpan minUpdateInterval)
            {
                _minUpdateInterval = minUpdateInterval;
            }

            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                // ✅ CONTROL DE FRECUENCIA DE ACTUALIZACIÓN
                if (ShouldUpdateHardware(hardware))
                {
                    lock (_lockObject)
                    {
                        if (ShouldUpdateHardware(hardware)) // Doble verificación thread-safe
                        {
                            hardware.Update();
                            _lastUpdates[hardware] = DateTime.Now;
                        }
                    }
                }

                // ✅ ACTUALIZAR SUBHARDWARE SOLO SI ES NECESARIO
                foreach (var subHardware in hardware.SubHardware)
                {
                    if (ShouldUpdateHardware(subHardware))
                    {
                        subHardware.Accept(this);
                    }
                }
            }

            private bool ShouldUpdateHardware(IHardware hardware)
            {
                if (!_lastUpdates.TryGetValue(hardware, out var lastUpdate))
                    return true;

                return DateTime.Now - lastUpdate >= _minUpdateInterval;
            }

            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        #endregion

        #region Campos y Propiedades
        public Computer Computer { get; private set; }
        private OptimizedUpdateVisitor _updateVisitor;
        private bool _isDisposed = false;
        private bool _isOpen = false;
        private readonly object _lockObject = new object();

        // ✅ CONFIGURACIÓN PERSONALIZABLE
        public TimeSpan MinimumUpdateInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public bool IsEnabled { get; private set; }
        #endregion

        #region Constructor y Inicialización
        private HardwareMonitorEngine()
        {
            InitializeComputer();
        }

        private void InitializeComputer()
        {
            Computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsPsuEnabled = false // ✅ DESHABILITAR COMPONENTES NO NECESARIOS
            };

            _updateVisitor = new OptimizedUpdateVisitor(MinimumUpdateInterval);
        }
        #endregion

        #region Métodos Públicos
        public void MonitorOpen()
        {
            lock (_lockObject)
            {
                if (_isOpen || _isDisposed)
                    return;

                try
                {
                    Computer.Open();
                    _isOpen = true;
                    IsEnabled = true;

                    // ✅ ACTUALIZACIÓN INICIAL
                    Computer.Accept(_updateVisitor);

                    Debug.WriteLine("HardwareMonitorEngine iniciado correctamente");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al iniciar HardwareMonitorEngine: {ex.Message}");
                    _isOpen = false;
                    IsEnabled = false;
                }
            }
        }

        public void MonitorClose()
        {
            lock (_lockObject)
            {
                if (!_isOpen || _isDisposed)
                    return;

                try
                {
                    Computer.Close();
                    _isOpen = false;
                    IsEnabled = false;
                    Debug.WriteLine("HardwareMonitorEngine cerrado correctamente");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al cerrar HardwareMonitorEngine: {ex.Message}");
                }
            }
        }

        public void UpdateAllHardware()
        {
            if (!_isOpen || _isDisposed)
                return;

            lock (_lockObject)
            {
                try
                {
                    Computer.Accept(_updateVisitor);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateAllHardware: {ex.Message}");
                }
            }
        }

        public void UpdateSpecificHardware(HardwareType hardwareType)
        {
            if (!_isOpen || _isDisposed)
                return;

            lock (_lockObject)
            {
                try
                {
                    var specificHardware = Computer.Hardware
                        .Where(h => h.HardwareType == hardwareType)
                        .ToList();

                    foreach (var hardware in specificHardware)
                    {
                        hardware.Accept(_updateVisitor);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en UpdateSpecificHardware: {ex.Message}");
                }
            }
        }

        public IHardware GetHardwareByType(HardwareType hardwareType)
        {
            if (!_isOpen || _isDisposed)
                return null;

            lock (_lockObject)
            {
                return Computer.Hardware
                    .FirstOrDefault(h => h.HardwareType == hardwareType);
            }
        }

        public IEnumerable<IHardware> GetAllHardware()
        {
            if (!_isOpen || _isDisposed)
                return Enumerable.Empty<IHardware>();

            lock (_lockObject)
            {
                return Computer.Hardware.ToList(); // ✅ COPIA PARA EVITAR MODIFICACIONES
            }
        }
        #endregion

        #region Configuración Dinámica
        public void EnableHardwareType(HardwareType hardwareType, bool enable)
        {
            lock (_lockObject)
            {
                if (_isOpen)
                {
                    Debug.WriteLine($"No se puede cambiar la configuración con el monitor abierto");
                    return;
                }

                // ✅ CONFIGURACIÓN DINÁMICA BASADA EN EL TIPO
                switch (hardwareType)
                {
                    case HardwareType.Cpu:
                        Computer.IsCpuEnabled = enable;
                        break;
                    case HardwareType.Memory:
                        Computer.IsMemoryEnabled = enable;
                        break;
                    case HardwareType.Storage:
                        Computer.IsStorageEnabled = enable;
                        break;
                    case HardwareType.Network:
                        Computer.IsNetworkEnabled = enable;
                        break;
                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        Computer.IsGpuEnabled = enable;
                        break;
                    case HardwareType.Motherboard:
                        Computer.IsMotherboardEnabled = enable;
                        break;
                }
            }
        }

        public void SetUpdateInterval(TimeSpan interval)
        {
            lock (_lockObject)
            {
                MinimumUpdateInterval = interval;
                _updateVisitor = new OptimizedUpdateVisitor(interval);
            }
        }
        #endregion

        #region Disposable Pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    MonitorClose();
                    Computer?.Close();
                }

                _isDisposed = true;
            }
        }

        ~HardwareMonitorEngine()
        {
            Dispose(false);
        }
        #endregion

        #region Métodos de Utilidad
        public string GetStatus()
        {
            return _isOpen ? "Ejecutándose" : "Detenido";
        }

        public int GetHardwareCount()
        {
            if (!_isOpen || _isDisposed)
                return 0;

            lock (_lockObject)
            {
                return Computer.Hardware.Count();
            }
        }

        public IEnumerable<HardwareType> GetEnabledHardwareTypes()
        {
            var types = new List<HardwareType>();

            if (Computer.IsCpuEnabled) types.Add(HardwareType.Cpu);
            if (Computer.IsMemoryEnabled) types.Add(HardwareType.Memory);
            if (Computer.IsStorageEnabled) types.Add(HardwareType.Storage);
            if (Computer.IsNetworkEnabled) types.Add(HardwareType.Network);
            if (Computer.IsGpuEnabled)
            {
                types.Add(HardwareType.GpuNvidia);
                types.Add(HardwareType.GpuAmd);
                types.Add(HardwareType.GpuIntel);
            }
            if (Computer.IsMotherboardEnabled) types.Add(HardwareType.Motherboard);

            return types;
        }
        #endregion
    }
}