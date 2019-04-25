using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Stuff I added
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Drawing;

namespace TCP_Server
{
    class Connect
    {
        private TcpClient client;
        private NetworkStream stream;

        public BlockingCollection<string> sendQueue = new BlockingCollection<string>();

        Thread sendThread;
        Thread receiveThread;

        string nickname = "";

        public Connect(TcpClient myClient)
        {
            client = myClient;

            try
            {
                stream = client.GetStream();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("WARNING: An exception occurred when the network stream was retrieved from the client. Most likely this client has disconnected");
            }

            sendThread = new Thread(new ThreadStart(Send));
            receiveThread = new Thread(new ThreadStart(Receive));

            sendThread.IsBackground = true;
            receiveThread.IsBackground = true;
            
            sendThread.Start();
            receiveThread.Start();
        }

        private void Send()
        {
            string toSend;
            try
            {
                while (true)
                {
                    toSend = sendQueue.Take();
                    //Check if its signaling to close connection
                    if (toSend == "closeConnection")
                    {
                        Console.WriteLine("Sending thread told to close");
                        return;
                    }

                    //Console.WriteLine("Sending: " + toSend);
                    Byte[] data = Encoding.Unicode.GetBytes(toSend);

                    Byte[] length = BitConverter.GetBytes(data.Length);

                    Byte[] sendBytes = new Byte[length.Length + data.Length];
                    length.CopyTo(sendBytes, 0);
                    data.CopyTo(sendBytes, 4);

                    //Console.WriteLine(Encoding.Unicode.GetString(sendBytes));

                    stream.Write(sendBytes, 0, sendBytes.Length);
                    //Console.WriteLine("Sent: " + toSend);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Sending thread closed");
                client.Close();
            }

        }

        private void Receive()
        {
            string response;
            Byte[] receiveByte;

            try
            {
                while (true)
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

                    response = Encoding.Unicode.GetString(receiveByte);

                    response = nickname + "¶" + response;

                    processResponse(response);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Receiving thread closed");
                client.Close();
                //Signal to other thread to close
                sendQueue.Add("closeConnection");

                //Set the isOnline attribute to false
                if (nickname != "")
                {
                    int i = 0;
                    while (i < Program.people.Count)
                    {
                        if (Program.people[i].name == nickname)
                        {
                            Console.WriteLine("Set isOnline to false");
                            Program.people[i].isOnline = false;
                        }
                        i += 1;
                    }
                }

                //Remove from clientList
                Program.clients.Remove(this);

                senduserList();
            }
        }

        private void processResponse(string response)
        {
            //We got the response, now process it
            int splitPlace = response.IndexOf('¶');
            int splitPlace2 = response.IndexOf('¶', splitPlace + 1);

            string sendName = response.Substring(0, splitPlace);
            string opcode = response.Substring(splitPlace + 1, splitPlace2 - (splitPlace + 1));
            string operand = response.Substring(splitPlace2 + 1);
            switch (opcode)
            {
                case "sendMessage":
                    if (sendName != "")
                    {
                        //Log the message, then send it to everyone
                        Message newMessage = new Message(sendName, operand);
                        Program.messages.Add(newMessage);

                        foreach (Connect clientElement in Program.clients)
                        {
                            clientElement.sendQueue.Add(response);
                        }
                    }

                    break;
                case "messageList":
                    if (sendName != "")
                    {
                        //Send the message log to the person who requested it
                        string messagesString = "¶messageList¶";
                        if (Program.messages.Count != 0)
                        {
                            foreach (Message message in Program.messages)
                            {
                                messagesString = messagesString + message.nickname + "¶" + message.message + "¶";
                            }
                        }

                        this.sendQueue.Add(messagesString);
                    }

                    break;
                case "userList":
                    //Take this person's name, then resend the userlist
                    int operandSplitter = operand.IndexOf('¶');

                    string personName = operand.Substring(0, operandSplitter);
                    Color personColour = Color.FromArgb(Convert.ToInt32(operand.Substring(operandSplitter + 1)));

                    Person newPerson = new Person(personName, personColour, true);

                    //Check name against all other names in the server
                    bool nameTaken = false;
                    if (Program.people.Count != 0)
                    {
                        int i = 0;
                        while (i < Program.people.Count)
                        {
                            //Find if name is in the list already
                            if (Program.people[i].name == newPerson.name)
                            {
                                //If they are still online then reject the new connection
                                if (Program.people[i].isOnline == true)
                                {
                                    nameTaken = true;
                                    i++;
                                }
                                //If they are not online simply remove them from the list
                                else
                                {
                                    Program.people.RemoveAt(i);
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }
                        /*
                        foreach (Person person in Program.people)
                        {
                            if (person.name == newPerson.name && person.isOnline == true)
                            {
                                nameTaken = true;
                            }
                        }*/
                    }
                    //Act upon it if the name is invalid, otherwise go ahead and add it to the people list and send the updated list
                    if (nameTaken) { this.sendQueue.Add("¶closeClient¶Name is already taken"); }
                    else if (personName == "") { this.sendQueue.Add("¶closeClient¶Name cannot be blank"); }
                    else
                    {
                        Program.people.Add(newPerson);
                        nickname = newPerson.name;

                        senduserList();
                    }

                    break;
            }
        }

        private void senduserList()
        {
            //Send updated userList to everyone
            string usersString = "¶userList¶";
            if (Program.people.Count != 0)
            {
                foreach (Person person in Program.people)
                {
                    usersString = usersString + person.name + "¶" + person.colour.ToArgb().ToString() + "¶" + person.isOnline.ToString() + "¶";
                }
            }

            foreach (Connect clientElement in Program.clients)
            {
                clientElement.sendQueue.Add(usersString);
            }
        }
    }
}
