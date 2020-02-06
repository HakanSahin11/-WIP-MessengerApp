using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
   public class Post
    {
        public Post(string email, string match)
        {
            this.email = email;
            this.match = match;
        }

        public string email { get; set; }
        public string match { get; set; }
    }
}
