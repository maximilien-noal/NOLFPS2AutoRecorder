using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using MetadataRetriever;
using System.Threading;

namespace ISOVoiceIDAddressCacheMaker
{
    class Program
    {
        /// <summary>
        /// Builds a file where all the references to voice IDs in the
        /// ISO file are mapped. To be used as a cache by ISOModifier
        /// </summary>
        static void Main()
        {
            string isoFile = Properties.Settings.Default.ISOPath;
            string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            long startPos = Convert.ToInt64(Properties.Settings.Default.StartAddressOfScan, 16);
            // long endPos = Convert.ToInt64(Properties.Settings.Default.EndAddressOfScan, 16);
            var listeOfVoiceIDsToFind = SoundInfo.GetAllVoiceIDs();

            List<Tuple<string, int>> listeOfPositionsAndVoiceId = new List<Tuple<string, int>>();

            string logFilePath = Path.Combine(workDir, "output.log");
            List<string> logLines = new List<string>();
            if(File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            string errorsLogFilePath = Path.Combine(workDir, "errors.log");
            List<string> errorsLines = new List<string>();
            if (File.Exists(errorsLogFilePath))
            {
                File.Delete(errorsLogFilePath);
            }

            string outputFilePath = Path.Combine(workDir, "VoiceIDsAddressesCache.csv");
            if(File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var binReader = new BinaryReader(File.OpenRead(isoFile));
            binReader.BaseStream.Position = startPos;

            Console.WriteLine("Searching...");
            File.AppendAllLines(logFilePath, new string[] { "Searching..." });

            using (binReader)
            {
                var areaOfInterest = binReader.ReadBytes((int)(binReader.BaseStream.Length - binReader.BaseStream.Position));

                Parallel.ForEach(listeOfVoiceIDsToFind, voiceIdToSeek =>
                {
                    byte[] voiceIdToSeekAsByteArray = ASCIIEncoding.ASCII.GetBytes(voiceIdToSeek.ToString());

                    List<byte> voiceIdSurroundedByZeroes = new List<byte>();
                    List<byte> voiceIdTailedWithZero = new List<byte>();

                    //To avoid false positives
                    voiceIdSurroundedByZeroes.Add(BitConverter.GetBytes(0)[0]);
                    voiceIdSurroundedByZeroes.AddRange(voiceIdToSeekAsByteArray);
                    voiceIdTailedWithZero.AddRange(voiceIdToSeekAsByteArray);
                    //To avoid false positives
                    voiceIdSurroundedByZeroes.Add(BitConverter.GetBytes(0)[0]);
                    voiceIdTailedWithZero.Add(BitConverter.GetBytes(0)[0]);

                    int indexOfVoiceId = GetIndexOfBytePattern(areaOfInterest, voiceIdSurroundedByZeroes.ToArray());

                    if (indexOfVoiceId != -1)
                    {
                        indexOfVoiceId = GetIndexOfBytePattern(areaOfInterest, voiceIdTailedWithZero.ToArray());
                    }

                    if (indexOfVoiceId != -1)
                    {
                        indexOfVoiceId = GetIndexOfBytePattern(areaOfInterest, voiceIdToSeekAsByteArray);
                    }

                    if (indexOfVoiceId != -1)
                    {
                        long finalIndex = indexOfVoiceId + startPos;

                        string logLineVoiceId = @"index found for voiceID : " + voiceIdToSeek + "->" + Convert.ToString(finalIndex, 16);
                        Console.WriteLine(logLineVoiceId);
                        logLines.Add(logLineVoiceId);

                        listeOfPositionsAndVoiceId.Add(new Tuple<string, int>(Convert.ToString(finalIndex, 16), voiceIdToSeek));
                    }
                    else
                    {
                        string logLineFailure = @"/!\ index NOT found for voiceID : " + voiceIdToSeek;
                        logLines.Add(logLineFailure);
                        errorsLines.Add(logLineFailure);
                        Console.WriteLine(logLineFailure);
                        listeOfPositionsAndVoiceId.Add(new Tuple<string, int>(indexOfVoiceId.ToString(), voiceIdToSeek));
                    }
                });
            }

            string logLine = "Writing results to " + outputFilePath;
            logLines.Add(logLine);
            Console.WriteLine(logLine);

            File.AppendAllLines(logFilePath, logLines.ToArray());
            File.AppendAllLines(errorsLogFilePath, errorsLines.ToArray());

            List<string> outputContent = new List<string>();

            Parallel.ForEach(listeOfPositionsAndVoiceId.AsParallel().OrderBy(x => x.Item2), IndexAndVoiceId =>
            {
                string line = string.Format("{0};{1}", IndexAndVoiceId.Item1, IndexAndVoiceId.Item2.ToString());
                outputContent.Add(line);
            });

            File.WriteAllLines(outputFilePath, outputContent);

            string finalLogLine = "Done ! :)";
            File.AppendAllLines(logFilePath, new string[] { finalLogLine });
            Console.WriteLine(finalLogLine);
        }

        /// <summary>
        /// Returns the index of a pattern of bytes in a byte array
        /// </summary>
        /// <param name="searchIn">the byte array to search in</param>
        /// <param name="searchBytes">the pattern to search</param>
        /// <returns>the 0 based index of the pattern, or -1 if not found</returns>
        private static int GetIndexOfBytePattern(byte[] searchIn, byte[] searchBytes)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start
            if (searchIn.Length > 0 && searchBytes.Length > 0 && 0 <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched
                for (int i = 0; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up
                            if (matched)
                            {
                                found = i;
                                break;
                            }

                        }
                        else
                        {
                            //search byte is only one bit nothing else to do
                            found = i;
                            break; //stop the loop
                        }

                    }
                }

            }
            return found;
        }
    }
}
