using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    public class Pass_Model
    {
        public Pass_Model(string hashSalt, string salt)
        {
            this.hashSalt = hashSalt;
            this.salt = salt;
        }

        public string hashSalt { get; set; }
        public string salt     { get; set; }
    }
}
