using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The_Chat_Program
{
    public partial class FormMain : Form
    {
        public string ip;
        public int port;
        public string nickname;
        public Color nicknameColour = Color.Black;

        public bool connectClicked = false;
        

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connectClicked)
            {
                ip = textBoxIP.Text;
                port = Convert.ToInt32(numericUpDownPort.Value);
                nickname = textBoxName.Text;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            connectClicked = true;
            this.Close();
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            //Check if it contains escape character
            if (textBoxName.Text.Contains("¶"))
            {
                textBoxName.Text = textBoxName.Text.Replace("¶", "");
            }
            
            //Check disable/enable connect button
            if (buttonConnect.Enabled == true && textBoxName.Text == "")
            {
                buttonConnect.Enabled = false;
            }
            else if (buttonConnect.Enabled == false && textBoxName.Text != "")
            {
                buttonConnect.Enabled = true;
            }
        }

        private void buttonColour_Click(object sender, EventArgs e)
        {
            colorDialogMain.Color = nicknameColour;
            if (colorDialogMain.ShowDialog() == DialogResult.OK)
            {
                nicknameColour = colorDialogMain.Color;
                buttonColour.ForeColor = nicknameColour;
            }
        }
    }
}
