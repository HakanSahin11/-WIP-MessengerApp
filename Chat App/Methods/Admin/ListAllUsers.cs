using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods.Admin
{
    public class ListAllUsers
    {
        public ListAllUsers(string content, int id)
        {
            this.content = content;
            this.id = id;
        }

        public string content { get; set; }
        public int id { get; set; }
    }
}
