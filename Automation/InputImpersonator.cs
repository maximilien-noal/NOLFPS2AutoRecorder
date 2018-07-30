using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOLFAutoRecorder.Automation
{
    /// <summary>
    /// "Plays" just enough to trigger the first set of spoken lines of a scene.
    /// </summary>
    internal class InputImpersonator
    {
        private IntPtr _targetWindow = IntPtr.Zero;
        public InputImpersonator(IntPtr targetWindow)
        {
            _targetWindow = targetWindow;
        }
    }
}
