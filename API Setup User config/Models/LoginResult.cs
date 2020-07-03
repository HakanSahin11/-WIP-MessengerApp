using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    public class LoginResult
    {
        public LoginResult(bool result, string userType)
        {
            this.result = result;
            this.userType = userType;
        }

        public bool result { get; set; }
        public string userType { get; set; }
    }
}
