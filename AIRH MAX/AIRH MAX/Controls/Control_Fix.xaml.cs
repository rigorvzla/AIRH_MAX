using AIRH_MAX.ClassView;
using AIRH_MAX.Models;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using RadioButton = System.Windows.Controls.RadioButton;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_Fix : UserControl
    {
        public Control_Fix()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
            Process.GetCurrentProcess().MaxWorkingSet = Process.GetCurrentProcess().MinWorkingSet;

            listRadio.Children.Add(WatchNet());
            listRadio.Children.Add(IPReset());
            listRadio.Children.Add(Scandisk());
            listRadio.Children.Add(FileOrganizer());

            foreach (var item in Tools())
            {
                listRadio.Children.Add(item);
            }
        }

        private static Dictionary<string, string> Herramientas()
        {
            Dictionary<string, string> herramientas = new Dictionary<string, string>();
            herramientas.Add("Administrador de Dispositivo", "devmgmt.msc");
            herramientas.Add("Administrador de Tareas", Environment.SystemDirectory + "\\" + "taskmgr.exe");
            herramientas.Add("Servicios", "services.msc");
            herramientas.Add("MSConfig", Environment.SystemDirectory + "\\" + "msconfig.exe");
            herramientas.Add("Voz", Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\Speech\SpeechUX\" + "sapi.cpl");
            herramientas.Add("Simbolo de Sistema", "cmd");
            herramientas.Add("Editor de Registro", "regedit");
            herramientas.Add("Panel de Control", Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\control.exe");
            herramientas.Add("Administrador de Usuarios", Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\netplwiz.exe");
            herramientas.Add("Ejecutar", "shell:::{2559a1f3-21d7-11d4-bdaf-00c04f60b9f0}");
            herramientas.Add("Programas y Caracteristicas", "shell:::{7b81be6a-ce2b-4676-a29e-eb907a5126c5}");
            herramientas.Add("Todos los ajustes de Windows", "shell:::{ED7BA470-8E54-465E-825C-99712043E01C}");
            herramientas.Add("Winver", "winver.exe");
            herramientas.Add("DirectX", "dxdiag.exe");
            herramientas.Add("Opciones de Energia", "shell:::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}");
            herramientas.Add("Modo Noche", "ms-settings:nightlight");
            herramientas.Add("Configuracion del adaptador Red", "ms-settings:network-status");
            herramientas.Add("Opciones de Carpetas", "shell:::{6DFD7C5C-2451-11d3-A299-00C04F8EF6AF}");
            herramientas.Add("Ethernet", "ms-settings:network-ethernet");
            herramientas.Add("Mobile Hotspot", "ms-settings:network-mobilehotspot");
            herramientas.Add("Alto Contraste", "ms-settings:easeofaccess-highcontrast");
            herramientas.Add("Bluetooth", "ms-settings:bluetooth");
            herramientas.Add("Apps Segundo Plano", "ms-settings:privacy-backgroundapps");
            herramientas.Add("Multitarea", "ms-settings:multitasking");
            herramientas.Add("Escritorio Remoto", "ms-settings:remotedesktop");
            return herramientas;
        }

        private static PackIconKind GetIconForTool(string toolName)
        {
            return toolName switch
            {
                "Administrador de Dispositivo" => PackIconKind.Devices,
                "Administrador de Tareas" => PackIconKind.Timer,
                "Servicios" => PackIconKind.ServiceToolbox,
                "MSConfig" => PackIconKind.Settings,
                "Voz" => PackIconKind.Microphone,
                "Simbolo de Sistema" => PackIconKind.Console,
                "Editor de Registro" => PackIconKind.Register,
                "Panel de Control" => PackIconKind.ControlPoint,
                "Administrador de Usuarios" => PackIconKind.Account,
                "Ejecutar" => PackIconKind.Run,
                "Programas y Caracteristicas" => PackIconKind.Application,
                "Todos los ajustes de Windows" => PackIconKind.Tune,
                "Winver" => PackIconKind.MicrosoftWindows,
                "DirectX" => PackIconKind.Gamepad,
                "Opciones de Energia" => PackIconKind.Battery,
                "Modo Noche" => PackIconKind.WeatherNight,
                "Configuracion del adaptador Red" => PackIconKind.Network,
                "Opciones de Carpetas" => PackIconKind.Folder,
                "Ethernet" => PackIconKind.Lan,
                "Mobile Hotspot" => PackIconKind.Wifi,
                "Alto Contraste" => PackIconKind.Contrast,
                "Bluetooth" => PackIconKind.Bluetooth,
                "Apps Segundo Plano" => PackIconKind.Apps,
                "Multitarea" => PackIconKind.TaskAuto,
                "Escritorio Remoto" => PackIconKind.RemoteDesktop,
                "Escaner de Red" => PackIconKind.Network,
                "Reparar IP" => PackIconKind.IpNetwork,
                "Reparar Discos" => PackIconKind.Harddisk,
                "Organizador de Archivos" => PackIconKind.FolderOpen,
                _ => PackIconKind.Toolbox
            };
        }

        private static RadioButton CreateToolCard(string title, string action, PackIconKind iconKind)
        {
            RadioButton card = new RadioButton()
            {
                Style = (Style)Application.Current.Resources["ToolCardStyle"],
                Tag = action
            };

            // Contenedor principal centrado
            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.Margin= new Thickness(10,10,10,10);
            container.Width = 100;

            // Icono centrado
            PackIcon icon = new PackIcon()
            {
                Kind = iconKind,
                Width = 36,
                Height = 36,
                Foreground = (Brush)Application.Current.Resources["PrimaryText"],
                HorizontalAlignment =  System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(icon, 0);

            // Texto centrado
            TextBlock textBlock = new TextBlock()
            {
                Text = title,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.AliceBlue,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(textBlock, 1);

            container.Children.Add(icon);
            container.Children.Add(textBlock);
            card.Content = container;

            // Eventos visuales
            card.MouseLeave += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = Brushes.Transparent;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            };

            card.MouseEnter += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(129, 212, 250));
            };

            card.Click += delegate (object sender, RoutedEventArgs args)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = action,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Views.MainWindow.NotificacionEvent.MensajeBox = $"Error: {ex.Message}";
                }
            };

            return card;
        }

        private static RadioButton WatchNet()
        {
            RadioButton card = new RadioButton()
            {
                Style = (Style)Application.Current.Resources["ToolCardStyle"]
            };

            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            PackIcon icon = new PackIcon()
            {
                Kind = PackIconKind.Network,
                Width = 36,
                Height = 36,
                Foreground = (Brush)Application.Current.Resources["PrimaryText"],
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(icon, 0);

            TextBlock textBlock = new TextBlock()
            {
                Text = "Escaner de Red",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.AliceBlue,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(textBlock, 1);

            container.Children.Add(icon);
            container.Children.Add(textBlock);
            card.Content = container;

            card.MouseLeave += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = Brushes.Transparent;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            };

            card.MouseEnter += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(129, 212, 250));
            };

            card.Click += async delegate (object sender, RoutedEventArgs args)
            {
                Views.MainWindow.NotificacionEvent.MensajeBox = "Analizando red...";
                var devices = new List<NetworkDevice>();
                string jsonOutput = await Py.Script_EXE(Py.NetWatcher);
                devices = JsonSerializer.Deserialize<List<NetworkDevice>>(jsonOutput, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                List<string> devicesList = Support.FormatDevicesList(devices);
                Views.MainWindow.NotificacionEvent.MensajeBox = "Dispositivos detectados: " + devices.Count;

                foreach (var item in devicesList)
                {
                    Views.MainWindow.NotificacionEvent.MensajeBoxMute = item;
                }
            };

            return card;
        }

        private static RadioButton FileOrganizer()
        {
            RadioButton card = new RadioButton()
            {
                Style = (Style)Application.Current.Resources["ToolCardStyle"]
            };

            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            PackIcon icon = new PackIcon()
            {
                Kind = PackIconKind.FolderOpen,
                Width = 36,
                Height = 36,
                Foreground = (Brush)Application.Current.Resources["PrimaryText"],
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(icon, 0);

            TextBlock textBlock = new TextBlock()
            {
                Text = "Organizador de Archivos",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.AliceBlue,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(textBlock, 1);

            container.Children.Add(icon);
            container.Children.Add(textBlock);
            card.Content = container;

            card.MouseLeave += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = Brushes.Transparent;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            };

            card.MouseEnter += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(129, 212, 250));
            };

            card.Click += delegate (object sender, RoutedEventArgs args)
            {
                Popups.FileOrganizer fileOrganizer = new Popups.FileOrganizer();
                fileOrganizer.ShowDialog();
            };

            return card;
        }

        private static RadioButton IPReset()
        {
            RadioButton card = new RadioButton()
            {
                Style = (Style)Application.Current.Resources["ToolCardStyle"]
            };

            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            PackIcon icon = new PackIcon()
            {
                Kind = PackIconKind.IpNetwork,
                Width = 36,
                Height = 36,
                Foreground = (Brush)Application.Current.Resources["PrimaryText"],
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(icon, 0);

            TextBlock textBlock = new TextBlock()
            {
                Text = "Reparar IP",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.AliceBlue,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(textBlock, 1);

            container.Children.Add(icon);
            container.Children.Add(textBlock);
            card.Content = container;

            card.MouseLeave += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = Brushes.Transparent;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            };

            card.MouseEnter += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(129, 212, 250));
            };

            card.Click += delegate (object sender, RoutedEventArgs args)
            {
                ClassView.LineaComandos.IPReset();
            };

            return card;
        }

        private static RadioButton Scandisk()
        {
            RadioButton card = new RadioButton()
            {
                Style = (Style)Application.Current.Resources["ToolCardStyle"]
            };

            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            PackIcon icon = new PackIcon()
            {
                Kind = PackIconKind.Harddisk,
                Width = 36,
                Height = 36,
                Foreground = (Brush)Application.Current.Resources["PrimaryText"],
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(icon, 0);

            TextBlock textBlock = new TextBlock()
            {
                Text = "Reparar Discos",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.AliceBlue,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(textBlock, 1);

            container.Children.Add(icon);
            container.Children.Add(textBlock);
            card.Content = container;

            card.MouseLeave += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = Brushes.Transparent;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            };

            card.MouseEnter += delegate (object sender, System.Windows.Input.MouseEventArgs e)
            {
                card.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(129, 212, 250));
            };

            card.Click += delegate (object sender, RoutedEventArgs args)
            {
                ClassView.LineaComandos.Scandisk();
            };

            return card;
        }

        private static List<RadioButton> CreateButton(Dictionary<string, string> pairs)
        {
            List<RadioButton> buttons = new List<RadioButton>();
            foreach (var item in pairs)
            {
                buttons.Add(CreateToolCard(item.Key, item.Value, GetIconForTool(item.Key)));
            }
            return buttons;
        }

        public static List<RadioButton> Tools()
        {
            return CreateButton(Herramientas());
        }
    }
}