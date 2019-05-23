using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;


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
                        //the bools for which data blocks are incorrect
                        bool Cfirst1 = true;
                        bool Cfirst2 = true;
                        bool Cfirst3 = true;
                        bool Csecond1 = true;
                        bool Csecond2 = true;
                        bool Csecond3 = true;
                        bool Cthird1 = true;
                        bool Cthird2 = true;
                        bool Cthird3 = true;
                        //break each dataset into two different arrays, then compare them for differences 
                        byte [] first1 = new byte[3];
                        byte [] first2 = new byte[3];
                        byte [] first3 = new byte[3];
                        byte [] second1 = new byte[3];
                        byte [] second2 = new byte[3];
                        byte [] second3 = new byte[3];
                        byte [] third1 = new byte[4];
                        byte [] third2 = new byte[4];
                        byte [] third3 = new byte[4];
                        Array.Copy(correctingdata[0], 0, first1, 0, 3);
                        Array.Copy(correctingdata[0], 3, second1, 0, 3);
                        Array.Copy(correctingdata[0], 6, third1, 0, 4);
                        Array.Copy(correctingdata[1], 0, first2, 0, 3);
                        Array.Copy(correctingdata[1], 3, second2, 0, 3);
                        Array.Copy(correctingdata[1], 6, third2, 0, 4);
                        Array.Copy(correctingdata[2], 0, first3, 0, 3);
                        Array.Copy(correctingdata[2], 3, second3, 0, 3);
                        Array.Copy(correctingdata[2], 6, third3, 0, 4);
                            ;
                        bool first1and2 = first1.SequenceEqual(first2);
                        bool first2and3 = first2.SequenceEqual(first3);
                        bool first3and1 = first3.SequenceEqual(first1);
                        bool second1and2 = second1.SequenceEqual(second2);
                        bool second2and3 = second2.SequenceEqual(second3);
                        bool second3and1 = second3.SequenceEqual(second1);
                        bool third1and2 = third1.SequenceEqual(third2);
                        bool third2and3 = third2.SequenceEqual(third3);
                        bool third3and1 = third3.SequenceEqual(third1);
                        //TODO create if block where if 1!=2, check (1==3) and (2==3) to see which block is wrong 
                        if (first1and2 == false)
                        {
                            if (first2and3 == true)
                            {
                                Cfirst1 = false;
                            }
                            else if (first3and1 == true)
                            {
                                Cfirst2 = false;
                            }
                        }
                        //may need to change this to an else if 
                        if (first2and3 == false)
                        {
                            if (first1and2 == true)
                            {
                                Cfirst3 = false;
                            }
                            else if (first3and1 == true)
                            {
                                Cfirst2 = false;
                            }
                        }
                        if (first3and1 == false)
                        {
                            if (first1and2==true)
                            {
                                Cfirst3 = false;
                            }
                            else if (first2and3 == true)
                            {
                                Cfirst1 = false;
                            }
                        }
                        //the second part of the data is now checked 
                        if (second1and2 == false)
                        {
                            if (second2and3 == true)
                            {
                                Csecond1 = false;
                            }
                            else if (second3and1 == true)
                            {
                                Csecond2 = false;
                            }
                        }
                        //may need to change this to an else if 
                        if (second2and3 == false)
                        {
                            if (second1and2 == true)
                            {
                                Csecond3 = false;
                            }
                            else if (second3and1 == true)
                            {
                                Csecond2 = false;
                            }
                        }
                        if (second3and1 == false)
                        {
                            if (second1and2==true)
                            {
                                Csecond3 = false;
                            }
                            else if (second2and3 == true)
                            {
                                Csecond1 = false;
                            }
                        }
                        //the third part of the data is now checked 
                        if (third1and2 == false)
                        {
                            if (third2and3 == true)
                            {
                                Cthird1 = false;
                            }
                            else if (third3and1 == true)
                            {
                                Cthird2 = false;
                            }
                        }
                        //may need to change this to an else if 
                        if (third2and3 == false)
                        {
                            if (third1and2 == true)
                            {
                                Cthird3 = false;
                            }
                            else if (third3and1 == true)
                            {
                                Cthird2 = false;
                            }
                        }
                        if (third3and1 == false)
                        {
                            if (third1and2==true)
                            {
                                Cthird3 = false;
                            }
                            else if (third2and3 == true)
                            {
                                Cthird1 = false;
                            }
                        }
                        
                        //create the data for the write message for any errors found
                        byte [] first = new byte[10];
                        byte [] second = new byte[10];
                        byte [] third = new byte[10];
                        //if some of the data is incorrect, copy the correct data from another source, then replace the faulty data
                        if (Cfirst1 == false || Csecond1 == false || Cthird1==false)
                        {
                            if (Cfirst1 == false && Csecond1 == true && Cthird1 ==true)
                            {
                                Array.Copy(first2,0,first,0,3);
                                Array.Copy(second1,0,first,3,3);
                                Array.Copy(third1,0,first,6,4);
                            }
                            else if (Cfirst1 == true && Csecond1 == false && Cthird1 ==true)
                            {
                                Array.Copy(first1,0,first,0,3);
                                Array.Copy(second2,0,first,3,3);
                                Array.Copy(third1,0,first,6,4);
                            }
                            else if (Cfirst1 == true && Csecond1 == true && Cthird1 ==false)
                            {
                                Array.Copy(first1,0,first,0,3);
                                Array.Copy(second1,0,first,3,3);
                                Array.Copy(third2,0,first,6,4);
                            }
                            else if (Cfirst1 == false && Csecond1 == false && Cthird1 ==true)
                            {
                                Array.Copy(first2,0,first,0,3);
                                Array.Copy(second2,0,first,3,3);
                                Array.Copy(third1,0,first,6,4);
                            }
                            else if (Cfirst1 == false && Csecond1 == true && Cthird1 ==false)
                            {
                                Array.Copy(first2,0,first,0,3);
                                Array.Copy(second1,0,first,3,3);
                                Array.Copy(third2,0,first,6,4);
                            }
                            else if (Cfirst1 == true && Csecond1 == false && Cthird1 ==false)
                            {
                                Array.Copy(first1,0,first,0,3);
                                Array.Copy(second2,0,first,3,3);
                                Array.Copy(third2,0,first,6,4);
                            }
                            else
                            {
                                Array.Copy(first2,0,first,0,3);
                                Array.Copy(second2,0,first,3,3);
                                Array.Copy(third2,0,first,6,4);
                            }
                            byte [] filename = new byte[32];
                            byte [] location = new byte[4];
                            Array.Copy(checkbytes[0], 1, filename, 0, 32);
                            Array.Copy(checkbytes[0], 33, location, 0, 4);
                            ReadToWrite(first,filename,location);
                        }
                        
                        if (Cfirst2 == false || Csecond2 == false || Cthird2==false)
                        {
                            if (Cfirst2 == false && Csecond2 == true && Cthird2 ==true)
                            {
                                Array.Copy(first3,0,second,0,3);
                                Array.Copy(second2,0,second,3,3);
                                Array.Copy(third2,0,second,6,4);
                            }
                            else if (Cfirst2 == true && Csecond2 == false && Cthird2 ==true)
                            {
                                Array.Copy(first2,0,second,0,3);
                                Array.Copy(second3,0,second,3,3);
                                Array.Copy(third2,0,second,6,4);
                            }
                            else if (Cfirst2 == true && Csecond2 == true && Cthird2 ==false)
                            {
                                Array.Copy(first2,0,second,0,3);
                                Array.Copy(second2,0,second,3,3);
                                Array.Copy(third3,0,second,6,4);
                            }
                            else if (Cfirst2 == false && Csecond2 == false && Cthird2 ==true)
                            {
                                Array.Copy(first3,0,second,0,3);
                                Array.Copy(second3,0,second,3,3);
                                Array.Copy(third2,0,second,6,4);
                            }
                            else if (Cfirst2 == false && Csecond2 == true && Cthird2 ==false)
                            {
                                Array.Copy(first3,0,second,0,3);
                                Array.Copy(second2,0,second,3,3);
                                Array.Copy(third3,0,second,6,4);
                            }
                            else if (Cfirst2 == true && Csecond2 == false && Cthird2 ==false)
                            {
                                Array.Copy(first2,0,second,0,3);
                                Array.Copy(second3,0,second,3,3);
                                Array.Copy(third3,0,second,6,4);
                            }
                            else
                            {
                                Array.Copy(first3,0,second,0,3);
                                Array.Copy(second3,0,second,3,3);
                                Array.Copy(third3,0,second,6,4);
                            }
                            byte [] filename = new byte[32];
                            byte [] location = new byte[4];
                            Array.Copy(checkbytes[1], 1, filename, 0, 32);
                            Array.Copy(checkbytes[1], 33, location, 0, 4);
                            ReadToWrite(second,filename,location);
                        }
                        if (Cfirst3 == false || Csecond3 == false || Cthird3==false)
                        {
                            if (Cfirst3 == false && Csecond3 == true && Cthird3 ==true)
                            {
                                Array.Copy(first1,0,third,0,3);
                                Array.Copy(second3,0,third,3,3);
                                Array.Copy(third3,0,third,6,4);
                            }
                            else if (Cfirst3 == true && Csecond3 == false && Cthird3 ==true)
                            {
                                Array.Copy(first3,0,third,0,3);
                                Array.Copy(second1,0,third,3,3);
                                Array.Copy(third3,0,third,6,4);
                            }
                            else if (Cfirst3 == true && Csecond3 == true && Cthird3 ==false)
                            {
                                Array.Copy(first3,0,third,0,3);
                                Array.Copy(second3,0,third,3,3);
                                Array.Copy(third1,0,third,6,4);
                            }
                            else if (Cfirst3 == false && Csecond3 == false && Cthird3 ==true)
                            {
                                Array.Copy(first1,0,third,0,3);
                                Array.Copy(second1,0,third,3,3);
                                Array.Copy(third3,0,third,6,4);
                            }
                            else if (Cfirst3 == false && Csecond3 == true && Cthird3 ==false)
                            {
                                Array.Copy(first1,0,third,0,3);
                                Array.Copy(second3,0,third,3,3);
                                Array.Copy(third1,0,third,6,4);
                            }
                            else if (Cfirst3 == true && Csecond3 == false && Cthird3 ==false)
                            {
                                Array.Copy(first3,0,third,0,3);
                                Array.Copy(second1,0,third,3,3);
                                Array.Copy(third1,0,third,6,4);
                            }
                            else
                            {
                                Array.Copy(first1,0,third,0,3);
                                Array.Copy(second1,0,third,3,3);
                                Array.Copy(third1,0,third,6,4);
                            }
                            byte [] filename = new byte[32];
                            byte [] location = new byte[4];
                            Array.Copy(checkbytes[2], 1, filename, 0, 32);
                            Array.Copy(checkbytes[2], 33, location, 0, 4);
                            ReadToWrite(third,filename,location);
                        }
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
        /// creates a write message from the readchunk function to correct any bitflips
        /// </summary>
        private static void ReadToWrite(Byte[] data, Byte[] file, Byte[] location)
        {
            byte [] sendarray = new byte[48];
            sendarray[0] = (byte)'W';
            Buffer.BlockCopy(file, 0, sendarray, 1, 32);
            Buffer.BlockCopy(location, 0, sendarray, 33, 4);
            sendarray[37] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, sendarray, 38, 10);
            bool sent = Udpsend(sendarray);
            while (sent == false)
            {
                sent = Udpsend(sendarray);
            }
            System.Threading.Thread.Sleep (15);
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
            Stopwatch sw = new Stopwatch();
            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Please enter a complete argument. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length > 3)
            {
                Console.WriteLine(
                    "Please enter only 2 accepted arguments. Accepted form is <program> <file name> <extra copies?: -Y or -N>");
                return;
            }
            else if (args.Length == 3)
            {
                if (args[1] == "-N")
                {
                    FileChunk(args[0], false);
                    Console.WriteLine("***************");
                    Console.WriteLine("*WRITE IS DONE*");
                    Console.WriteLine("***************");
                    System.Threading.Thread.Sleep (2000);
                    sw.Start();
                    while (true)
                    {
                        readchunk(args[0], false);
                        System.Threading.Thread.Sleep (100);
                        if (sw.ElapsedMilliseconds >= Convert.ToInt32(args[2]))
                        {
                            Console.WriteLine("************");
                            Console.WriteLine("*TIME IS UP*");
                            Console.WriteLine("************");
                            break;
                        }
                    }
                    System.Threading.Thread.Sleep (2000);
                    readchunk(args[0], false);
                    System.Threading.Thread.Sleep (500);
                    readchunk(args[0], false);
                    
                }
                else if (args[1] == "-Y")
                {
                    FileChunk(args[0], true);
                    while (true)
                    {
                    }

                    return;
                }
                return;
            }
        }
    }
}