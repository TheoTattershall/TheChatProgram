using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Stuff I added
using System.Drawing;

namespace The_Chat_Program
{
    class Person
    {
        public string name;
        public Color colour;
        public bool isOnline;

        public Person(string myName, Color myColour, bool myIsOnline)
        {
            name = myName;
            colour = myColour;
            isOnline = myIsOnline;
        }
    }
}
