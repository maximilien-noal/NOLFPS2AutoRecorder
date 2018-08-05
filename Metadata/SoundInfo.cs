using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NOLFAutoRecorder.Metadata
{
    public static class SoundInfo
    {
        static List<int> listOfAllUsaVoiceIDs = SoundInfo.GetAllVoiceIDs(Properties.Settings.Default.VoiceUsaDir);

        private static List<int> GetAllVoiceIDs(string dirPath)
        {
            var voiceIds = new List<int>();
            var directoryFiles = Directory.EnumerateFiles(dirPath, "*.wav", SearchOption.TopDirectoryOnly).ToList();
            for (int i = 0; i < directoryFiles.Count(); i++)
            {
                var currentFile = directoryFiles[i];
                string filenameAlone = Path.GetFileNameWithoutExtension(currentFile);
                int voiceId = 0;
                int.TryParse(filenameAlone, out voiceId);
                voiceIds.Add(voiceId);
            }
            return voiceIds;
        }

        public static int GetNextVoiceId(int previousId)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(previousId);
            indexOfNextId = indexOfNextId + 1;
            if (indexOfNextId <= 0 || indexOfNextId > listOfAllUsaVoiceIDs.Count - 1)
            {
                throw new ArgumentOutOfRangeException();
            }


            return listOfAllUsaVoiceIDs[indexOfNextId];
        }

        public static int GetVoiceIdAtOffsetFromVoiceId(int previousId, int offset)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(previousId);
            indexOfNextId += offset;
            if (indexOfNextId <= 0 || indexOfNextId > listOfAllUsaVoiceIDs.Count - 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            return listOfAllUsaVoiceIDs[indexOfNextId];
        }

        public static List<int> GetNextVoiceIds(int previousId, int count)
        {
            int indexOfNextId = listOfAllUsaVoiceIDs.IndexOf(previousId);
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
        public static long GetSoundLengthOfFiles(int startVoiceId, int numberOfFiles)
        {
            long lengthSumOfFiles = 0;
            for(int i = 0; i < numberOfFiles; i++)
            {
                string fileName = string.Format("{0}.WAV", startVoiceId);
                if(i > 0)
                {
                    string voiceId = GetNextVoiceId(startVoiceId).ToString();
                    fileName = string.Format("{0}.WAV", voiceId);
                }
                string fullFileName = Path.Combine(Properties.Settings.Default.VoiceUsaDir, fileName);
                if(File.Exists(fullFileName))
                {
                    lengthSumOfFiles += GetSoundLength(fullFileName);
                }
            }
            return Convert.ToInt64(Math.Round(lengthSumOfFiles * 1.2, 0));
        }

        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        /// <summary>
        /// Returns the length of the WAVE file, in seconds
        /// </summary>
        /// <param name="fileName">The WAVE file to test</param>
        /// <returns>The result from the WINMM/MCI API</returns>
        private static int GetSoundLength(string fileName)
        {

            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }
    }
}
