using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    //class is being used for the salt values of the database (in a different collection from the rest)
    public class SaltClass
    {
        public static readonly string Name = "System";

        public SaltClass(int id, string Salt)
        {
            _id = id;
            this.Salt = Salt;
        }

        public int _id {get;set;}
        public string Salt { get; set; }
    }
}
