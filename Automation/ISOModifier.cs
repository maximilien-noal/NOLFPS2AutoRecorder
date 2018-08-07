using MetadataRetriever;
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

        static List<Tuple<long, int>> positionsAndOriginalValues = new List<Tuple<long, int>>();

        /// <summary>
        /// static address in the ISO to search for voice lines IDs.
        /// Offset relative to the start of the ISO file
        /// </summary>
        static long startAddress = Convert.ToInt64("B42EFE30", 16);

        /// <summary>
        /// static address in the ISO to end the search for voice lines IDs.
        /// Offset relative to the start of the ISO file
        /// </summary>
        static long endAddress = Convert.ToInt64("EBDCFA5F", 16);

        /// <summary>
        /// The area of the ISO file containing voice IDs
        /// </summary>
        private static byte[] areaOfInterest;

        static readonly string isoFilePath = Properties.Settings.Default.ISOPath;

        /// <summary>
        /// Modifies the ISO to make the next batch available to the recorder
        /// </summary>
        /// <param name="refVoiceIdToReplace">The real voice id of the first line of the scene</param>
        /// <param name="startIdOfNextBatch">Will replace the ref with this one, plus next 5 IDs (incremental)</param>
        /// <param name="numberOfVoices">The number of voices to replace</param>
        public static void PrepareNextBatch(int refVoiceIdToReplace, int startIdOfNextBatch, int numberOfVoices)
        {

            if (string.IsNullOrWhiteSpace(isoFilePath) || File.Exists(isoFilePath) == false)
            {
                return;
            }

            List<Tuple<long, int>> positionsToWriteToAndModifiedValues = new List<Tuple<long, int>>();

            if(areaOfInterest == null)
            {
                var binReader = new BinaryReader(File.OpenRead(isoFilePath));
                using (binReader)
                {
                    binReader.BaseStream.Position = startAddress;
                    areaOfInterest = binReader.ReadBytes((int)(endAddress - startAddress));
                }
            }

            Parallel.For(0, numberOfVoices, i =>
            {
                int voiceIdToSeek = SoundInfo.GetVoiceIdAtOffsetFromVoiceId(refVoiceIdToReplace, i);
                int voiceIdToPut = SoundInfo.GetVoiceIdAtOffsetFromVoiceId(startIdOfNextBatch, i);

                byte[] voiceIdToSeekAsByteArray = BitConverter.GetBytes(voiceIdToSeek);
                if(BitConverter.IsLittleEndian)
                {
                    voiceIdToSeekAsByteArray = voiceIdToSeekAsByteArray.Reverse().ToArray();
                }

                int startPosOfFoundVoiceId = PatternAt(areaOfInterest, voiceIdToSeekAsByteArray);

                if(startPosOfFoundVoiceId != -1)
                {
                    long finalPosInFile = startPosOfFoundVoiceId + startAddress;

                    positionsToWriteToAndModifiedValues.Add(new Tuple<long, int>(
                                        startPosOfFoundVoiceId,
                                        voiceIdToPut));
                    positionsAndOriginalValues.Add(new Tuple<long, int>(
                        startPosOfFoundVoiceId,
                        voiceIdToSeek));
                }
            });

            var binWriter = new BinaryWriter(File.OpenRead(isoFilePath));
            binWriter.BaseStream.Position = startAddress;
            using (binWriter)
            {
                foreach (var positionAndModifiedValue in positionsToWriteToAndModifiedValues)
                {
                    binWriter.BaseStream.Position = positionAndModifiedValue.Item1;
                    binWriter.Write(positionAndModifiedValue.Item2);
                }
                binWriter.Flush();
            }
        }

        private static int PatternAt(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Restores the ISO to it's original state without having to copy the original file over.
        /// </summary>
        public static void UndoModifications()
        {
            if (string.IsNullOrWhiteSpace(isoFilePath) || File.Exists(isoFilePath) == false)
            {
                return;
            }

            var binWriter = new BinaryWriter(File.OpenRead(isoFilePath), Encoding.ASCII);
            binWriter.BaseStream.Position = startAddress;
            using (binWriter)
            {
                foreach (var positionAndOriginalValue in positionsAndOriginalValues)
                {
                    binWriter.BaseStream.Position = positionAndOriginalValue.Item1;
                    binWriter.Write(positionAndOriginalValue.Item2);
                }
                binWriter.Flush();
            }
        }
    }
}
