using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class StorageSystem
    {
        // sends a write message containing, the opcode W
        // the filename, the location/offset, and the data to be written 
        static void SendWrite(string filename, int location, byte[] data )
        {
            byte opcode  = (byte)'W';
            
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
            byte [] file_bytes = File.ReadAllBytes(path);
            for (int i = 0; i <= file_bytes.Length; i+=10)
            {
                byte [] sendchunk  = new byte[10];
                for (int j = i; j <= (i + 10); j++)
                {
                    Console.WriteLine(file_bytes[j]);

                    sendchunk.Append(file_bytes[j]);
                    
                    
                }

                SendWrite(path, i, sendchunk);

            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a an argument.");
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