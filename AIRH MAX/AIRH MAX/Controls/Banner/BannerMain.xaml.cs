// Controls/Banner/BannerMain.xaml.cs
using AIRH_MAX.ClassView;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario.Banner
{
    public partial class BannerMain : UserControl
    {
        private readonly INavigationService _navigationService;
        private const string UrlYouTube = "http://www.youtube.com/c/RigorVzla";
        private const string UrlWeb = "https://av-airh.com/";
        private const string UrlTelegram = "https://t.me/rigorvzla";

        public BannerMain()
        {
            InitializeComponent();
            // Inyección simple (en producción, usaría un contenedor IoC)
            _navigationService = new NavigationService();
        }

        private void BtnPaypal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _navigationService.ShowLicenseDialog();
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnYouTube_Click(object sender, RoutedEventArgs e) => OpenUrlSafe(UrlYouTube);
        private void BtnWeb_Click(object sender, RoutedEventArgs e) => OpenUrlSafe(UrlWeb);
        private void RadioButton_Click(object sender, RoutedEventArgs e) => OpenUrlSafe(UrlTelegram);

        private void OpenUrlSafe(string url)
        {
            try
            {
                _navigationService.OpenUrl(url);
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error al abrir enlace", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}