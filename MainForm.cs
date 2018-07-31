﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        const int SW_MINIMIZE = 6;
        const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SwitchToThisWindow(IntPtr hWnd, bool fromAltTab);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int swCommand);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        IntPtr emulatorWindow = IntPtr.Zero;
        IntPtr emulatorViewPortWindow = IntPtr.Zero;

        string pcsx2ExePath = @"C:\Jeux\PCSX2\PCSX2.EXE";
        string nolfPs2IsoPath = @"C:\Jeux\ISOs\NOLF.ISO";

        string fmediaPath = @"C:\Jeux\Outils\fmedia.exe";
        string fmediaStartArgs = "--record --dev-loopback=0 --globcmd=listen -o ";
        string fmediaStopArgs = "--globcmd=stop";
        string fmediaWorkDir = @"C:\Jeux\ISOs\VOICE_FR\_Recordings";

        int tempRecordFileNameStart = 10521;

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
            pcsx2Process.WaitForInputIdle();
            emulatorWindow = pcsx2Process.MainWindowHandle;
            Thread.Sleep(TimeSpan.FromSeconds(13));
            emulatorViewPortWindow = WndFinder.SearchForWindow("wxWindowNR", "Slot");
            bool quickLoadSuccess = LoadQuickSave();
            SetForegroundWindow(emulatorViewPortWindow);
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

        void WriteLog(string message, ToolTipIcon icon)
        {
            this.LogTextBox.AppendText(string.Format("{0} : {1}{2}", icon.ToString(), message, Environment.NewLine));
        }

        bool LoadQuickSave()
        {
            if (emulatorViewPortWindow == null)
            {
                return false;
            }
            SetActiveWindow(emulatorViewPortWindow);
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