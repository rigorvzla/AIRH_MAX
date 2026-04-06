using MaterialDesignThemes.Wpf;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX.ClassView
{
    internal class ControlesManuales
    {
        public static int TokenID = 0;
        public static Dictionary<int, CancellationTokenSource> taskLookup = new Dictionary<int, CancellationTokenSource>();

        public static System.Windows.Controls.RadioButton ButtomTemplate()
        {
            var packIcon = new PackIcon
            {
                Kind = PackIconKind.StopCircle, // Icono similar a circle-stop
                Foreground = System.Windows.Media.Brushes.AliceBlue,
                Width = 16,
                Height = 16,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            System.Windows.Controls.RadioButton a = new System.Windows.Controls.RadioButton()
            {
                Content = packIcon,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderBrush = System.Windows.Media.Brushes.Transparent,
                Style = (Style)Application.Current.Resources["RadioButtomTheme"],
                Width = 20,
                Height = 20,
                Tag = TokenID
            };

            a.Click += delegate (object sender, RoutedEventArgs args)
            {
                taskLookup.ElementAt(Convert.ToInt32(a.Tag)).Value.Cancel();
                Xceed.Wpf.Toolkit.MessageBox.Show("Tarea cancelada", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            return a;
        }

        public static System.Windows.Controls.RadioButton RadioTemplate(string ruta)
        {
            var packIcon = new PackIcon
            {
                Kind = PackIconKind.PlayCircle, 
                Foreground = System.Windows.Media.Brushes.AliceBlue,
                Width = 16,
                Height = 16,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            System.Windows.Controls.RadioButton a = new System.Windows.Controls.RadioButton()
            {
                Content = packIcon,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderBrush = System.Windows.Media.Brushes.Transparent,
                Style = (Style)Application.Current.Resources["RadioButtomTheme"],
                Width = 20,
                Height = 20,
                Tag = TokenID
            };

            a.Click += delegate (object sender, RoutedEventArgs args)
            {
                Engrane.EXE(ruta);
            };

            return a;
        }
    }
}
