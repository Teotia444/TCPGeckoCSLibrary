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
                    connected = false;
                    Console.WriteLine("force disconnect");
                    Disconnect();
                    break;
                }

                results[queueElement.Item2] = result;
            }
        }

        public bool Connect(string ipAddress, bool _debug = false)
        {
            debug = _debug;
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddress), 7332);
                client = new Socket(
                      ip.AddressFamily,
                      SocketType.Stream,
                      ProtocolType.Tcp);

                var result = client.BeginConnect(ip, null, null);
                if(!result.AsyncWaitHandle.WaitOne(3000, true))
                {
                    client.Close();
                    return false;
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            connected = true;
            commandHandler = new Thread(CommandHandler);
            commandHandler.Start();
            return true;
        }

        public void Disconnect()
        {
            if (!client.Connected) return;
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(true);
            connected = false;
            commandHandler.Join();
            commands.Clear();
            results.Clear();
        }

        public string Peek(Datatype type, int address)
        {
            if (!connected) return "Disconnected";
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
            while (results[commandId] == "" && connected)
            {
                
            }
            if (!connected) return "Disconnected";
            var result = results[commandId];
            results.Remove(commandId);
            return result;
        }

        public void Poke(Datatype type, int address, uint value)
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {
                
            }
            if (!connected) return;
            results.Remove(commandId);
            return;
        }
        public void Poke(Datatype type, int address, int value)
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {

            }
            if (!connected) return;
            results.Remove(commandId);
            return;
        }

        public List<string> PeekMultiple(Datatype type, int[] addresses)
        {
            if (!connected) return Enumerable.Range(1, 16).Select(_ => "0").ToList();
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
            while (results[commandId] == "" && connected)
            {
                
            }
            if (!connected) return Enumerable.Range(1, 16).Select(_ => "0").ToList();
            var result = results[commandId].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            results.Remove(commandId);
            return result;
        }

        public void PokeMultiple(Datatype type, int[] addresses, uint[] values)
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {
                
            }
            results.Remove(commandId);
            return;
        }

        public void PauseExec()
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {

            }
            if (!connected) return;
            results.Remove(commandId);
            return;
        }

        public void AdvanceExec()
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {

            }

            results.Remove(commandId);
            return;
        }

        public void ResumeExec()
        {
            if (!connected) return;
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
            while (results[commandId] == "" && connected)
            {

            }
            if (!connected) return;
            results.Remove(commandId);
            return;
        }
        public void DisplayText(string text, int r, int g, int b, int a)
        {
            if (!connected) return;
            if (client == null)
            {
                Console.WriteLine("Not Connected to wii u");
                return;
            }
            var message = "drawtext -text ("+ text +") -a "+ a +" -r "+ r + " -g "+ g +"-b " + b;
            var commandId = Guid.NewGuid().ToString();

            commands.Enqueue((message, commandId));
            results[commandId] = "";
            if (debug) Console.WriteLine(message);
            while (results[commandId] == "" && connected)
            {

            }
            if (!connected) return;
            results.Remove(commandId);
            return;
        }

    }
}
