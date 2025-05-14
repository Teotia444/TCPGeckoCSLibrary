using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public Queue<(string, string)> commands = new Queue<(string, string)>();
        public Dictionary<string, string> results = new Dictionary<string, string>();
        Thread commandHandler;

        

        public void CommandHandler()
        {
            while (connected)
            {
                if (commands.Count == 0) continue;
                var queueElement = commands.Dequeue();
                var message = queueElement.Item1;

                byte[] msg = Encoding.UTF8.GetBytes(message);
                byte[] bytes = new byte[256];
                string result = "";
                try
                {
                    // Blocks until send returns.
                    int byteCount = client.Send(msg, 0, msg.Length, SocketFlags.None);

                    // Get reply from the server.
                    byteCount = client.Receive(bytes, 0, bytes.Length, SocketFlags.None);

                    if (byteCount > 0)
                    {
                        result = Encoding.UTF8.GetString(bytes, 0, byteCount);
                    }
                    

                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                    continue;
                }

                results[queueElement.Item2] = result;
            }
        }

        public void Connect(string ipAddress, bool _debug = false)
        {
            debug = _debug;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddress), 7332);
            client = new Socket(
                  ip.AddressFamily,
                  SocketType.Stream,
                  ProtocolType.Tcp);

            client.Connect(ip);
            connected = true;
            commandHandler = new Thread(CommandHandler);
            commandHandler.Start();
            return;
        }

        public void Disconnect()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(true);
            connected = false;
            commandHandler.Join();
        }

        public string Peek(Datatype type, int address)
        {
            if(address < 0x10000000)
            {
                return "Invalid";
            }
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return "Error";
            }

            var message = "peek -t " + type + " -a " + $"0x{address:X}";
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return results[commandId];
        }

        public void Poke(Datatype type, int address, int value)
        {
            if (address < 0x10000000)
            {
                return;
            }
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }

            var message = "poke -t " + type + " -a " + $"0x{address:X}" + " -v " + $"0x{value:X}";
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return;
        }

        public List<string> PeekMultiple(Datatype type, int[] addresses)
        {
            foreach (int address in addresses)
            {
                if (address < 0x10000000)
                {
                    return new List<string>();
                }
            }

            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return new List<string>();
            }

            var message = "peekmultiple -t " + type;
            foreach (var addr in addresses)
            {
                message += " -a " + $"0x{addr:X}";
            }
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }

            return results[commandId].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void PokeMultiple(Datatype type, int[] addresses, int[] values)
        {
            foreach (int address in addresses)
            {
                if (address < 0x10000000)
                {
                    return;
                }
            }
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }

            var message = "pokemultiple -t " + type;

            for(int i = 0; i < addresses.Length; i++)
            {
                message += " -a " + addresses[i] + " -v " + values[i];
            }

            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return;
        }

        public void PauseExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }
            var message = "pause";
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return;
        }

        public void AdvanceExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }
            var message = "advance";
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return;
        }

        public void ResumeExec()
        {
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }
            var message = "resume";
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "")
            {

            }
            return;
        }

    }
}
