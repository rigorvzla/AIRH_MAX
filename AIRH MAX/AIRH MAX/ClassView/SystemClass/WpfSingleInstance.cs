using System.Windows;
using System.Windows.Threading;

namespace AIRH_MAX.ClassView.SystemClass
{
    internal class WpfSingleInstance
    {
        public enum SingleInstanceModes
        {
            NotInited = 0,
            ForEveryUser,
        }

        internal static void Make()
        {
            Make(SingleInstanceModes.ForEveryUser);
        }

        internal static void Make(SingleInstanceModes singleInstanceModes)
        {
            var appName = System.Windows.Application.Current.GetType().Assembly.ManifestModule.ScopeName;

            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var keyUserName = windowsIdentity != null ? windowsIdentity.User.ToString() : string.Empty;

            var eventWaitHandleName = string.Format(
                "{0}{1}",
                appName,
                singleInstanceModes == SingleInstanceModes.ForEveryUser ? keyUserName : string.Empty
                );

            try
            {
                using (var eventWaitHandle = EventWaitHandle.OpenExisting(eventWaitHandleName))
                {
                    eventWaitHandle.Set();
                }
                Environment.Exit(0);
            }
            catch
            {
                using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventWaitHandleName))
                {
                    ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, OtherInstanceAttemptedToStart, null, Timeout.Infinite, false);
                }

                RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            }
        }

        private static void OtherInstanceAttemptedToStart(object state, bool timedOut)
        {
            RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { try { System.Windows.Application.Current.MainWindow.Activate(); } catch { } }));
        }

        internal static DispatcherTimer AutoExitAplicationIfStartupDeadlock;

        public static void RemoveApplicationsStartupDeadlockForStartupCrushedWindows()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AutoExitAplicationIfStartupDeadlock =
                    new DispatcherTimer(
                        TimeSpan.FromSeconds(6),
                        DispatcherPriority.ApplicationIdle,
                        (o, args) =>
                        {
                            if (System.Windows.Application.Current.Windows.Cast<Window>().Count(window => !double.IsNaN(window.Left)) == 0)
                            {
                                Environment.Exit(0);
                            }
                        },
                        System.Windows.Application.Current.Dispatcher
                    );
            }),
                DispatcherPriority.ApplicationIdle
                );
        }
    }
}
