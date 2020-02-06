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

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for LoginSite.xaml
    /// </summary>
    /// 
    /// connct to bson API with the request / timer ban coming from this application
    ///

    public partial class LoginSite : Window
    {
        public LoginSite()
        {

            InitializeComponent();
            PassLogin.Password = "Random Template Password Random Password";

        }

        private void txtUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TxtUsername.TextAlignment = TextAlignment.Left;
            TxtUsername.Clear();
        }

        private void PassLogin_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PassLogin.Clear();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
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
                ApiSetup();
            }
        }
        private static readonly HttpClient client = new HttpClient();

        public DateTime BanCheck()
        {
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User/{TxtUsername.Text}"));
            JObject Json = JObject.Parse(jsonStr);
             return DateTime.Parse(Json.SelectToken("loginBan").ToString());
        }

        public void ApiSetup()
        {
            //Post request to send
            string test = $"{{ \"email\" : \"{TxtUsername.Text}\",\"match\" : \"{PassLogin.Password}\", \"id\" : \"0\"}}";
            JObject json = JObject.Parse(test);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/User");
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

            if (responseString == "true")
            {
                MainClient mainClient = new MainClient();

                mainClient.Show();
                WindowState = WindowState.Maximized;

                this.Close();
                //this.Hide();

                //Note Make a full screen function - maybe
            }
            else
            {
                LoginLimit();
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
