using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
    public class FriendsListMethod
    {
        public FriendsListMethod(string names, int id)
        {
            this.names = names;
            this.id = id;
        }

        string names { get; set; }
        int id { get; set; }
    }
}
