using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        private Timer _timer = new Timer();

        private InputSimulator inputSimulator = new InputSimulator();

        private Action _keyboardAction;

        private TimeSpan _inputDuration = TimeSpan.FromSeconds(1);

        public InputImpersonator(IntPtr targetWindow)
        {
            _targetWindow = targetWindow;
            this._timer.Tick += Timer_Tick;
            this._timer.Interval = 40;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(this._keyboardAction == null)
            {
                return;
            }
            WindowReorganizer.SetActiveWindow(_targetWindow);
            this._keyboardAction.Invoke();
        }

        public void Advance()
        {
            this._timer.Enabled = true;
            this._keyboardAction = new Action(() =>
            {
                this.inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_U);
            });
            for (int i = 0; i < this._inputDuration.Ticks; i++)
            {
                Application.DoEvents();
            }
            this._timer.Enabled = false;
        }

        public void GoBack()
        {

        }

        public void Interact()
        {

        }

        public void TurnLeft()
        {

        }

        public void TurnRight()
        {

        }
    }
}
