using Chat_App.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Chat_App.Methods.labels;
using static Chat_App.Methods.Login_Elements.Countries_List;
using Path = System.Windows.Shapes.Path;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for LoginSite.xaml
    /// </summary>
    /// 
    /// connct to bson API with the request / timer ban coming from this application
    /// Next step is Sign Up, combine it with the stackpanel

    public partial class LoginSite : Window
    {
        public static string username;
        public static string pw;
        public List<string> RegisteredNames = new List<string>();
        public LoginSite()
        {
            InitializeComponent();
            SignInSetup();
        }

        public void SignInSetup()
        {
            StackPanelField.Children.Clear();
            ClearRegisteredNames();
            StackPanelLabels.Visibility = Visibility.Hidden;
            StackPanelLabels.Children.Clear();

            //LB Heading
            /* Label lb = Labels("Chat Login Site", 14, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center);
             lb.Style = this.FindResource("ControlBaseSpacing") as Style;
             StackPanelField.Children.Add( lb);
             */
            LBHeading.Text = "Login Site";


            //LB Username 
            var lb2 = Labels("Username / Email", 13, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center);
            StackPanelField.Children.Add(lb2);
            
            //Textbot Username 
            TextBox txt = TextBoxes("Enter Username / Email", 13, FontStyles.Normal, Brushes.Black);
            txt.PreviewMouseDown += ClearBox_MouseDown;
            //Registers the name so it can be called by StackPanelField.FindName("txtUserName")
            RegisterName("txtUserName", txt);
            RegisteredNames.Add("txtUserName");
            StackPanelField.Children.Add(txt);

            //LB Password 
            var lb3 = Labels("Password", 13, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center); 
            StackPanelField.Children.Add(lb3);

            //PasswordBox Password 
            PasswordBox pwBox = new PasswordBox();
            pwBox.Password = "Random Template Password Random Password";
            pwBox.PreviewMouseDown += ClearBox_MouseDown;
            pwBox.Style = this.FindResource("ControlBaseSpacing") as Style;
            pwBox.VerticalAlignment = VerticalAlignment.Top;
            pwBox.PreviewMouseDown += ClearBox_MouseDown;
            RegisterName("PasswordBox", pwBox);
            RegisteredNames.Add("PasswordBox");

            StackPanelField.Children.Add(pwBox);

            //Btn Sign In
            var btn = Buttons("Sign In", 13, Brushes.Black, Brushes.LightGray, btnSignIn_Click);
            btn.Style = this.FindResource("ControlBaseSpacing") as Style;
            StackPanelField.Children.Add(btn);

            //LB No user, create one
            var lb4 = Labels("No User? Create one here!", 10, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center); ;
            StackPanelField.Children.Add(lb4);

            //Btn Create new account
            var btn2 = Buttons("Create User", 13, Brushes.Black, Brushes.LightGray, SignUp_Click);
            StackPanelField.Children.Add(btn2);
        }
        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpSetup();
        }

        public void SignUpSetup()
        {
            StackPanelField.Children.Clear();
            ClearRegisteredNames();
            StackPanelLabels.Visibility = Visibility.Visible;

            //LB Heading
            /* Label lb = Labels("Chat Login Site", 14, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center);
             lb.Style = this.FindResource("ControlBaseSpacing") as Style;
             StackPanelField.Children.Add(lb); */
            LBHeading.Text = "Create User";
            
            /*Grid1.Children.Remove(arrow);

            Label Arr = arrow;
            Arr.Visibility = Visibility.Visible;
            Arr.PreviewMouseDown += Arr_PreviewMouseDown;
            StackPanelField.Children.Add(Arr);
            */
            sections("Email", "txt");
            sections("Password", "txt");
            sections("FirstName", "txt");
            sections("LastName", "txt");
            sections("Gender", "Gender");
            sections("Country", "Countries");
            sections("City", "txt");
            sections("Address", "txt");
            sections("JobTitle", "txt");
            sections("Age", "txt");
            Button btn = Buttons("Confirm", 14, Brushes.Black, Brushes.LightGray, CreateUser_Click);
            RegisterName("CreateUserBtn", btn);
            RegisteredNames.Add("CreateUserBtn");

            StackPanelField.Children.Add(btn);

            //Next Step
        }

        private void Arr_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (StackPanelLabels.Visibility == Visibility.Visible)
            {
                SignInSetup();
            }
            else
            {
                SignUpSetup();
            }
        }

        public void sections(string content, string usage)
        {
           // StackPanel sp = new StackPanel();
            //Heading
            Label lb2 = Labels($"{content}", 11, FontStyles.Normal, Brushes.Transparent, Brushes.White, HorizontalAlignment.Center);
            //sp.Children.Add(lb2);
            StackPanelLabels.Children.Add(lb2);
            switch (usage)
            {
                //input
                case "txt":

                    TextBox txt = TextBoxes("", 13, FontStyles.Normal, Brushes.Black);
                    txt.Style = this.FindResource("ControlBaseSpacing") as Style;
                    RegisterName($"{content}", txt);
                    RegisteredNames.Add(content);

                   // sp.Children.Add(txt);
                    StackPanelField.Children.Add(txt);

                    break;

                case "Gender":
                    ComboBox cb = new ComboBox();
                    cb.Items.Add("Not Telling");
                    cb.Items.Add("Male");
                    cb.Items.Add("Female");
                    cb.Items.Add("Other");
                    cb.SelectedIndex = 0;
                    cb.Style = this.FindResource("ControlBaseSpacing") as Style;
                    RegisterName($"{content}", cb);
                    RegisteredNames.Add(content);

                    //    sp.Children.Add(cb);
                    StackPanelField.Children.Add(cb);

                    break;

                case "Countries":
                    ComboBox cb2 = new ComboBox();
                    foreach (var item in Countries)
                    {
                        cb2.Items.Add(item);
                    }
                    cb2.Style = this.FindResource("ControlBaseSpacing") as Style;
                    RegisterName($"{content}", cb2);
                    RegisteredNames.Add(content);

                    //    sp.Children.Add(cb2);
                    StackPanelField.Children.Add(cb2);

                    break;

            }
       //     StackPanelField.Children.Add(sp);
            ScrollView.Visibility = Visibility.Visible;
        }

        void ClearRegisteredNames()
        {
            foreach (var item in RegisteredNames)
            {
                UnregisterName(item);
            }
            RegisteredNames.Clear();
        }
        void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            //Json string for the new user
            string jsonStr = $"{{ " +
                $"\"Email\"     : \"{(this.FindName("Email")     as TextBox).Text}\"," +
                $"\"Password\"  : \"{(this.FindName("Password")  as TextBox).Text}\", " +
                $"\"FirstName\" : \"{(this.FindName("FirstName") as TextBox).Text}\", " +
                $"\"LastName\"  : \"{(this.FindName("LastName")  as TextBox).Text}\", " +
                $"\"Gender\"    : \"{(this.FindName("Gender")    as ComboBox).SelectedItem.ToString()}\", " +
                $"\"Country\"   : \"{(this.FindName("Country")   as ComboBox).SelectedItem.ToString()}\", " +
                $"\"City\"      : \"{(this.FindName("City")      as TextBox).Text}\", " +
                $"\"Address\"   : \"{(this.FindName("Address")   as TextBox).Text}\", " +
                $"\"JobTitle\"  : \"{(this.FindName("JobTitle")  as TextBox).Text}\", " +
                $"\"Age\"       : \"{(this.FindName("Age")       as TextBox).Text}\" " +
                $"}}";
           
            ApiSetup(jsonStr, "CreateUser");
        }

        int userNameStartUp = 0;
        int passStartUp = 0;
        private void ClearBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (userNameStartUp == 0 && sender.GetType().Name == "TextBox")
            {
                TextBox txt = e.Source as TextBox;
                txt.Clear();
                userNameStartUp++;
            }
            else if (passStartUp == 0 && sender.GetType().Name == "PasswordBox")
            {
                PasswordBox pwBox = e.Source as PasswordBox;
                pwBox.Clear();
                passStartUp++;
            }
        }

        public static void ClearText(object sender, RoutedEventArgs e)
        {
            TextBox txt = e.Source as TextBox;
            txt.TextAlignment = TextAlignment.Left;
            txt.Clear();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            username = (StackPanelField.FindName("txtUserName") as TextBox).Text;
            pw = (StackPanelField.FindName("PasswordBox") as PasswordBox).Password;

            if (logintries >= 3 && banTimer > DateTime.Now)
            {
                MessageBox.Show($"Error! You have been logged out of the system until {banTimer}!");
            }
            else if (BanCheck() > DateTime.Now)
            {
                MessageBox.Show($"Error! You have been logged out of the system until {BanCheck()}!");
            }
            
            else
            {
                ApiSetup($"{{ \"email\" : \"{username}\",\"match\" : \"{pw}\", \"id\" : \"0\", \"usage\" : \"login\"}}", "Login");
            }
        }
        private static readonly HttpClient client = new HttpClient();

        public DateTime BanCheck()
        {
             var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User/{username}"));
             JObject Json = JObject.Parse(jsonStr);
             return DateTime.Parse(Json.SelectToken("loginBan").ToString());
        }
        

        public void ApiSetup(string content, string usage)
        {
            //Post request to send
            
            JObject json = JObject.Parse(content);
            HttpWebRequest httpWebRequest;
            if (usage == "Login")
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/User");
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/CreateUser");
            }
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            //Sending the request
            var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(json);
            streamWriter.Flush();
            httpWebRequest.Timeout = 999999;

            //Recieving the response
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string responseString = reader.ReadToEnd();
            reader.Close();
            if (usage == "Login")
            {
                if (responseString == "true")
                {
                    MainClient mainClient = new MainClient();
                    mainClient.Show();
                    WindowState = WindowState.Maximized;
                    //this.Close();
                    this.Hide();

                    //Note Make a full screen function - maybe
                }
                else
                {
                    LoginLimit();
                }
            }
            else
            {
                MessageBox.Show(responseString);
            }
        }
        public int logintries = 0;
        public int ban = 2;
        public DateTime banTimer;
        public void LoginLimit()
        {
          

            logintries++;
            if (logintries < 3)
            {
                MessageBox.Show($"Error! Wrong credentials entered, please try again. You have {3 - logintries} tries left");
            }
            else
            {
                banTimer = DateTime.Now.AddMinutes(ban);
                MessageBox.Show($"Error! Wrong credentials entered! You have now been locked out of the system until {banTimer}!");
                ban += ban;
            }
        }
    }
}
