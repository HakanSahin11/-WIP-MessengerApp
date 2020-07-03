using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
    public class FriendsListMethod
    {
        //Method for mainclient's (User client) visible friendslist on StackPanelChatHistory
        public FriendsListMethod(string names, int id)
        {
            this.Names = names;
            this.Id = id;
        }

       public string Names { get; set; }
       public int Id { get; set; }
    }
}
