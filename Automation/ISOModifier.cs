using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        static string isoFilePath = "";

        /// <summary>
        /// Modifies the ISO to make the next batch available to the recorder
        /// </summary>
        /// <param name="file">Path to the ISO file</param>
        /// <param name="refVoiceIdToReplace">The real voice id of the first line of the scene</param>
        /// <param name="startIdOfNextBatch">Will replace the ref with this one, plus next 5 IDs (incremental)</param>
        /// <param name="numberOfVoices">The number of voices to replace</param>
        public static void PrepareNextBatch(string file, int refVoiceIdToReplace, int startIdOfNextBatch, int numberOfVoices)
        {
            isoFilePath = file;

            if(string.IsNullOrWhiteSpace(file) || File.Exists(isoFilePath) == false)
            {
                return;
            }

            List<Tuple<long, int>> positionsToWriteToAndModifiedValues = new List<Tuple<long, int>>();

            var binReader = new BinaryReader(File.OpenRead(file), Encoding.ASCII);
            using (binReader)
            {
                binReader.BaseStream.Position = startAddress;

                for (int i = refVoiceIdToReplace; i <= refVoiceIdToReplace + numberOfVoices; i++)
                {
                    if (binReader.BaseStream.Position >= binReader.BaseStream.Length)
                    {
                        return;
                    }

                    int voiceIdToSeek = refVoiceIdToReplace + i;
                    int voiceIdToPut = startIdOfNextBatch + i;
                    string foundVoiceId = "";

                    while (binReader.BaseStream.Position >= binReader.BaseStream.Length)
                    {
                        char character = binReader.ReadChar();
                        int readCharToInt = 0;
                        bool parsedIntSuccessfully = int.TryParse(character.ToString(), out readCharToInt);
                        if (parsedIntSuccessfully)
                        {
                            foundVoiceId += readCharToInt.ToString();
                        }

                        if(binReader.BaseStream.Position >= binReader.BaseStream.Length)
                        {
                            break;
                        }

                        if (foundVoiceId.Length == voiceIdToSeek.ToString().Length)
                        {
                            break;
                        }
                    }

                    long startPosOfFoundVoiceId = binReader.BaseStream.Position - Encoding.ASCII.GetByteCount(foundVoiceId.ToCharArray());
                    int foundVoiceIdToInt = 0;
                    int.TryParse(foundVoiceId, out foundVoiceIdToInt);

                    positionsToWriteToAndModifiedValues.Add(new Tuple<long, int>(
                        startPosOfFoundVoiceId,
                        voiceIdToPut));
                    positionsAndOriginalValues.Add(new Tuple<long, int>(
                        startPosOfFoundVoiceId,
                        foundVoiceIdToInt));
                }
            }

            var binWriter = new BinaryWriter(File.OpenRead(file), Encoding.ASCII);
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

        /// <summary>
        /// Restores the ISO to it's original state without having to copy the original file over.
        /// </summary>
        public static void UndoModifications()
        {
            if(string.IsNullOrWhiteSpace(isoFilePath) || File.Exists(isoFilePath) == false)
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
