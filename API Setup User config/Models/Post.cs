using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    public partial class Post
    {
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
