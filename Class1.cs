using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace TCPGeckoAromaLibrary
{
    public class TCPGecko
    { 
        public enum Datatype
        {
            u8,
            u16,
            u32,
            f32
        }

        public bool connected;
        public Socket client;
        public bool debug;
        public List<string> busy = new List<string>();
        
        public void Connect(string ipAddress, bool _debug = false)
        {
            busy.Clear();
            debug = _debug;

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddress), 7332);
            client = new Socket(
                  ip.AddressFamily,
                  SocketType.Stream,
                  ProtocolType.Tcp);

            client.Connect(ip);
            connected = true;
            return;
        }

        public void Disconnect()
        {
            busy.Clear();
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(true);
            connected = false;
        }

        public string Peek(Datatype type, int address)
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return "Error";
            }
            busy.Add(address.ToString());

            while (busy[0] != address.ToString())
            {
                //do nothing while address not on top of queue
            }

            var message = "peek -t " + type + " -a " + $"0x{address:X}";
            
            if(debug) Console.WriteLine(message);
            
            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);

            string result = "";
            do
            {
                int numberOfBytesReceived = client.Receive(messageBytes);
                string t = Encoding.UTF8.GetString(messageBytes, 0, numberOfBytesReceived);
                result += t;
            } while (client.Available > 0);

            busy.RemoveAt(0);
            return result;
        }

        public void Poke(Datatype type, int address, int value)
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }
            busy.Add(address.ToString());

            while (busy[0] != address.ToString())
            {
                //do nothing while address not on top of queue
            }

            var message = "poke -t " + type + " -a " + $"0x{address:X}" + " -v " + $"0x{value:X}";

            if (debug) Console.WriteLine(message);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);
            string result = "";

            do
            {
                int numberOfBytesReceived = client.Receive(messageBytes);
                string t = Encoding.UTF8.GetString(messageBytes, 0, numberOfBytesReceived);
                result += t;
            } while (client.Available > 0);

            busy.RemoveAt(0);
            return;
        }

        public void PauseExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }

            busy.Add("Pause");

            while (busy[0] != "Pause")
            {
                //do nothing while instruction not on top of queue
            }

            var message = "pause ";

            if (debug) Console.WriteLine(message);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);
            string result = "";

            do
            {
                int numberOfBytesReceived = client.Receive(messageBytes);
                string t = Encoding.UTF8.GetString(messageBytes, 0, numberOfBytesReceived);
                result += t;
            } while (client.Available > 0);

            busy.RemoveAt(0);
            return;
        }

        public void AdvanceExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }

            busy.Add("Advance");

            while (busy[0] != "Advance")
            {
                //do nothing while instruction not on top of queue
            }

            var message = "advance ";

            if (debug) Console.WriteLine(message);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);
            string result = "";

            do
            {
                int numberOfBytesReceived = client.Receive(messageBytes);
                string t = Encoding.UTF8.GetString(messageBytes, 0, numberOfBytesReceived);
                result += t;
            } while (client.Available > 0);

            busy.RemoveAt(0);
            return;
        }

        public void ResumeExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }

            busy.Add("Resume");

            while (busy[0] != "Resume")
            {
                //do nothing while instruction not on top of queue
            }

            var message = "resume ";

            if (debug) Console.WriteLine(message);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);
            string result = "";

            do
            {
                int numberOfBytesReceived = client.Receive(messageBytes);
                string t = Encoding.UTF8.GetString(messageBytes, 0, numberOfBytesReceived);
                result += t;
            } while (client.Available > 0);

            busy.RemoveAt(0);
            return;
        }

    }
}
