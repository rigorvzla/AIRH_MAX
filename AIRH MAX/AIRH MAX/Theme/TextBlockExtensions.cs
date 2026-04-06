using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AIRH_MAX.Theme
{
    public class TextBlockExtensions
    {
        public static readonly DependencyProperty AutoLinkProperty =
            DependencyProperty.RegisterAttached(
                "AutoLink",
                typeof(string),
                typeof(TextBlockExtensions),
                new PropertyMetadata(null, OnAutoLinkChanged));

        public static string GetAutoLink(DependencyObject obj) => (string)obj.GetValue(AutoLinkProperty);
        public static void SetAutoLink(DependencyObject obj, string value) => obj.SetValue(AutoLinkProperty, value);

        private static void OnAutoLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.Inlines.Clear();

                var mensaje = e.NewValue as string;
                if (string.IsNullOrEmpty(mensaje))
                    return;

                // Expresión regular para URLs (HTTP/HTTPS)
                var regex = new Regex(
                   @"https?://(?:[-\w.])+(?:[:\d]+)?(?:/[-\w._~:/?#\[\]@!$&'()*+,;=%]*)?",
                    RegexOptions.IgnoreCase);

                int lastIndex = 0;
                foreach (Match match in regex.Matches(mensaje))
                {
                    // Texto antes de la URL
                    if (match.Index > lastIndex)
                    {
                        textBlock.Inlines.Add(new Run(mensaje.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    // Crear hipervínculo
                    var uri = new Uri(match.Value);
                    var hyperlink = new Hyperlink(new Run(match.Value))
                    {
                        NavigateUri = uri,
                        ToolTip = match.Value
                    };
                    hyperlink.RequestNavigate += (sender, args) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(args.Uri.AbsoluteUri)
                            {
                                UseShellExecute = true
                            });
                        }
                        catch { /* Puedes mostrar un mensaje si falla */ }
                    };

                    textBlock.Inlines.Add(hyperlink);
                    lastIndex = match.Index + match.Length;
                }

                // Texto restante después de la última URL
                if (lastIndex < mensaje.Length)
                {
                    textBlock.Inlines.Add(new Run(mensaje.Substring(lastIndex)));
                }
            }
        }
    }
}