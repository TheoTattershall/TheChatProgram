using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Stuff I added
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCP_Server
{
    class Program
    {
        static int packetSize = 1024;

        public static List<Connect> clients = new List<Connect>();
        public static List<Message> messages = new List<Message>();
        public static List<Person> people = new List<Person>();

        static void Main(string[] args)
        {
            TcpListener server= null;

            clients = new List<Connect>();

            //Console.WriteLine(GetLocalIPAddress());

            //IP entry
            string internalIP = GetLocalIPAddress();
            if (internalIP != "") { Console.Write("Server IP [" + GetLocalIPAddress() + "]: "); }
            else { Console.Write("Server IP: "); }

            string ip = Console.ReadLine();
            if (ip == "") { ip = internalIP; }

            //Port entry
            Console.Write("Server port [1273]: ");
            string portString = Console.ReadLine();
            if (portString == "") { portString = "1273"; }

            int port;
            try { port = Convert.ToInt32(portString); }
            catch (FormatException)
            {
                Console.WriteLine("Port needs to be a number");
                return;
            }
            
            //Create server
            try
            {
                IPAddress myAddress = IPAddress.Parse(ip);

                server = new TcpListener(myAddress, port);
                server.Start();

                Console.WriteLine("The Chat Program server is online on " + ip + ":" + port.ToString());

                //Start loop - creates a new thread for each user
                while (true)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected to client");

                        Connect connection = new Connect(client);
                        clients.Add(connection);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("WARNING: An exception occurred when a client tried to connect");
                    }
                }

            }
            catch (ArgumentNullException)
            {
                throwError("ERROR: The IP cannot be left blank");
            }
            catch (ArgumentOutOfRangeException)
            {
                throwError("ERROR: The port must be between " + IPEndPoint.MinPort.ToString() + " and " + IPEndPoint.MaxPort.ToString());
            }
            catch (SocketException)
            {
                throwError("ERROR: Could not start server on this IP and port");
            }
            finally
            {
                server.Stop();
            }

        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        private static void threadUser(object obj)
        {
            TcpClient client = (TcpClient)obj;

            Byte[] bytes = new byte[packetSize];
            string data;

            data = null;

            NetworkStream stream = client.GetStream();

            while (true)
            {
                try { stream.Read(bytes, 0, packetSize); }
                catch (System.IO.IOException)
                {
                    client.Close();
                    return;
                }
                
                data = Encoding.ASCII.GetString(bytes).Trim();

                Console.WriteLine(data);
            }
        }

        private static void throwError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
