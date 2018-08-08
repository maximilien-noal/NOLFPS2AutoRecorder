using MetadataRetriever;
using NOLFAutoRecorder.Automation;
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

        List<Action> endOfRecordSessionActions = new List<Action>();

        DateTime recordEndTime = DateTime.MinValue;

        Process pcsx2Process;

        public MainForm()
        {
            InitializeComponent();

            ISOModifier.UndoModifications();

            endOfRecordSessionActions.Add(StopRecorder);
            endOfRecordSessionActions.Add(() => StopProcess(pcsx2Process));

            this.Shown += MainForm_Shown;
            Application.ApplicationExit += OnApplicationExit;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            RecordBerlinSceneOne();
        }

        private void RecordBerlinSceneOne()
        {
            int currentSceneStartVoiceId = 10504; //always the same ID
            int numberOfVoicesAvailable = 9; //number of lines in the dialogue used
            int currentBatchStartId = 10520; //loop over it until the voice ID no longer belongs to the Scene
            int currentSceneLastVoiceId = 10600; //Wild guess...
            var triggerAction = new Action(() =>
            {
                WriteLog("Activating Berlin Scene One dialogue...", ToolTipIcon.Warning);
                this.inputImpersonator.TriggerBerlinSceneOne();
            });
            
            //The ID are linear between 10520 and 10600, so we can simply loop over it with a for,
            // but other scenes will need a different loop
            for (int i = currentBatchStartId; i <= currentSceneLastVoiceId; i++)
            {
                RecordVoiceSet(currentSceneStartVoiceId, numberOfVoicesAvailable, currentBatchStartId, triggerAction);
                WaitSeconds(SoundInfo.GetSoundLengthOfFiles(i, currentSceneLastVoiceId - currentBatchStartId));
            }
        }

        private void RecordVoiceSet(int startVoiceIdOfScene, int numberOfVoicesAvailable, int currentBatchStartId, Action activationAction)
        {
            ISOModifier.PrepareNextBatch(startVoiceIdOfScene, currentBatchStartId, numberOfVoicesAvailable);
            pcsx2Process = StartPcsx2();
            pcsx2Process.WaitForInputIdle();
            emulatorWindow = pcsx2Process.MainWindowHandle;
            //Wait for the emulator to load the main screen...
            WaitSeconds(12);
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
            //Wait for the quicksave to be loaded
            WaitSeconds(22);
            activationAction.Invoke();
            StartRecorder(currentBatchStartId, numberOfVoicesAvailable);
        }

        private static void WaitSeconds(int seconds)
        {
            var whenEmulatorReady = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < whenEmulatorReady)
            {
                Application.DoEvents();
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Parallel.ForEach(endOfRecordSessionActions, action =>
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
            var psi = new ProcessStartInfo
            {
                Arguments = nolfPs2IsoPath,
                WorkingDirectory = Path.GetDirectoryName(pcsx2ExePath),
                FileName = pcsx2ExePath
            };
            WriteLog("Emulator started", ToolTipIcon.Info);
            return Process.Start(psi);
        }

        void StartRecorder(int currentBatchStartId, int numberOfVoicesAvailable)
        {
            var psi = new ProcessStartInfo
            {
                Arguments = fmediaStartArgs + GetTempFileName(currentBatchStartId, numberOfVoicesAvailable),
                WorkingDirectory = fmediaWorkDir,
                FileName = fmediaPath,
                WindowStyle = ProcessWindowStyle.Minimized
            };
            Process.Start(psi);
            WriteLog("Recording process started", ToolTipIcon.Info);
        }

        string GetTempFileName(int currentBatchStartId, int numberOfVoicesAvailable)
        {
            string tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}_{1}.wav", currentBatchStartId, SoundInfo.GetVoiceIdAtOffsetFromVoiceId(currentBatchStartId, numberOfVoicesAvailable)));
            if(File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            return tempFileName;
        }

        void StopRecorder()
        {
            Process.Start(fmediaPath, fmediaStopArgs);
            WriteLog("Recording process stopped", ToolTipIcon.Info);
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
                WriteLog("Emulator stopped", ToolTipIcon.Info);
            }
            catch
            {
            }
        }
    }
}
