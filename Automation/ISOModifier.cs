using MetadataRetriever;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NOLFAutoRecorder.Automation
{

    /// <summary>
    /// Makes the next set of voices available to the recording process
    /// </summary>
    static internal class ISOModifier
    {
        /// <summary>
        /// Cache of all positions and values of each reference to a voice ID in the ISO file
        /// </summary>
        static List<Tuple<long, int>> voiceIDsPositionsAndValueCache;

        /// <summary>
        /// static address in the ISO to search for voice lines IDs.
        /// Offset relative to the start of the ISO file
        /// </summary>
        static readonly long startAddress = Convert.ToInt64("B42EFE30", 16);
        
        static readonly string isoFilePath = Properties.Settings.Default.ISOPath;

        static readonly string cacheFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\VoiceIDsAddressesCache.csv");

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
                throw new InvalidOperationException("Fichier ISO introuvable ou chemin non renseigné");
            }

            LazyLoadCache();

            List<Tuple<long, int>> positionsToWriteToAndModifiedValues = new List<Tuple<long, int>>();

            for (int i = 0; i < numberOfVoices; i++)
            {
                int voiceIdToSeek = SoundInfo.GetVoiceIdAtOffsetFromVoiceId(refVoiceIdToReplace, i);
                int voiceIdToPut = SoundInfo.GetVoiceIdAtOffsetFromVoiceId(startIdOfNextBatch, i);

                if (voiceIDsPositionsAndValueCache.Any(x => x.Item2 == voiceIdToSeek))
                {
                    long positionInFile = voiceIDsPositionsAndValueCache.Where(x => x.Item2 == voiceIdToSeek).FirstOrDefault().Item1;

                    if (positionInFile != 0)
                    {
                        positionsToWriteToAndModifiedValues.Add(new Tuple<long, int>(
                                            positionInFile,
                                            voiceIdToPut));
                    }
                }
            }

            var binWriter = new BinaryWriter(File.OpenWrite(isoFilePath), Encoding.ASCII);
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

        private static void LazyLoadCache()
        {
            if (voiceIDsPositionsAndValueCache == null)
            {
                voiceIDsPositionsAndValueCache = new List<Tuple<long, int>>();
                if (File.Exists(cacheFilePath) == false)
                {
                    throw new InvalidOperationException("Fichier cache introuvable ou chemin non renseigné");
                }
                foreach (var line in File.ReadAllLines(cacheFilePath))
                {
                    string[] splittedLine = line.Split(';');
                    if (splittedLine[0] == "-1")
                    {
                        splittedLine[0] = "0";
                    }
                    var entry = new Tuple<long, int>(Convert.ToInt64(splittedLine[0], 16), int.Parse(splittedLine[1]));
                    voiceIDsPositionsAndValueCache.Add(entry);
                }
            }
        }

        /// <summary>
        /// Restores the ISO to it's original state without having to copy the original file over.
        /// </summary>
        public static void UndoModifications()
        {
            if (string.IsNullOrWhiteSpace(isoFilePath) || File.Exists(isoFilePath) == false)
            {
                throw new InvalidOperationException("Fichier ISO introuvable ou chemin non renseigné");
            }

            LazyLoadCache();

            var binWriter = new BinaryWriter(File.OpenWrite(isoFilePath), Encoding.ASCII);
            binWriter.BaseStream.Position = startAddress;
            using (binWriter)
            {
                foreach (var positionAndOriginalValue in voiceIDsPositionsAndValueCache)
                {
                    binWriter.BaseStream.Position = positionAndOriginalValue.Item1;
                    binWriter.Write(positionAndOriginalValue.Item2);
                }
                binWriter.Flush();
            }
        }
    }
}
