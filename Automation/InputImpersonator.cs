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

        public void TriggerBerlinSceneOne()
        {
            Advance();
            Advance();
            Advance();
            Advance();
            Advance();
            GoRight();
            GoRight();
            GoRight();
            GoRight();
            Advance();
            Interact();
        }
    }
}
