using NOLFAutoRecorder.Automation;
using NOLFAutoRecorder.Statistics;
using System;
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
        
        IntPtr emulatorWindow = IntPtr.Zero;
        IntPtr emulatorViewPortWindow = IntPtr.Zero;
        private InputImpersonator inputImpersonator;
        string pcsx2ExePath = @"C:\Jeux\PCSX2\PCSX2.EXE";
        string nolfPs2IsoPath = @"C:\Jeux\ISOs\NOLF.ISO";

        string fmediaPath = @"C:\Jeux\Outils\fmedia.exe";
        string fmediaStartArgs = "--record --dev-loopback=0 --globcmd=listen -o ";
        string fmediaStopArgs = "--globcmd=stop";
        string fmediaWorkDir = @"C:\Jeux\ISOs\VOICE_FR\_Recordings";

        string voiceUsaDir = @"C:\Jeux\ISOs\VOICE_USA";

        int tempRecordFileNameStart = 10521;

        System.Windows.Forms.Timer recordEndTimer = new System.Windows.Forms.Timer();

        DateTime recordEndTime = DateTime.Now;

        Process pcsx2Process;

        public MainForm()
        {
            InitializeComponent();
            this.Shown += MainForm_Shown;
            Application.ApplicationExit += OnApplicationExit;
            recordEndTimer.Interval = 400;
            recordEndTimer.Tick += RecordEndTimer_Tick;
        }

        private void RecordEndTimer_Tick(object sender, EventArgs e)
        {
            if(DateTime.Now.Ticks >= recordEndTime.Ticks)
            {
                Application.Exit();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            ISOModifier.PrepareNextBatch(nolfPs2IsoPath, 10504, tempRecordFileNameStart, 9);
            pcsx2Process = StartPcsx2();
            pcsx2Process.WaitForInputIdle();
            emulatorWindow = pcsx2Process.MainWindowHandle;
            Thread.Sleep(TimeSpan.FromSeconds(12));
            emulatorViewPortWindow = WndFinder.SearchForWindow("wxWindowNR", "Slot");
            this.inputImpersonator = new InputImpersonator(emulatorViewPortWindow);
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
            Thread.Sleep(TimeSpan.FromSeconds(22));
            WriteLog("Trigerring Berlin Scene One...", ToolTipIcon.Warning);
            this.inputImpersonator.TriggerBerlinSceneOne();
            StartRecorder();
            recordEndTime = DateTime.Now;
            recordEndTime.AddSeconds(SoundInfo.GetSoundLengthOfFiles(this.tempRecordFileNameStart, 9, voiceUsaDir));
            recordEndTimer.Start();
        }

        private void OnApplicationExit(object sender, EventArgs e)
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
            WindowReorganizer.SetActiveWindow(emulatorViewPortWindow);
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
            string tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}_.wav", tempRecordFileNameStart));
            while (File.Exists(tempFileName))
            {
                tempRecordFileNameStart++;
                tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}_.wav", tempRecordFileNameStart));
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
