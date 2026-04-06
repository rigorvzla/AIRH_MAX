using ErrorReportingNET.Services;
using MAX.ClassView;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace MAX
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FileConfig();
            HandlerReporter.Initialize(Assembly.GetExecutingAssembly().GetName().Version.ToString(), "[1]", "[0]"); 
            StartMAX.Start(e.Args);
            base.OnStartup(e);
        }
        private void FileConfig()
        {
            var Theme = Engrane.File_Theme();
            AppTheme.SetTheme(Resources, Theme.PrimaryColor, Theme.SecondaryColor, Theme.PrimaryText, Theme.SecondaryText, Theme.PrimaryAcsendText, Theme.SecondaryAcsendText);
        }
    }

}
