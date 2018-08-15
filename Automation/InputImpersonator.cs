using System;
using WindowsInput;
using WindowsInput.Native;

namespace NOLFAutoRecorder.Automation
{
    /// <summary>
    /// "Plays" just enough to trigger the first set of spoken lines of a scene.
    /// </summary>
    internal class InputImpersonator
    {
        private IntPtr _targetWindow = IntPtr.Zero;

        private InputSimulator inputSimulator = new InputSimulator();

        private TimeSpan _inputDuration = TimeSpan.FromMilliseconds(300);

        public InputImpersonator(IntPtr targetWindow)
        {
            _targetWindow = targetWindow;
        }

        public void Advance()
        {
            TriggerAction(VirtualKeyCode.VK_E);
        }

        private void TriggerAction(VirtualKeyCode vkey)
        {
            WindowReorganizer.SetForegroundWindow(this._targetWindow);
            WindowReorganizer.SetActiveWindow(this._targetWindow);
            this.inputSimulator.Keyboard.KeyDown(vkey);
            this.inputSimulator.Keyboard.Sleep(this._inputDuration);
            this.inputSimulator.Keyboard.KeyUp(vkey);
        }

        public void GoBack()
        {
            TriggerAction(VirtualKeyCode.VK_D);
        }

        public void Interact()
        {
            TriggerAction(VirtualKeyCode.VK_Y);
        }

        public void GoLeft()
        {
            TriggerAction(VirtualKeyCode.VK_S);
        }

        public void GoRight()
        {
            TriggerAction(VirtualKeyCode.VK_F);
        }

        public void MenuDown()
        {
            TriggerAction(VirtualKeyCode.VK_K);
        }

        public void MenuUp()
        {
            TriggerAction(VirtualKeyCode.VK_I);
        }

        public void SelectFrenchLanguage()
        {
            MenuDown();
            Interact();
            MainForm.WaitSeconds(8);
        }

        /// <summary>
        /// Selects and trigger the loading of a scene from the dev menu
        /// </summary>
        /// <param name="numberOfRepeats">Number of times we have to hit 'Menu Down' button</param>
        public void LoadScene(int numberOfRepeats)
        {
            Interact();
            for(int i = 0; i < numberOfRepeats; i++)
            {
                MenuDown();
                MainForm.WaitSeconds(0.1);
            }
            Interact();
            MainForm.WaitSeconds(19);

        }

        public void LoadBerlinSceneTwo()
        {
            LoadScene(8);
        }

        public void LoadBerlinSceneThree()
        {
            LoadScene(9);
        }

        public void LoadBerlinSceneFour()
        {
            LoadScene(10);
        }

        public void LoadBerlinSceneFive()
        {
            LoadScene(11);
        }

        /// <summary>
        /// StartVoiceId : 10504
        /// EndVoiceId : 10541
        /// 9 lines available
        /// </summary>
        public void TriggerBerlinSceneOne()
        {
            Advance();
            Advance();
            Advance();
            Advance();
            Advance();
            GoLeft();
            GoLeft();
            Advance();
            Advance();
            Interact();
        }

        /// <summary>
        /// 17 lines available
        /// </summary>
        public void TriggerBerlinSceneTwo()
        {
            GoRight();
            GoRight();
            Advance();
            Advance();
        }

        /// <summary>
        /// ? lines available
        /// </summary>
        public void TriggerBerlinSceneFour()
        {
            GoRight();
            GoRight();
            Advance();
            Advance();
        }

        /// <summary>
        /// startVoiceId : 10597
        /// EndVoiceId : ?
        /// 6 voices available
        /// </summary>
        public void TriggerBerlinSceneFive()
        {
            Advance();
            Advance();
            GoRight();
        }
    }
}
