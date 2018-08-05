using NOLFAutoRecorder.Automation;
using NOLFAutoRecorder.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        readonly string pcsx2ExePath = Properties.Settings.Default.PCSX2ExePath;
        const string nolfPs2IsoPath = @"C:\Jeux\ISOs\NOLF.ISO";

        readonly string fmediaPath = Properties.Settings.Default.FMediaExePath;
        const string fmediaStartArgs = "--record --dev-loopback=0 --globcmd=listen -o ";
        const string fmediaStopArgs = "--globcmd=stop";
        readonly string fmediaWorkDir = Properties.Settings.Default.TempRecordingsWorkDir;

        readonly string voiceUsaDir = Properties.Settings.Default.VoiceUsaDir;
        
        System.Windows.Forms.Timer recordEndTimer = new System.Windows.Forms.Timer();

        List<Action> endOfLifeActions = new List<Action>();

        DateTime recordEndTime = DateTime.Now;

        static int currentBatchStartVoiceId = 10521;

        static readonly int endVoiceId = 13757;

        Process pcsx2Process;

        public MainForm()
        {
            InitializeComponent();
            endOfLifeActions.Add(StopRecorder);
            endOfLifeActions.Add(() => StopProcess(pcsx2Process));
            endOfLifeActions.Add(ISOModifier.UndoModifications);
            this.Shown += MainForm_Shown;
            Application.ApplicationExit += OnApplicationExit;
            recordEndTimer.Interval = 200;
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
            int currentSceneStartVoiceId = 10504;
            int numberOfVoicesAvailable = 9;
            int currentBatchStart = 10520;
            RecordVoiceSet(currentSceneStartVoiceId, numberOfVoicesAvailable, currentBatchStart);
        }

        private void RecordVoiceSet(int startVoiceIdOfScene, int numberOfVoicesAvailable, int currentBatchStartId)
        {
            recordEndTimer.Stop();
            ISOModifier.PrepareNextBatch(nolfPs2IsoPath, startVoiceIdOfScene, currentBatchStartId, numberOfVoicesAvailable);
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
            recordEndTime.AddSeconds(SoundInfo.GetSoundLengthOfFiles(currentBatchStartId, numberOfVoicesAvailable, voiceUsaDir));
            recordEndTimer.Start();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Parallel.ForEach(this.endOfLifeActions, action =>
            {
                action.Invoke();
            });
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
            string tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}_.wav", currentBatchStartVoiceId));
            if(File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
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
