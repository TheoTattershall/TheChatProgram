using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//Stuff I added
using System.Threading;

namespace The_Chat_Program
{

    public partial class FormChat : Form
    {
        List<Person> people = new List<Person>();

        string ip;
        int port;
        string nickname;
        Color nicknameColour;

        Connect connection;

        Thread sendThread;
        Thread receiveThread;

        public FormChat()
        {
            InitializeComponent();
        }

        private void FormChat_Load(object sender, EventArgs e)
        {
            FormMain formMain = new FormMain();
            formMain.ShowDialog();
            this.ActiveControl = textBoxInput;
            //Check if the form was closed or connect button pressed
            if (formMain.connectClicked == false) { Application.Exit(); }
            else
            {
                //If connect button pressed assign to variables and go go go
                ip = formMain.ip;
                port = formMain.port;
                nickname = formMain.nickname;
                nicknameColour = formMain.nicknameColour;

                connection = new Connect(ip, port, nickname);

                //Create threads
                sendThread = new Thread(new ThreadStart(connection.Send));
                receiveThread = new Thread(new ThreadStart(connection.Receive));

                sendThread.IsBackground = true;
                receiveThread.IsBackground = true;

                sendThread.Start();
                receiveThread.Start();

                timerReceive.Enabled = true;

                //Make a request for the user list, sending my nickname
                connection.sendQueue.Add("userList¶" + nickname + "¶" + nicknameColour.ToArgb().ToString());
                //Make a request for the message history
                connection.sendQueue.Add("messageList¶");
            }
        }

        private void timerReceive_Tick(object sender, EventArgs e)
        {
            //If there's nothing in the queue just exit
            if (connection.receiveQueue.Count == 0)
            {
                return;
            }
            //There is a thing in the queue
            string message = connection.receiveQueue.Take();
            int splitPlace = message.IndexOf('¶');
            int splitPlace2 = message.IndexOf('¶', splitPlace + 1);

            string sendName = message.Substring(0, splitPlace);
            string opcode = message.Substring(splitPlace + 1, splitPlace2 - (splitPlace + 1));
            string operand = message.Substring(splitPlace2 + 1);
            switch (opcode)
            {
                case "sendMessage":
                    //Create a message object, then send it to the server
                    Message newMessage = new Message(sendName, operand);

                    messageToUI(newMessage);

                    break;
                case "messageList":
                    //Get the message history from the server
                    List<string> newMessagesString = operand.Split('¶').ToList();

                    bulkAddMessages(newMessagesString);

                    break;
                case "userList":
                    //Get the list of users online, sending the server your nickname in the process
                    if (operand != "")
                    {
                        List<string> newPersonString = operand.Split('¶').ToList();

                        bulkAddPeople(newPersonString);
                    }

                    break;
                case "closeClient":
                    //Close the program with an error
                    Console.WriteLine(operand);
                    FormError formError = new FormError(operand);
                    formError.ShowDialog();
                    Application.Exit();
                    break;
                default:
                    break;
            }
        }

        private void messageToUI(Message message)
        {
            //Find this user's colour
            richTextBoxChat.SelectionStart = richTextBoxChat.Text.Length;
            richTextBoxChat.SelectionColor = Color.Black;
            foreach (Person person in people)
            {
                if (person.name == message.nickname)
                {
                    richTextBoxChat.SelectionColor = person.colour;
                }
            }
            //Append the message
            richTextBoxChat.SelectionFont= new Font(richTextBoxChat.Font, FontStyle.Bold);
            richTextBoxChat.AppendText(message.nickname + ": ");
            richTextBoxChat.SelectionFont = new Font(richTextBoxChat.Font, FontStyle.Regular);
            richTextBoxChat.AppendText(message.message + "\n");
            //Clear the textbox if the message was from me
            if (message.nickname == nickname)
            {
                textBoxInput.Clear();
            }
        }

        private void bulkAddMessages(List<string> newMessagesString)
        {
            richTextBoxChat.Clear();

            for (int i = 0; i < newMessagesString.Count - 2; i += 2)
            {
                var thisMessage = newMessagesString.Skip(i).Take(2).ToArray();
                Message thisMessageObj = new Message(thisMessage[0], thisMessage[1]);

                messageToUI(thisMessageObj);
            }
        }

        private void bulkAddPeople(List<string> newPeopleString)
        {
            people.Clear();

            //Add to the list of people
            for (int i = 0; i < newPeopleString.Count - 3; i += 3)
            {
                var thisPerson = newPeopleString.Skip(i).Take(3).ToArray();
                Person thisPersonObj = new Person(thisPerson[0], Color.FromArgb(Convert.ToInt32(thisPerson[1])), Convert.ToBoolean(thisPerson[2]));

                people.Add(thisPersonObj);
            }

            //Add to richTextBox
            richTextBoxUsers.Clear();

            foreach (Person person in people)
            {
                if (person.isOnline == true)
                {
                    richTextBoxUsers.SelectionColor = person.colour;
                    richTextBoxUsers.AppendText(person.name + "\n");
                }
            }
        }

        private void sendMessage()
        {
            if (textBoxInput.Text != "")
            {
                string messageToSend = "sendMessage¶" + textBoxInput.Text;
                connection.sendQueue.Add(messageToSend);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            textBoxInput.Text = textBoxInput.Text.Replace(Environment.NewLine, "");
            sendMessage();
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBoxInput.Text = textBoxInput.Text.Replace(Environment.NewLine, "");
                sendMessage();
            }
        }

        private void richTextBoxChat_TextChanged(object sender, EventArgs e)
        {
            //Set the current caret position to the end
            richTextBoxChat.SelectionStart = richTextBoxChat.Text.Length;
            //Scroll it automatically
            richTextBoxChat.ScrollToCaret();
        }
    }
}
