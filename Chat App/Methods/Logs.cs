using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
   public class Logs
    {
        //Method which shows the results of DB collection "Log" which is former chat history logs / elements / conversations / Picture base64
        public class chatListsMethod
        {
            public static readonly string Name = "Log";

            public chatListsMethod(int _id, List<messages> Chat)
            {
                this._id = _id;
                this.Chat = Chat;
            }

            public int _id { get; set; }
            public List<messages> Chat { get; set; }
        }
        public class messages
        {
            public messages(int id, List<Chats> chatLists)
            {
                _id = id;
                this.chatLists = chatLists;
            }

            public int _id { get; set; }
            public List<Chats> chatLists { get; set; }
        }
        public class Chats
        {
            public Chats(string message, string Timestamp, bool CurrentUser)
            {
                this.message = message;
                this.Timestamp = Timestamp;
                this.CurrentUser = CurrentUser;
            }

            public string message { get; set; }
            public string Timestamp { get; set; }
            public bool CurrentUser { get; set; }
        }
        public class chatMessages
        {
            public chatMessages(string message, DateTime timeStamp, int id)
            {
                this.message = message;
                TimeStamp = timeStamp;
                _id = id;
            }
            public string message { get; set; }
            public DateTime TimeStamp { get; set; }
            public int _id { get; set; }
        }
    }

}
