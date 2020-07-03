using Chat_App.Methods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Chat_App.Methods.Api_elements.APICallReq;
using static Chat_App.Methods.Controls.Templates;
using Chat_App.Methods.Admin;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using MongoDB.Bson;
using System.Collections;
using System.Net.Http;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for Admin_Panel.xaml
    /// </summary>
    public partial class Admin_Panel : Window
    {
        public Admin_Panel()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            inputSetup();
        }

        //backup
        public List<ListAllUsers> listAllUsers = new List<ListAllUsers>();
        public List<ListAllUsers> ViewlistAllUsers = new List<ListAllUsers>();
        public void inputSetup()
        {
            //fill list of all
            UserList();
            var liste = new List<ListAllUsers>();
            if(ViewlistAllUsers.Count == 0)
            {
                liste = listAllUsers;
            }
            else
            {
                liste = ViewlistAllUsers;
            }
            foreach (var content in liste)
            {
                var btn = buttonSetup(content.content, 16, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Left);
                btn.Name = $"ListAll{content.id}";
                btn.Click += Btn_Click;
                SPUsers.Children.Add(btn);
            }
            CBUserType.Items.Add("User");
            CBUserType.Items.Add("Administrator");
            CBUserType.SelectedIndex = 0;
            CBGender.Items.Add("M");
            CBGender.Items.Add("F");
            CBGender.Items.Add("Not Telling");
            CBGender.SelectedIndex = 3;

        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            var id = Convert.ToInt32(Regex.Replace(btn.Name, "[^0-9.]", ""));
            ClickSetup(id);
        }
        public void ClickSetup(int id)
        {
            var userClass = UserList().Where(x => x.Any(y => y._id == id)).First();
            foreach (var item in userClass)
            {
                LBID.Content = item._id;
                CBUserType.SelectedIndex = CBUserType.Items.IndexOf($"{item.UserType}");
                txtEmail.Text = item.Email;
                txtJobTitle.Text = item.JobTitle;
                txtFirstName.Text = item.FirstName;
                txtLastName.Text = item.LastName;
                txtCountry.Text = item.Country;
                txtCity.Text = item.City;
                txtAddress.Text = item.Address;
                txtAge.Text = item.Age.ToString();
                CBGender.SelectedIndex = CBGender.Items.IndexOf($"{item.Gender}");
                txtLoginBan.Text = item.LoginBan;
            }
        }

        public void APIPost(string json, string usage)
        {

            HttpWebRequest httpWebRequest;
            StreamReader reader;
            if (usage == "Create")
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/CreateUser");
                reader = streamReader(json, httpWebRequest);
            }
            else if(usage == "Delete")
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/User");
                reader = streamReader(json, httpWebRequest);
            }
            else if(usage == "Save")
            {
                //todo make a new controller for this so i can just send over the ready to go json
                
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/Admin");
                reader = streamReader(json, httpWebRequest);
 
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/User");
                reader = streamReader(json, httpWebRequest);
            }
            
            if (reader.ReadToEnd() == "Success")
            {
                MessageBox.Show("Mask Successfully Completed!");
            }
            else
            {
                MessageBox.Show("Error creating user! Error code 500");
            }
            reader.Close();
            
        }

        public List<List<UserClass>> UserList()
        {
            // gets a list of all users in the database, then puts in the ID and full name of the users into a seperate list to be used in the stackpanel overview
            listAllUsers.Clear();
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/user"));
            List<List<UserClass>> list = JsonConvert.DeserializeObject<List<List<UserClass>>>(jsonStr);
            var listAll = new List<ListAllUsers>();
            list.ToList().ForEach(x => x.ForEach(y => listAll.Add(new ListAllUsers($"{y.FirstName} {y.LastName}", y._id))));
            listAllUsers = listAll.OrderBy(x => x.content).ThenBy(y => y.id).ToList();
            return list;
        }

        private void btnLogOut_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginSite loginSite = new LoginSite();
            loginSite.Show();
            Close();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (Validator("Create") == true) {
                UserList();
                //Json string for the new user
                string jsonStr = $"{{ " +
                    $"\"Email\"     : \"{txtEmail.Text}\", " +
                    $"\"Password\"  : \"{txtPassword.Text}\", " +
                    $"\"FirstName\" : \"{txtFirstName.Text}\", " +
                    $"\"LastName\"  : \"{txtLastName.Text}\", " +
                    $"\"Gender\"    : \"{CBGender.Text}\", " +
                    $"\"Country\"   : \"{txtCountry.Text}\", " +
                    $"\"City\"      : \"{txtCity.Text}\", " +
                    $"\"Address\"   : \"{txtAddress.Text}\", " +
                    $"\"JobTitle\"  : \"{txtJobTitle.Text}\", " +
                    $"\"UserType\"  : \"{CBUserType.Text}\", " +
                    $"\"Age\"       : \"{txtAge.Text}\"  " +
                    $"}}";
                APIPost(jsonStr, "Create");
            }

            else
            {

            }
        }
        public bool Validator(string usage)
        {
            //validates if all info is in correct format
            List<string> errors = new List<string>();
            var list = new List<Control>
            {
                CBUserType,
                txtEmail,
                txtPassword,
                txtJobTitle,
                txtFirstName,
                txtLastName,
                txtCountry,
                txtCity,
                txtAddress,
                txtAge,
                CBGender,
                txtLoginBan
            };
            if (!txtEmail.Text.Contains("@"))
            {
                errors.Add("Error at Email! Missing @");
            }
            bool missingDetails = false;
            foreach (var item in list)
            {
                if (item is TextBox)
                {
                    if ( (((TextBox)item).Name != "txtAge" && ( ((TextBox)item).Text == string.Empty || ((TextBox)item).Text.Length >= 3)) 
                     && (((TextBox)item).Name != "txtPassword" && usage == "Create" ) )
                    {
                            missingDetails = true;
                    }
                    else if (((TextBox)item).Name == "txtLoginBan")
                    {
                        try
                        {
                            var match = DateTime.Parse(txtLoginBan.Text);
                        }
                        catch (Exception)
                        {
                            errors.Add("Error at Loginban! Please only use datetime format!");
                        }
                    }
                    else if( ((TextBox)item).Name == "txtAge")
                    {
                        try
                        {
                            var match = int.Parse(txtAge.Text);
                        }
                        catch (Exception)
                        {
                            errors.Add("Error at Age! Only use numers");
                        }
                    }
                }
            }
            if (missingDetails)
            {
                errors.Add("Error! Missing characters / fields not filled! At least use 3 characters!");
            }
            if (errors.Count == 0)
            {
                return true;
            }
            MessageBox.Show(string.Join(Environment.NewLine, errors));
            return false;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Validator("Save"))
            {
                //makes makes list of old User info and the new one by the textboxes, where all existing user info will be removed from the changes list
                var oldUser = UserList().Where(x => x.Any(y => y._id == Convert.ToInt32(LBID.Content))).First().First();
                var oldUserStr = new List<string>
                {
                    oldUser.FirstName,
                    oldUser.LastName,
                    oldUser.UserType,
                    oldUser.Email,
                    oldUser.JobTitle,
                    oldUser.Country,
                    oldUser.City,
                    oldUser.Address,
                    Convert.ToString(oldUser.Age),
                    oldUser.Gender.ToString(),
                    oldUser.LoginBan
                };
                var NewUserStr = new Dictionary<string, int>
                {
                    { txtFirstName.Text, 0 },
                    { txtLastName.Text, 1 },
                    { CBUserType.Text, 2 },
                    { txtEmail.Text, 3 },
                    { txtJobTitle.Text, 4 },
                    { txtCountry.Text, 5 },
                    { txtCity.Text, 6 },
                    { txtAddress.Text, 7 },
                    { txtAge.Text, 8 },
                    { CBGender.Text, 9 },
                    { txtLoginBan.Text, 10 }
                };
                foreach (var item in oldUserStr)
                {
                    NewUserStr.Remove(item);
                }

                //Converts the list with changes to JSON format (which can be read by the MongoDB in current format)
                //and makes it ready to be sent to the API to make the following changes
                string changesToUser = $"{{";
                for (int i = 0; i < NewUserStr.Count; i++)
                {
                 changesToUser += $"{Value(NewUserStr.ElementAt(i).Key, NewUserStr.ElementAt(i).Value)},";
                }
                changesToUser += $" \"_id\" : {oldUser._id}";
                changesToUser += $"}}";
                APIPost(changesToUser, "Save");
            }
        }
        public string Value(string content, int value)
        {
            //literates through the switchstatement to find the case of the value (to determind if its example FirstName, to not mix it)
            string result = "";
            switch (value)
            {
                case 0:
                    result = $"\"FirstName\" : \"{content}\"";
                    break;
                case 1:
                    result = $"\"LastName\" : \"{content}\"";
                    break;
                case 2:
                    result = $"\"UserType\" : \"{content}\"";
                    break;
                case 3:
                    result = $"\"Email\" : \"{content}\"";
                    break;
                case 4:
                    result = $"\"JobTitle\" : \"{content}\"";
                    break;
                case 5:
                    result = $"\"Country\" : \"{content}\"";
                    break;
                case 6:
                    result = $"\"City\" : \"{content}\"";
                    break;
                case 7:
                    result = $"\"Address\" : \"{content}\"";
                    break;
                case 8:
                    result = $"\"Age\" : \"{content}\"";
                    break;
                case 9:
                    result = $"\"Gender\" : \"{content}\"";
                    break;
                case 10:
                    result = $"\"LoginBan\" : \"{content}\"";
                    break;
            }
                    return result;
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ClickSetup(Convert.ToInt32(LBID.Content));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you wish to delete current user?!", "Confirmation", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                string jsonStr = $"{{ \"id\" : {LBID.Content}, \"usage\" : \"Delete\" }}";
                APIPost(jsonStr, "Delete");
                SPUsers.Children.Clear();
                inputSetup();
            }
            else { return; }
        }

        private void txtSelectUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            SPUsers.Children.Clear();
            ViewlistAllUsers = listAllUsers.Where(X => X.content.StartsWith(txtSelectUser.Text, true, CultureInfo.InvariantCulture)).ToList();
            inputSetup();
        }
    }
}
