using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_App.Methods
{
    public class UserClass
    {
        public UserClass(int id, string email, string password, int[] friendsList, int[] incFriendReq, int[] sentFriendReq, string userType, string firstName, string lastName, char gender, string country, string city, string address, string jobTitle, int age, string loginBan)
        {
            _id = id;
            Email = email;
            Password = password;
            FriendsList = friendsList;
            IncFriendReq = incFriendReq;
            SentFriendReq = sentFriendReq;
            UserType = userType;
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            Country = country;
            City = city;
            Address = address;
            JobTitle = jobTitle;
            Age = age;
            LoginBan = loginBan;
        }

        public int _id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int[] FriendsList { get; set; }
        public int[] IncFriendReq { get; set; }
        public int[] SentFriendReq { get; set; }
        public string UserType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public char Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string JobTitle { get; set; }
        public int Age { get; set; }
        public string LoginBan { get; set; }
    }
}
