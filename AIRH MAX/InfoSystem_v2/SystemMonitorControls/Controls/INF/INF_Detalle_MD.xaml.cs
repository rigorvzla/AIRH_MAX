using InfoSystem_v2.Services;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SystemMonitorControls.INF
{
    public partial class INF_Detalle_MD : UserControl
    {
        public INF_Detalle_MD()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadSystemInfoAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando información del sistema: {ex.Message}");
            }
        }

        private async Task LoadSystemInfoAsync()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    Carga();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en carga de información: {ex.Message}");
                }
            });
        }

        private void Carga()
        {
            try
            {
                LoadMotherboardInfo();
                LoadCpuInfo();
                LoadNetworkInfo();
                LoadStorageInfo();
                LoadMemoryInfo();
                LoadGpuInfo();
                LoadWindowsInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Carga: {ex.Message}");
            }
        }

        #region Métodos de Carga Específicos
        private void LoadMotherboardInfo()
        {
            var motherboard = DeviceInfoService.Motherboard();
            tvMotherBoard.Header = CreateHeaderPanel(PackIconKind.AlphaMBoxOutline, motherboard.Modelo);
        }

        private void LoadCpuInfo()
        {
            var cpu = DeviceInfoService.CPU();
            tvCPU.Header = CreateHeaderPanel(PackIconKind.Cpu64Bit, cpu.Modelo);

            tvCPU.Items.Clear();
            tvCPU.Items.Add($"Frecuencia: {cpu.Velocidad_Actual} MHz");
            tvCPU.Items.Add($"Frecuencia Máxima: {cpu.Velocidad_Maxima} MHz");
            tvCPU.Items.Add($"Núcleos: {cpu.Nucleos}");
            tvCPU.Items.Add($"Procesadores Lógicos: {cpu.Hilos}");
        }

        private void LoadNetworkInfo()
        {
            var network = MonitorService.RED();
            tvRED.Header = CreateHeaderPanel(PackIconKind.Lan, network.Dispositivo);

            tvRED.Items.Clear();
            tvRED.Items.Add($"Tipo: {network.NombreRed}");
        }

        private void LoadStorageInfo()
        {
            var storageDevices = DeviceInfoService.Storage();
            var primaryStorage = storageDevices.FirstOrDefault(item => item.Principal);

            if (primaryStorage != null)
            {
                tvHDD.Header = CreateHeaderPanel(PackIconKind.Hdd, primaryStorage.Modelo);
            }

            tvHDD.Items.Clear();
            foreach (var storage in storageDevices)
            {
                tvHDD.Items.Add($"{storage.Modelo} {storage.TamañoTotal} ({storage.Tipo})");
            }
        }

        private void LoadMemoryInfo()
        {
            var ramMonitor = MonitorService.RAM();
            var ramDevices = DeviceInfoService.RAM();

            // ✅ ENCONTRAR PRIMER DISPOSITIVO RAM VÁLIDO
            var validRam = ramDevices.FirstOrDefault(item =>
                !item.Tipo.Equals("Unknown", StringComparison.OrdinalIgnoreCase));

            string ramType = validRam?.Tipo ?? "Desconocido";
            string ramSpeed = validRam?.Velocidad ?? "N/A";
            string ramTotal = validRam != null ?
                NormalizeFileSize(validRam.RAM_Total) :
                NormalizeFileSize(ramMonitor.Total);

            tvRAM.Header = CreateHeaderPanel(PackIconKind.Memory, ramTotal);

            tvRAM.Items.Clear();
            tvRAM.Items.Add($"Tipo: {ramType}");
            tvRAM.Items.Add($"Frecuencia: {ramSpeed} MHz");
        }

        private void LoadGpuInfo()
        {
            var gpu = DeviceInfoService.GPU();
            tvGPU.Header = CreateHeaderPanel(PackIconKind.Gpu, gpu.Nombre);

            tvGPU.Items.Clear();
            tvGPU.Items.Add($"VRAM (Integrada): {gpu.MemoriaIntegrada}");

            if (gpu.MemoriaTotal.HasValue)
            {
                tvGPU.Items.Add($"VRAM (Física): {NormalizeFileSize(gpu.MemoriaTotal.Value)}");
            }

            try
            {
                var resolution = SystemInfoService.DirectX_Value("Current Mode");
                var directXVersion = SystemInfoService.DirectX_Value("DirectX Version");

                tvGPU.Items.Add($"Resolución: {resolution}");

                if (!string.IsNullOrEmpty(directXVersion))
                {
                    var dxVersion = directXVersion.Split(' ').Length > 1 ?
                        directXVersion.Split(' ')[1] : directXVersion;
                    tvGPU.Items.Add($"DirectX: {dxVersion}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo información DirectX: {ex.Message}");
                tvGPU.Items.Add("DirectX: No disponible");
            }
        }

        private void LoadWindowsInfo()
        {
            var windows = SystemInfoService.Windows();
            txbDRLite.Text = $"{windows.Modelo}{Environment.NewLine}" +
                            $"{windows.Version} ({windows.NumCompilacion}) {windows.Arquitectura}";
        }
        #endregion

        #region Métodos de Utilidad
        private static StackPanel CreateHeaderPanel(PackIconKind iconKind, string text)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new PackIcon { Kind = iconKind, Margin = new Thickness(0, 0, 5, 0) },
                    new TextBlock {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 200
                    }
                }
            };
        }

        private static string NormalizeFileSize(double fileSize)
        {
            if (fileSize <= 0) return "0 B";

            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.#} {units[unit]}";
        }
        #endregion
    }
}