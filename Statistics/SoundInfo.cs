using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NOLFAutoRecorder.Statistics
{
    public static class SoundInfo
    {

        public static long GetSoundLengthOfFiles(int startVoiceId, int numberOfFiles, string dirPath)
        {
            long lengthSumOfFiles = 0;
            for(int i = startVoiceId; i <= startVoiceId + numberOfFiles; i++)
            {
                string fileName = string.Format("{0}.WAV", i);
                string fullFileName = Path.Combine(dirPath, fileName);
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
