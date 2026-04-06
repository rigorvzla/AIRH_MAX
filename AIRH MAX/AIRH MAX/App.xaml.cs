using AIRH_MAX.ClassView;
using AIRH_MAX.ClassView.SystemClass;
using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Theme;
using ErrorReportingNET.Services;
using System.IO;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WpfSingleInstance.Make();
            SolicitudOnline.ApiConfigServiceGet();
            HandlerReporter.Initialize(Assembly.GetExecutingAssembly().GetName().Version.ToString(), ConstantElementsSecrets.DeviceTelegram, ConstantElementsSecrets.TokenTelegramBot);
            //VerifyLi.StartVerify_Online.Check(ConstantElementsSecrets.TokenTelegramBot, ConstantElementsSecrets.DeviceTelegram, ConstantElementsSecrets.API_BASE_URL, ConstantElementsSecrets.API_KEY);            

            if (!File.Exists(Environment.CurrentDirectory + "\\Theme.cfg"))
            {               
                AppTheme.SetTheme(Resources);
            }
            else
            {
                AppTheme.SetTheme(Resources, Engrane.File_Theme().PrimaryColor, Engrane.File_Theme().SecondaryColor, Engrane.File_Theme().PrimaryText, Engrane.File_Theme().SecondaryText, Engrane.File_Theme().PrimaryAcsendText, Engrane.File_Theme().SecondaryAcsendText);
            }
            base.OnStartup(e);
        }
    }

}
