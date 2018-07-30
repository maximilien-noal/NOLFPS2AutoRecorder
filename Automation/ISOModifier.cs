using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOLFAutoRecorder.Automation
{

    /// <summary>
    /// Makes the next set of voices available to the recording process
    /// </summary>
    static internal class ISOModifier
    {
        /// <summary>
        /// static address in the ISO to search for voice lines IDs.
        /// Offset relative to the start of the ISO file
        /// </summary>
        static long startAddress = Convert.ToInt64("B42EFE30", 16);
        
        /// <summary>
        /// Modifies the ISO to make the next batch available
        /// </summary>
        /// <param name="refVoiceIdToReplace">The real voice id of the first line of the scene</param>
        /// <param name="startIdOfNextBatch">Will replace the ref with this one, plus next 5 IDs (incremental)</param>
        static void PrepareNextBatch(int refVoiceIdToReplace, int startIdOfNextBatch)
        {

        }
    }
}
