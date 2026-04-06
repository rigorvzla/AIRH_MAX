using System.Windows;
using System.Windows.Media;

namespace MAX.ClassView
{
    internal class AppTheme
    {
        public static void SetTheme(ResourceDictionary resource, string primarycolor = "#FF02010F", string secondarycolor = "#FF303055", string primarytext = "#f0f8ff", string secondarytext = "#87ceeb", string primaryacsendtext = "#00FF00", string secondaryacsendtext = "#FF35FDE2", string colorprogress = "#FEF200")
        {
            System.Windows.Media.Color PrimaryColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(primarycolor);
            System.Windows.Media.Color SecondaryColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(secondarycolor);
            System.Windows.Media.Color PrimaryText = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(primarytext);
            System.Windows.Media.Color SecondaryText = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(secondarytext);
            System.Windows.Media.Color PrimaryAcsendText = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(primaryacsendtext);
            System.Windows.Media.Color SecondaryAcsendText = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(secondaryacsendtext);
            System.Windows.Media.Color ColorProgress = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorprogress);

            resource["PrimaryColor"] = new SolidColorBrush(PrimaryColor);
            resource["SecondaryColor"] = new SolidColorBrush(SecondaryColor);
            resource["PrimaryText"] = new SolidColorBrush(PrimaryText);
            resource["SecondaryText"] = new SolidColorBrush(SecondaryText);
            resource["PrimaryAcsendText"] = new SolidColorBrush(PrimaryAcsendText);
            resource["SecondaryAcsendText"] = new SolidColorBrush(SecondaryAcsendText);
            resource["ColorProgress"] = new SolidColorBrush(ColorProgress);
        }
    }
}
