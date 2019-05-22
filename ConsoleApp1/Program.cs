using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

//C:\Users\Ian\Documents\6890\FirstProject\targetfolder
namespace ConsoleApp1
{
    internal class StorageSystem
    {
        // sends a write message containing, the opcode W
        // the filename, the location/offset, and the data to be written
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
                    string copyname = i.ToString() + filename;
                    Console.WriteLine(copyname);
                    var sendarray = new byte[48];
                    sendarray[0] = opcode;
                    Byte [] filecode = Encoding.ASCII.GetBytes(copyname);
                    Buffer.BlockCopy(filecode,0,sendarray,1, filecode.Length);
                    Buffer.BlockCopy(locationcode,0,sendarray,33,4);
                    sendarray[37] = lengthcode;
                    Buffer.BlockCopy(data,0,sendarray,38,data.Length);
                    bool sent = Udpsend(sendarray);
                    while (sent == false)
                    {
                        sent = Udpsend(sendarray);
                    }
                }
            }
            // ReSharper disable once RedundantBoolCompare
            if (extracopy == true)
            {
             for (int i = 0; i<6;i++ )
             {
                 string copyname = i.ToString() + filename;
                 var sendarray = new byte[48];
                 sendarray[0] = opcode;
                 Byte [] filecode = Encoding.ASCII.GetBytes(copyname);
                 Buffer.BlockCopy(filecode,0,sendarray,1, filecode.Length);
                 Buffer.BlockCopy(locationcode,0,sendarray,33,4);
                 sendarray[37] = lengthcode;
                 Buffer.BlockCopy(data,0,sendarray,38,data.Length);
                 bool sent = Udpsend(sendarray);
                 while (sent == false)
                 {
                     sent = Udpsend(sendarray);
                 }
             }
            }
        }

        // sends a read message containing, the opcode R
        // the filename,and the location/offset
        //TODO: create an array of byte arrays to send down to the read chunk function. 
        private byte[][] SendRead(string filename, int location,bool extracopy)
        {
            if (extracopy == true)
            {
                byte [][] readvals = new byte[6][];
                for (int i = 0; i < 6; i++)
                {
                    string copyname = i.ToString() + filename;
                    var opcode = (byte) 'R';
                    var filecode = Encoding.ASCII.GetBytes(copyname);
                    var locationcode = BitConverter.GetBytes(location);
                    var sendarray = new byte[37];
                    sendarray[0] = opcode;
                    filecode.CopyTo(sendarray,1);
                    locationcode.CopyTo(sendarray,33);
                    bool sent = Udpsend(sendarray);
                    while (sent == false)
                    {
                         sent = Udpsend(sendarray);
                    }


                }
                return readvals;
            }
            else
            {
                byte [][] readvals = new byte[3][];
                for (int i = 0; i <3; i++)
                {
                    
                    string copyname = i.ToString() + filename;
                    var opcode = (byte) 'R';
                    var filecode = Encoding.ASCII.GetBytes(copyname);
                    var locationcode = BitConverter.GetBytes(location);
                    var sendarray = new byte[37];
                    sendarray[0] = opcode;
                    filecode.CopyTo(sendarray,1);
                    locationcode.CopyTo(sendarray,33);
                    bool sent = Udpsend(sendarray);
                    while (sent == false)
                    {
                         sent = Udpsend(sendarray);
                    }
                    

                }
                return readvals;
            }
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

        //chunks out a file, but then calls read on the original and copies
        // to get the needed bytes to check integrity 
        private static void readchunk()
        {
            
        }


        /// <summary>
        ///  UDPsend sends a byte array for the write function in the project. if the function sends successfully, it returns true. if it doesnt, it returns false 
        /// </summary>
        /// <param name="sendarray"> the byte array that will be sent via udp</param>
        /// <returns></returns>
        private static bool Udpsend(byte[] sendarray)
        {
            //this code sends the message 
            bool successful = false;
            //UDP setup for sending
            IPEndPoint sendPoint = new IPEndPoint(IPAddress.Loopback, 1982);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //UDP setup for receiving 
            server.SendTo(sendarray, sendarray.Length, SocketFlags.None, sendPoint);
            successful = UDPrecieve();
            return successful;

            // this code recieves the ack or data message and chooses to resend if not valid
            //TODO write receive code, if valid true, if invalid false
        }

        private static bool UDPrecieve()
        {
            bool done = false;
            UdpClient listener = new UdpClient(1983);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Loopback, 1983);
            string received_data;
            byte[] receive_byte_array;
            Console.WriteLine("Waiting for broadcast");
            receive_byte_array = listener.Receive(ref groupEP);
            Console.WriteLine("Received a broadcast from IP Address {0}", groupEP.ToString() );
            received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
            Console.WriteLine("readable data is \n{0}\n\n", received_data);
            listener.Close();
            bool successful = false;
            if (receive_byte_array[receive_byte_array.Length - 1] == 0)
            {
                Console.WriteLine("returned Unsuccessful");
                successful = false;
            }
            else if (receive_byte_array[receive_byte_array.Length - 1] == 1)
            {
                Console.WriteLine("returned successful");
                successful= true;
            }

            return successful;
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