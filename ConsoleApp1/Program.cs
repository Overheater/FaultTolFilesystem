﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApp1
{
    internal class StorageSystem
    {
        // sends a write message containing, the opcode W
        // the filename, the location/offset, and the data to be written 
        //TODO: FIND APPROPRIATE FILLER WHEN BYTECODE IS SHORTER THAN SET LENGTH 
        private static void SendWrite(string filename, int location, byte[] data,bool extracopy)
        {
            Byte opcode = (byte) 'W';
            Byte [] locationcode = BitConverter.GetBytes(location);
            Byte lengthcode = (byte) data.Length;
            Console.WriteLine(locationcode.Length);
            if (extracopy == false)
            {
                for (int i = 0; i<3;i++ )
                {
                    filename = i.ToString() + filename;
                    Console.WriteLine(filename);
                    var sendarray = new byte[48];
                    sendarray[0] = opcode;
                    Byte [] filecode = Encoding.ASCII.GetBytes(filename);
                    Buffer.BlockCopy(filecode,0,sendarray,1, filecode.Length);
                    Buffer.BlockCopy(locationcode,0,sendarray,33,4);
                    sendarray[37] = lengthcode;
                    Buffer.BlockCopy(data,0,sendarray,38,data.Length);
                    Udpsend(sendarray, 'S');
                }
            }
            // ReSharper disable once RedundantBoolCompare
            if (extracopy == true)
            {
             for (int i = 0; i<6;i++ )
             {
                var filecode = Encoding.ASCII.GetBytes(filename);
                var sendarray = new byte[48];
                sendarray[0] = opcode;
                filecode.CopyTo(sendarray, 1);
                locationcode.CopyTo(sendarray, 33);
                sendarray[37] = lengthcode;
                data.CopyTo(sendarray, 38);
                Udpsend(sendarray, 'S');
             }
            }
        }

        // sends a read message containing, the opcode R
        // the filename,and the location/offset
        //TODO create a repeat system that recalls itself if the read send fails
        //TODO: Form a voting system, as well as creating a repeating system if the write send fails 
        private void SendRead(string filename, int location)
        {
            var opcode = (byte) 'R';
            var filecode = Encoding.ASCII.GetBytes(filename);
            var locationcode = BitConverter.GetBytes(location);
            var sendarray = new byte[37];
            sendarray[0] = opcode;
            filecode.CopyTo(sendarray,1);
            locationcode.CopyTo(sendarray,33);
            Udpsend(sendarray,'R');

        }

        //breaks apart a file and sends the correct chunk specified by the offset/location given
        private static void FileChunk(string path,bool extracopy)
        {
            byte [] fileBytes = File.ReadAllBytes(path);
            Console.WriteLine(fileBytes.Length);
            int size = fileBytes.Length;
            for (int i = 0; i <= fileBytes.Length; i += 10)
            {
                if (size - i >= 10)
                {
                    byte [] sendchunk = new byte[10];
                    for (int j = i; j <= i + 9; j++)
                        //Console.WriteLine(file_bytes[j]);
                        //THIS MAYBE AN ISSUE IF THE LAST BIT DOES NOT APPEAR
                        // CHANGE THIS TO THE LENGTH OF THE ARRAY IN THAT CASE 
                        if (j >= fileBytes.Length )
                        {
                        }
                        else
                        {
                            sendchunk[j - i] = fileBytes[j];
                        }

                    if (extracopy == true)
                    {
                        SendWrite(path, i, sendchunk,true);
                    }
                    else if (extracopy == false)
                    {
                        SendWrite(path, i, sendchunk,false);
                    }
                }
                else
                {
                    byte[] sendchunk = new byte[size - i];
                    for (int j = i; j <= i + 9; j++)
                        //Console.WriteLine(file_bytes[j]);
                        //THIS MAYBE AN ISSUE IF THE LAST BIT DOES NOT APPEAR
                        // CHANGE THIS TO THE LENGTH OF THE ARRAY IN THAT CASE 
                        if (j >= fileBytes.Length )
                        {
                        }
                        else
                        {
                            sendchunk[j - i] = fileBytes[j];
                        }

                    if (extracopy == true)
                    {
                        SendWrite(path, i, sendchunk,true);
                    }
                    else if (extracopy == false)
                    {
                        SendWrite(path, i, sendchunk,false);
                    }
                }
            }
        }
        //TODO: add repeated function system using the functype variable to correctly resend  
        private static void Udpsend(byte[] sendarray, char functype)
        {
            bool successful = false;
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress serveradress = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(serveradress, 1982);
            sock.SendTo(sendarray, endPoint);
        }

//may want to change the main to just accept the filename and milliseconds between corrections
// this would automate the read and write, and just ensure that the files are correct every n milliseconds 
        private static void Main(string[] args)
        {
            if (args.Length <2)
            {
                Console.WriteLine("Please enter a complete argument. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length >2)
            {
                Console.WriteLine("Please enter only 2 accepted arguments. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length ==2)
            {
                if (args[1] == "-N")
                {
                    FileChunk(args[0],false);
                    while (true)
                    {
                    
                    }
                }
                else if (args[1] == "-Y")
                {
                    FileChunk(args[0],true);
                    while (true)
                    {
                    
                    }

                    return;
                }

            }
        }
    }
}