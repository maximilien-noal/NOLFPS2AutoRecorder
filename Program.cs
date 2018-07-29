using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NOLFAutoRecorder
{
    class Program
    {
        static string pcsx2ExePath = @"C:\Jeux\PCSX2\PCSX2.EXE";
        static string nolfPs2IsoPath = @"C:\Jeux\ISOs\NOLF.ISO";

        static string fmediaPath = @"C:\Jeux\Outils\fmedia.exe";
        static string fmediaStartArgs = "--record --dev-loopback=0 --globcmd=listen -o ";
        static string fmediaStopArgs = "--globcmd=stop";
        static string fmediaWorkDir = @"C:\Jeux\ISOs\VOICE_FR\_Recordings";

        static int tempRecordFileNameStart = 1;

        static void Main(string[] args)
        {
            var pcsx2Process = StartPcsx2();
            Console.WriteLine("Emulator started");
            pcsx2Process.WaitForInputIdle();
            Console.WriteLine("Recording process started");
            StartRecorder();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            StopRecorder();
            StopProcess(pcsx2Process);
        }

        static Process StartPcsx2()
        {
            var psi = new ProcessStartInfo();
            psi.Arguments = nolfPs2IsoPath;
            psi.WorkingDirectory = Path.GetDirectoryName(pcsx2ExePath);
            psi.FileName = pcsx2ExePath;
            return Process.Start(psi);
        }

        static void StartRecorder()
        {
            var psi = new ProcessStartInfo();
            psi.Arguments = fmediaStartArgs + GetTempFileName();
            psi.WorkingDirectory = fmediaWorkDir;
            psi.FileName = fmediaPath;
            Process.Start(psi);
        }

        static string GetTempFileName()
        {
            string tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}.wav", tempRecordFileNameStart));
            while(File.Exists(tempFileName))
            {
                tempRecordFileNameStart++;
                tempFileName = Path.Combine(fmediaWorkDir, string.Format("{0}.wav", tempRecordFileNameStart));
            }
            return tempFileName;
        }

        static void StopRecorder()
        {
            Process.Start(fmediaPath, fmediaStopArgs);
        }

        static void StopProcess(Process procToStop)
        {
            try
            {
                if(procToStop == null)
                {
                    return;
                }
                procToStop.Kill();
            }
            catch
            {
            }
        }
    }
}
