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
    public partial class FormError : Form
    {
        string errorMessage;

        public FormError(string myErrorMessage)
        {
            errorMessage = myErrorMessage;
            InitializeComponent();
        }

        private void FormError_Load(object sender, EventArgs e)
        {
            labelMessage.Text = errorMessage;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
