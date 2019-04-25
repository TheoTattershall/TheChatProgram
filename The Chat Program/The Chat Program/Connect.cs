using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//Extra stuff
using System.IO;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Drawing;

namespace The_Chat_Program
{
    class Connect
    {
        private TcpClient client;
        private NetworkStream stream;

        public BlockingCollection<string> sendQueue = new BlockingCollection<string>();
        public BlockingCollection<string> receiveQueue = new BlockingCollection<string>();

        string ip;
        int port;
        string nickname;

        bool errorShown = false;

        public Connect(string myIp, int myPort, string myNickname)
        {
            ip = myIp;
            port = myPort;
            nickname = myNickname;

            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
            }
            catch (SocketException)
            {
                raiseError("Could not connect to server");
            }
            catch (InvalidOperationException)
            {
                raiseError("Could not connect to server");
            }
        }

        public void Send()
        {
            string toSend;

            try
            {
                while (true)
                {
                    toSend = sendQueue.Take();
                    //toSend = nickname + "¶" + toSend;
                    Byte[] data = Encoding.Unicode.GetBytes(toSend);

                    Byte[] length = BitConverter.GetBytes(data.Length);

                    Byte[] sendBytes = new Byte[length.Length + data.Length];
                    length.CopyTo(sendBytes, 0);
                    data.CopyTo(sendBytes, 4);

                    stream.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (IOException)
            {
                raiseError("Something went wrong while sending data");
                return;
            }
            catch (ObjectDisposedException)
            {
                raiseError("Something went wrong while sending data");
                return;
            }
            catch (NullReferenceException)
            {
                raiseError("Something went wrong while sending data");
                return;
            }
        }

        public void Receive()
        {
            string response;
            Byte[] receiveByte;

            while (true)
            {
                response = "";

                try
                {
                    //Get the length first
                    Byte[] byteLength = new Byte[4];
                    stream.Read(byteLength, 0, 4);
                    int length = BitConverter.ToInt32(byteLength, 0);

                    //Now get the data
                    List<Byte[]> responseList = new List<byte[]>();
                    //Keep reading from the stream until we have the whole message
                    int bytesLeft = length;
                    int received;
                    while (bytesLeft > 0)
                    {
                        Byte[] byteResponse = new Byte[length];
                        received = stream.Read(byteResponse, 0, bytesLeft);
                        bytesLeft -= received;
                        Array.Resize(ref byteResponse, received);

                        responseList.Add(byteResponse);
                    }
                    //Finally, put it all into one byte
                    receiveByte = new Byte[length];
                    int currentPosition = 0;
                    foreach (Byte[] responsePiece in responseList)
                    {
                        responsePiece.CopyTo(receiveByte, currentPosition);
                        currentPosition += responsePiece.Length;
                    }

                }
                catch (IOException)
                {
                    raiseError("Something went wrong while receiving data");
                    return;
                }
                catch (ObjectDisposedException)
                {
                    raiseError("Something went wrong while receiving data");
                    return;
                }
                catch (NullReferenceException)
                {
                    raiseError("Something went wrong while receiving data");
                    return;
                }

                response = Encoding.Unicode.GetString(receiveByte);
                receiveQueue.Add(response);
            }
        }

        private void raiseError(string errorDescription)
        {
            if (errorShown == false)
            {
                errorShown = true;
                Console.WriteLine(errorDescription);
                FormError formError = new FormError(errorDescription);
                formError.ShowDialog();
                Application.Exit();
            }
        }
    }
}
