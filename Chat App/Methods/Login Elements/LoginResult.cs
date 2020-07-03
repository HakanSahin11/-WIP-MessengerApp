using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods.Login_Elements
{
    public class LoginResult
    {
        //Method for checking if the login was successfull and then if it needs to ask which panel to redirect to (Admin- or Userpanel)
        public LoginResult(bool result, string userType)
        {
            this.result = result;
            this.userType = userType;
        }

        public bool result { get; set; }
        public string userType { get; set; }
    }
}
