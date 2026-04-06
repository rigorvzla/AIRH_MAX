using System.Runtime.InteropServices;

namespace AIRH_MAX.ClassView.SystemClass
{
    internal class MonitorOff
    {
        private static int SC_MONITORPOWER = 0xF170;

        private static uint WM_SYSCOMMAND = 0x0112;

        [DllImport("user32.dll")]
        static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        enum MonitorState
        {
            ON = -1,
            OFF = 2,
            STANDBY = 1
        }
        private static void SetMonitorState(MonitorState state)
        {
            Form frm = new Form();

            SendMessage(frm.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, (nint)state);

        }
        public static void ApagadoMonitor()
        {
            SetMonitorState(MonitorState.OFF);
        }
    }
}
