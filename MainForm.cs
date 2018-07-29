using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace NOLFAutoRecorder
{
    public partial class MainForm : Form
    {
        Process pcsx2Process;

        public MainForm()
        {
            InitializeComponent();
            this.Shown += MainForm_Shown;
            Application.ApplicationExit += Application_ApplicationExit;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            pcsx2Process = StartPcsx2();
            Console.WriteLine("Emulator started");
            pcsx2Process.WaitForInputIdle();
            emulatorWindow = pcsx2Process.MainWindowHandle;
            Thread.Sleep(TimeSpan.FromSeconds(13));
            emulatorViewPortWindow = WndFinder.SearchForWindow("wxWindowNR", "Slot");
            bool quickLoadSuccess = LoadQuickSave();
            if (quickLoadSuccess)
            {
                WriteLog("QuickSave loaded", ToolTipIcon.Info);
            }
            else
            {
                WriteLog("QuickSave NOT loaded !", ToolTipIcon.Error);
                return;
            }
            WriteLog("Recording process started", ToolTipIcon.Info);
            StartRecorder();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            StopRecorder();
            StopProcess(pcsx2Process);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SwitchToThisWindow(IntPtr hWnd, bool fromAltTab);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string windowClass, string windowName);

        static IntPtr emulatorWindow = IntPtr.Zero;
        static IntPtr emulatorViewPortWindow = IntPtr.Zero;

        static string pcsx2ExePath = @"C:\Jeux\PCSX2\PCSX2.EXE";
        static string nolfPs2IsoPath = @"C:\Jeux\ISOs\NOLF.ISO";

        static string fmediaPath = @"C:\Jeux\Outils\fmedia.exe";
        static string fmediaStartArgs = "--record --dev-loopback=0 --globcmd=listen -o ";
        static string fmediaStopArgs = "--globcmd=stop";
        static string fmediaWorkDir = @"C:\Jeux\ISOs\VOICE_FR\_Recordings";

        static int tempRecordFileNameStart = 1;

        void WriteLog(string message, ToolTipIcon icon)
        {
            this.LogTextBox.AppendText(string.Format("{0} : {1}{2}", icon.ToString(), message, Environment.NewLine));
        }

        bool LoadQuickSave()
        {
            /*
            if (emulatorViewPortWindow == null)
            {
                return false;
            }
            SwitchToThisWindow(emulatorViewPortWindow, false);
            */
            new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.F3);
            return true;
        }

        Process StartPcsx2()
        {
            var psi = new ProcessStartInfo();
            psi.Arguments = nolfPs2IsoPath;
            psi.WorkingDirectory = Path.GetDirectoryName(pcsx2ExePath);
            psi.FileName = pcsx2ExePath;
            return Process.Start(psi);
        }

        void StartRecorder()
        {
            var psi = new ProcessStartInfo();
            psi.Arguments = fmediaStartArgs + GetTempFileName();
            psi.WorkingDirectory = fmediaWorkDir;
            psi.FileName = fmediaPath;
            psi.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start(psi);
        }

        string GetTempFileName()
        {
            string tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}.wav", tempRecordFileNameStart));
            while (File.Exists(tempFileName))
            {
                tempRecordFileNameStart++;
                tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}.wav", tempRecordFileNameStart));
            }
            return tempFileName;
        }

        void StopRecorder()
        {
            Process.Start(fmediaPath, fmediaStopArgs);
        }

        void StopProcess(Process procToStop)
        {
            try
            {
                if (procToStop == null)
                {
                    return;
                }
                procToStop.Kill();
            }
            catch
            {
            }
        }
    }
}
