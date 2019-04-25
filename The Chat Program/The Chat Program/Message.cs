using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Chat_Program
{
    class Message
    {
        public string nickname;
        public string message;

        public Message(string myNickname, string myMessage)
        {
            nickname = myNickname;
            message = myMessage;
        }
    }
}
