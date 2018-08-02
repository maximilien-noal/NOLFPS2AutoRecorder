using System;
using System.Runtime.InteropServices;

namespace NOLFAutoRecorder.Automation
{
    internal static class WindowReorganizer
    {
        const int SW_MINIMIZE = 6;
        const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fromAltTab);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int swCommand);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
