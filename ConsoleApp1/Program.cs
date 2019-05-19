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
        //TODO: Form a voting system, as well as creating a repeating system if the write send fails 
        //TODO: FIND APPROPRIATE FILLER WHEN BYTECODE IS SHORTER THAN SET LENGTH 
        static void SendWrite(string filename, int location, byte[] data )
        {
            Byte opcode  = (byte)'W';
            Byte[] filecode = System.Text.Encoding.ASCII.GetBytes(filename);
            Byte[] locationcode = BitConverter.GetBytes(location);
            Byte length = (byte)data.Length;
            Console.WriteLine(locationcode.Length);
            Byte []sendarray = new Byte[48];
            sendarray[0] = opcode;
            filecode.CopyTo(sendarray,1);
            locationcode.CopyTo(sendarray,33);
            sendarray[38] = length;
            data.CopyTo(sendarray, 39);
            

            return;

        }
        // sends a read message containing, the opcode R
        // the filename,and the location/offset
        //TODO create a repeat system that recalls itself if the read send fails 
        void SendRead(string filename, int location)
        {
            Byte opcode  = (byte)'R';
            Byte[] filecode = System.Text.Encoding.ASCII.GetBytes(filename);
            Byte[] locationcode = BitConverter.GetBytes(location);
            
        }
        //breaks apart a file and sends the correct chunk specified by the offset/location given
        public static void FileChunk(string path)
        {
            byte [] file_bytes = File.ReadAllBytes(path);
            Console.WriteLine(file_bytes.Length);
            for (int i = 0; i <= file_bytes.Length; i+=10)
            {
                byte [] sendchunk  = new byte[10];
                for (int j = i; j <= (i + 10); j++)
                {
                    //Console.WriteLine(file_bytes[j]);
                    //THIS MAYBE AN ISSUE IF THE LAST BIT DOES NOT APPEAR
                    // CHANGE THIS TO THE LENGTH OF THE ARRAY IN THAT CASE 
                    if (j >= file_bytes.Length - 1)
                    {

                    }
                    else
                    {
                        sendchunk.Append(file_bytes[j]);
                    }

                }

                SendWrite(path, i, sendchunk);

            }
        }
//may want to change the main to just accept the filename and milliseconds between corrections
// this would automate the read and write, and just ensure that the files are correct every n milliseconds 
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
            else if (args[0] == "Read")
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