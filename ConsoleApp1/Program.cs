using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


//C:\Users\Ian\Documents\6890\FirstProject\targetfolder
namespace ConsoleApp1
{
    internal static class StorageSystem
    {
        // sends a write message containing, the opcode W
        // the filename, the location/offset, and the data to be written
        private static void SendWrite(string filename, int location, byte[] data, bool extracopy)
        {
            Byte opcode = (byte) 'W';
            Byte[] locationcode = BitConverter.GetBytes(location);
            Byte lengthcode = (byte) data.Length;
            Console.WriteLine(locationcode.Length);
            if (extracopy == false)
            {
                for (int i = 0; i < 3; i++)
                {
                    string copyname = i.ToString() + filename;
                    Console.WriteLine(copyname);
                    var sendarray = new byte[48];
                    sendarray[0] = opcode;
                    Byte[] filecode = Encoding.ASCII.GetBytes(copyname);
                    Buffer.BlockCopy(filecode, 0, sendarray, 1, filecode.Length);
                    Buffer.BlockCopy(locationcode, 0, sendarray, 33, 4);
                    sendarray[37] = lengthcode;
                    Buffer.BlockCopy(data, 0, sendarray, 38, data.Length);
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
                for (int i = 0; i < 6; i++)
                {
                    string copyname = i.ToString() + filename;
                    var sendarray = new byte[48];
                    sendarray[0] = opcode;
                    Byte[] filecode = Encoding.ASCII.GetBytes(copyname);
                    Buffer.BlockCopy(filecode, 0, sendarray, 1, filecode.Length);
                    Buffer.BlockCopy(locationcode, 0, sendarray, 33, 4);
                    sendarray[37] = lengthcode;
                    Buffer.BlockCopy(data, 0, sendarray, 38, data.Length);
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
        private static byte[][] SendRead(string filename, int location, bool extracopy)
        {
            if (extracopy == true)
            {
                byte[][] readvals = new byte[6][];
                for (int i = 0; i < 6; i++)
                {
                    string copyname = i.ToString() + filename;
                    var opcode = (byte) 'R';
                    var filecode = Encoding.ASCII.GetBytes(copyname);
                    var locationcode = BitConverter.GetBytes(location);
                    var sendarray = new byte[37];
                    sendarray[0] = opcode;
                    filecode.CopyTo(sendarray, 1);
                    locationcode.CopyTo(sendarray, 33);
                    Tuple<bool, byte[]> sent = UdpReadsend(sendarray);
                    while (sent.Item1 == false)
                    {
                        sent = UdpReadsend(sendarray);
                    }

                    readvals[i] = sent.Item2;
                }

                return readvals;
            }
            else
            {
                byte[][] readvals = new byte[3][];
                for (int i = 0; i < 3; i++)
                {
                    string copyname = i.ToString() + filename;
                    var opcode = (byte) 'R';
                    var filecode = Encoding.ASCII.GetBytes(copyname);
                    var locationcode = BitConverter.GetBytes(location);
                    var sendarray = new byte[37];
                    sendarray[0] = opcode;
                    filecode.CopyTo(sendarray, 1);
                    locationcode.CopyTo(sendarray, 33);
                    Tuple<bool, byte[]> sent = UdpReadsend(sendarray);
                    while (sent.Item1 == false)
                    {
                        sent = UdpReadsend(sendarray);
                    }

                    readvals[i] = sent.Item2;
                }

                return readvals;
            }
        }


        //breaks apart a file and sends the correct chunk specified by the offset/location given
        private static void FileChunk(string path, bool extracopy)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            Console.WriteLine(fileBytes.Length);
            int size = fileBytes.Length;
            for (int i = 0; i <= fileBytes.Length; i += 10)
            {
                if (size - i >= 10)
                {
                    byte[] sendchunk = new byte[10];
                    for (int j = i; j <= i + 9; j++)
                        //Console.WriteLine(file_bytes[j]);
                        //THIS MAYBE AN ISSUE IF THE LAST BIT DOES NOT APPEAR
                        // CHANGE THIS TO THE LENGTH OF THE ARRAY IN THAT CASE 
                        if (j >= fileBytes.Length)
                        {
                        }
                        else
                        {
                            sendchunk[j - i] = fileBytes[j];
                        }

                    if (extracopy == true)
                    {
                        SendWrite(path, i, sendchunk, true);
                    }
                    else if (extracopy == false)
                    {
                        SendWrite(path, i, sendchunk, false);
                    }
                }
                else
                {
                    byte[] sendchunk = new byte[size - i];
                    for (int j = i; j <= i + 9; j++)
                        //Console.WriteLine(file_bytes[j]);
                        //THIS MAYBE AN ISSUE IF THE LAST BIT DOES NOT APPEAR
                        // CHANGE THIS TO THE LENGTH OF THE ARRAY IN THAT CASE 
                        if (j >= fileBytes.Length)
                        {
                        }
                        else
                        {
                            sendchunk[j - i] = fileBytes[j];
                        }

                    if (extracopy == true)
                    {
                        SendWrite(path, i, sendchunk, true);
                    }
                    else if (extracopy == false)
                    {
                        SendWrite(path, i, sendchunk, false);
                    }
                }
            }
        }

        //chunks out a file, but then calls read on the original and copies
        // then uses the bytes found to check integrity
        //TODO create voting system function or inline that ensures data integrity
        private static void readchunk(string path, bool extracopy)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            int size = fileBytes.Length;

            for (int i = 0; i <= fileBytes.Length; i += 10)
            {
                if (size - i >= 10)
                {
                    if (extracopy == true)
                    {
                        byte[][] checkBytes = new byte[6][];
                        checkBytes = SendRead(path, i, true);
                    }
                    else if (extracopy == false)
                    {
                        
                        byte[][] checkbytes = SendRead(path, i, false);
                        byte[][] correctingdata = new byte[3][];
                        correctingdata[0] = new byte[10];
                        correctingdata[1] = new byte[10];
                        correctingdata[2] = new byte[10];
                        int k = 0;
                        for (int j = 38; j < 48; j++,k++)
                        {
                            correctingdata[0][k] = checkbytes[0][j];
                            correctingdata[1][k] = checkbytes[1][j];
                            correctingdata[2][k] = checkbytes[2][j];
                        }
                        //break each dataset into two different arrays, then compare them for differences 
                        byte [] first1 = new byte[5];
                        byte [] first2 = new byte[5];
                        byte [] first3 = new byte[5];
                        byte [] second1 = new byte[5];
                        byte [] second2 = new byte[5];
                        byte [] second3 = new byte[5];
                        Array.Copy(correctingdata[0], 0, first1, 0, 5);
                        Array.Copy(correctingdata[0], 4, second1, 0, 5);
                        Array.Copy(correctingdata[1], 0, first2, 0, 5);
                        Array.Copy(correctingdata[1], 4, second2, 0, 5);
                        Array.Copy(correctingdata[2], 0, first3, 0, 5);
                        Array.Copy(correctingdata[2], 4, second3, 0, 5);
                        bool first1and2 = first1.SequenceEqual(first2);
                        bool first2and3 = first2.SequenceEqual(first3);
                        bool first3and1 = first3.SequenceEqual(first1);
                        bool second1and2 = second1.SequenceEqual(second2);
                        bool second2and3 = second2.SequenceEqual(second3);
                        bool second3and1 = second3.SequenceEqual(second1);
                        




                    }
                }
                else
                {
                    if (extracopy == true)
                    {
                        byte[][] checkBytes = new byte[6][];
                        checkBytes = SendRead(path, i, true);
                    }
                    else if (extracopy == false)
                    {
                        byte[][] checkBytes = new byte[3][];
                        checkBytes = SendRead(path, i, false);
                    }
                }
            }
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
            successful = UdpReceive();
            return successful;
        }

        /// <summary>
        /// UdpReceive receives the ACK packet that is sent back from the corrupter executable and checks if the write packet successfully went through 
        /// </summary>
        /// <returns> true for a successful write, false otherwise </returns>
        private static bool UdpReceive()
        {
            bool done = false;
            UdpClient listener = new UdpClient(1983);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Loopback, 1983);
            string received_data;
            byte[] receive_byte_array;
            Console.WriteLine("Waiting for broadcast");
            receive_byte_array = listener.Receive(ref groupEP);
            Console.WriteLine("Received a broadcast from IP Address {0}", groupEP.ToString());
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
                successful = true;
            }

            return successful;
        }

        private static Tuple<bool, byte[]> UdpReadsend(byte[] sendarray)
        {
            //this code sends the message 
            //UDP setup for sending
            IPEndPoint sendPoint = new IPEndPoint(IPAddress.Loopback, 1982);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //UDP setup for receiving 
            server.SendTo(sendarray, sendarray.Length, SocketFlags.None, sendPoint);
            Tuple<bool, byte[]> successful = UdpReadReceive();
            return successful;
        }

        private static Tuple<bool, byte[]> UdpReadReceive()
        {
            UdpClient listener = new UdpClient(1983);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Loopback, 1983);
            string received_data;
            byte[] receive_byte_array;
            //Console.WriteLine("Waiting for broadcast");
            receive_byte_array = listener.Receive(ref groupEP);
            //Console.WriteLine("Received a broadcast from IP Address {0}", groupEP.ToString());
            //received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
            //Console.WriteLine("readable data is \n{0}\n\n", received_data);
            listener.Close();
            bool successful = false;
            byte[] data = new byte[49];
            if (receive_byte_array[receive_byte_array.Length - 1] == 0)
            {
                //Console.WriteLine("returned Unsuccessful");
            }
            else if (receive_byte_array[receive_byte_array.Length - 1] == 1)
            {
                //Console.WriteLine("returned successful");
                successful = true;
                data = receive_byte_array;
            }

            return new Tuple<bool, byte[]>(successful, data);
        }


//may want to change the main to just accept the filename and milliseconds between corrections
// this would automate the read and write, and just ensure that the files are correct every n milliseconds 
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(
                    "Please enter a complete argument. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length > 2)
            {
                Console.WriteLine(
                    "Please enter only 2 accepted arguments. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length == 2)
            {
                if (args[1] == "-N")
                {
                    FileChunk(args[0], false);
                    readchunk(args[0], false);
                    return;
                }
                else if (args[1] == "-Y")
                {
                    FileChunk(args[0], true);
                    while (true)
                    {
                    }

                    return;
                }
            }
        }
    }
}