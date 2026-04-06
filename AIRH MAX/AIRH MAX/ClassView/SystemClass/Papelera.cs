using System.Runtime.InteropServices;
using System.Windows;
using Application = System.Windows.Application;

namespace AIRH_MAX.ClassView.SystemClass
{
    internal class Papelera
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(nint hwnd, string pszRootPath, RecycleFlags dwFlags);

        enum RecycleFlags : uint
        {
            SHRB_NOCONFIRMATION = 0x00000001,
            SHRB_NOPROGRESSUI = 0x00000002,
            SHRB_NOSOUND = 0x00000004
        }

        public static void limpieza()
        {
            Application app = Application.Current;
            if (app != null)
            {
                app.Dispatcher.Invoke(() =>
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("¿Esta seguro que quiere vaciar la papelera?", "AV-AIRH MAX", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        uint IsSuccess = SHEmptyRecycleBin(nint.Zero, null, RecycleFlags.SHRB_NOCONFIRMATION);
                        Views.MainWindow.NotificacionEvent.MensajeBox = "La papelera se vació";
                    }
                });
            }
        }
    }
}
