using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class StorageSystem
    {
        // sends a write message containing, the opcode W
        // the filename, the location/offset, and the data to be written 
        void SendWrite(string filename, int location, byte[] data )
        {
            
        }
        // sends a read message containing, the opcode R
        // the filename,and the location/offset
        void SendRead(string filename, int location)
        {
            
        }
        //breaks apart a file and sends the correct chunk specified by the offset/location given 
        //TODO change this to a full file chunking system, then feed into the sendWrite function one piece at a time 
        public static void FileChunk(string path)
        {
            
            byte[] chunk = new byte[10];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // TODO change the hardcoded 10 to a chunk size variable if you want, not really needed though 
                int sizeval = fs.Read(chunk,0, 10);
                Console.WriteLine(sizeval);
                Console.WriteLine(chunk);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a an argument.");
                return;

            }
            if (args[0] == "Write")
            {
                if (args.Length == 1)
                {
                    Console.WriteLine("you need a filepath to run this command");
                }
                else if (args.Length == 2)
                {
                    FileChunk(args[1]);
                }
            }
        }
    }
}