﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MetadataRetriever
{
    public static class SoundInfo
    {
        static List<int> listOfAllUsaVoiceIDs = SoundInfo.GetAllVoiceIDs(Properties.Settings.Default.VoiceUsaDir);

        public static List<int> GetAllVoiceIDs()
        {
            int[] copy = new int[listOfAllUsaVoiceIDs.Count];
            listOfAllUsaVoiceIDs.CopyTo(copy);
            return copy.OrderBy(x => x).ToList();
        }

        private static List<int> GetAllVoiceIDs(string dirPath)
        {
            var voiceIds = new List<int>();
            var directoryFiles = Directory.EnumerateFiles(dirPath, "*.wav", SearchOption.TopDirectoryOnly).ToList();
            for (int i = 0; i < directoryFiles.Count(); i++)
            {
                var currentFile = directoryFiles[i];
                string filenameAlone = Path.GetFileNameWithoutExtension(currentFile);
                int.TryParse(filenameAlone, out int voiceId);
                voiceIds.Add(voiceId);
            }
            return voiceIds;
        }

        public static int GetNextVoiceId(int voiceId)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(voiceId);
            indexOfNextId = indexOfNextId + 1;
            if (indexOfNextId <= 0 || indexOfNextId > listOfAllUsaVoiceIDs.Count - 1)
            {
                throw new ArgumentOutOfRangeException();
            }


            return listOfAllUsaVoiceIDs[indexOfNextId];
        }

        public static int GetVoiceIdAtOffsetFromVoiceId(int voiceId, int offset)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(voiceId);
            indexOfNextId += offset;
            if (indexOfNextId <= 0 || indexOfNextId > listOfAllUsaVoiceIDs.Count - 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            return listOfAllUsaVoiceIDs[indexOfNextId];
        }

        public static List<int> GetNextVoiceIds(int voiceId, int count)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(voiceId);
            indexOfNextId = indexOfNextId + 1;
            if (indexOfNextId <= 0 || indexOfNextId > listOfAllUsaVoiceIDs.Count - 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            return listOfAllUsaVoiceIDs.GetRange(indexOfNextId, indexOfNextId + count);
        }


        /// <summary>
        /// Returns the sum of the length of several WAVE files, in seconds
        /// </summary>
        /// <param name="startVoiceId">The first voice ID of the batch</param>
        /// <param name="numberOfFiles">The number of files to take </param>
        /// <returns>The sum of the length of the sound files, plus 20% since we are recording French instead of English</returns>
        public static int GetSoundLengthOfFiles(int startVoiceId, int numberOfFiles)
        {
            int lengthSumOfFiles = 0;
            for (int i = 0; i < numberOfFiles; i++)
            {
                string fileName = string.Format("{0}.WAV", startVoiceId);
                if (i > 0)
                {
                    string voiceId = GetNextVoiceId(startVoiceId).ToString();
                    fileName = string.Format("{0}.WAV", voiceId);
                }
                string fullFileName = Path.Combine(Properties.Settings.Default.VoiceUsaDir, fileName);
                if (File.Exists(fullFileName))
                {
                    lengthSumOfFiles += (int)GetSoundLength(fullFileName);
                }
            }
            return Convert.ToInt32(lengthSumOfFiles);
        }

        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        /// <summary>
        /// Returns the length of the WAVE file, in milliseconds
        /// </summary>
        /// <param name="fileName">The WAVE file to test</param>
        /// <returns>The result from the WINMM/MCI API</returns>
        private static decimal GetSoundLength(string fileName)
        {

            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            int.TryParse(lengthBuf.ToString(), out int length);
            
            return Math.Round((decimal)length / 1000, 0);
        }
    }
}
