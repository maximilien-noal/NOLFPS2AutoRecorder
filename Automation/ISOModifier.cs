using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="file">Path to the ISO file</param>
        /// <param name="refVoiceIdToReplace">The real voice id of the first line of the scene</param>
        /// <param name="startIdOfNextBatch">Will replace the ref with this one, plus next 5 IDs (incremental)</param>
        /// <param name="numberOfVoices">The number of voices to replace</param>
        static void PrepareNextBatch(string file, int refVoiceIdToReplace, int startIdOfNextBatch, int numberOfVoices)
        {
            var fileStream = File.OpenWrite(file);
            fileStream.Position = startAddress;

            for(int i = refVoiceIdToReplace; i <= refVoiceIdToReplace + numberOfVoices; i++)
            {
                int voiceIdToSeek = refVoiceIdToReplace + i;
                int voiceIdToPut = startIdOfNextBatch + i;

                
            }
        }
    }
}
