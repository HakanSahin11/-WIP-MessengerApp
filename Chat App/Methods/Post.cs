using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
   public class Post
    {
        //Method used for matching login informations
        public Post(string email, string match)
        {
            this.email = email;
            this.match = match;
        }

        public string email { get; set; }
        public string match { get; set; }
    }

    public class PostReturnUsers
    {
        public PostReturnUsers(string json, int id)
        {
            this.json = json;
            this.id = id;
        }

        public string json { get; set; }
        public int id { get; set; }
    }
}
