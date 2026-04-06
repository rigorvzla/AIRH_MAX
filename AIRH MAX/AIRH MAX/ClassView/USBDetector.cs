using System.Management;
using System.Windows;
using System.Windows.Threading;

namespace AIRH_MAX.ClassView
{
    internal class USBDetector : Window
    {
        ManagementEventWatcher watcher = new ManagementEventWatcher();
        WqlEventQuery consulta = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

        public void USBWatcherON()
        {
            Task.Run(() =>
            {
                watcher.Query = consulta;
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Start();
            });
        }

        public void USBWatcherOFF()
        {
            Dispatcher.Invoke(() =>
            {
                watcher.Stop();
                watcher.EventArrived -= Watcher_EventArrived;
            });
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Views.MainWindow.NotificacionEvent.MensajeBox = "Memoria USB detectada, unidad " + e.NewEvent.Properties["DriveName"].Value.ToString();
            });
        }
    }
}
