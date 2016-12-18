using System;
using System.IO;

namespace CS_StreamCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 | (args[0] != "D" && args[0] != "E"))
            {
                Console.WriteLine("The project takes two arguments");
                Console.WriteLine("1. D or E, denoting Decode, or Encode respectively");
                Console.WriteLine("2. The path to the compiled stream for decoding, or the path to text file to encode");
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("File path pointed by second argument does not exist.");
                return;
            }

            if (args[0] == "D")
            {
                Decompile(args[1]);
                return;
            }
            else if(args[0] == "E")
            {
                Compile(args[1]);
            }
        }

        private static void Compile(string inputPath)
        {
            byte[] fileContents = File.ReadAllBytes(inputPath);
            string outputPath = inputPath + ".out";
            int filePntr = 0;
            int noRecords = 0;

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (var stream = new FileStream(outputPath, FileMode.Append))
            {

                while (filePntr < fileContents.Length - 1)
                {
                    int startPoint = filePntr;
                    int endPoint = filePntr;

                    while (filePntr < fileContents.Length - 1)
                    {
                        if (fileContents[filePntr] == 0x0D && fileContents[filePntr + 1] == 0x0A) // Match end of line chars
                        {
                            endPoint = filePntr;
                            filePntr += 2; // Skip the EOL chars for the next round.
                            break;
                        }
                        filePntr++;
                    }

                    if (endPoint - startPoint <= 0) // End of records
                    {
                        break;
                    }

                    int recordlength = endPoint - startPoint;
                    byte[] buffer = new byte[recordlength + 4];
                    Array.Copy(fileContents, startPoint, buffer, 3, recordlength);
                    buffer[buffer.Length - 1] = 0;

                    buffer[0] = (byte)(recordlength & 0xFF);
                    buffer[1] = (byte)((recordlength >> 8) & 0xFF);
                    buffer[2] = (byte)((recordlength >> 16) & 0xFF);

                    stream.Write(buffer, 0, buffer.Length);
                    noRecords++;
                }

            }

            Console.WriteLine("Complete");
            Console.WriteLine("Number of records encoded: " + noRecords.ToString());
            Console.ReadKey();

        }

        private static void Decompile(string inputPath)
        {
            byte[] fileContents = File.ReadAllBytes(inputPath);
            string outputPath = inputPath + ".txt";
            int filePnter = 0;
            int noRecords = 0;

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (var stream = new FileStream(outputPath, FileMode.Append))
            {

                while (filePnter < (fileContents.Length - 1))
                {
                    int chunkSize = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        chunkSize += fileContents[filePnter++] << (8 * i);
                    }

                    byte flag = fileContents[filePnter++];

                    byte[] buffer = new byte[chunkSize + 2]; // Stripping the null character off the end and then adding a CR & LF
                    buffer[0] = flag; // There's a flag of some sort as the initial character, don't yet know what this is responsable for???
                    Array.Copy(fileContents, filePnter, buffer, 1, chunkSize);
                    buffer[chunkSize ] = 13;
                    buffer[chunkSize+1] = 10;
                    stream.Write(buffer, 0, buffer.Length);
                    noRecords++;
                    filePnter += chunkSize;
                }
            }
            Console.WriteLine("Complete");
            Console.WriteLine("No of records decoded: " + noRecords.ToString());
            Console.ReadKey();

        }
    }
}
