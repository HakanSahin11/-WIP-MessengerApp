using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chat_App.Methods;
using Newtonsoft.Json.Linq;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for MainClient.xaml
    /// </summary>
    public partial class MainClient : Window
    {
        public MainClient()
        {
            InitializeComponent();
            Initiate();
        }
        public string CurrentName = "jpomfrey5@uol.com.br";

        public void Initiate()
        {
            friendsList();
        }
        private void txtSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.Clear();
            txtSearch.FontStyle = FontStyles.Normal;
        }

        private void txtUserMessageSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var userName = "User";

                DateTime timeSent = DateTime.Now;
                string text = txtUserMessage.Text;

                // show messages
                var timeStamp = new Label();
                timeStamp.Content = $"{userName} at {timeSent}";
                timeStamp.Foreground = Brushes.White;
                timeStamp.FontSize = 10;
                timeStamp.VerticalAlignment = VerticalAlignment.Bottom;
                timeStamp.VerticalContentAlignment = VerticalAlignment.Bottom;
                timeStamp.FontStyle = FontStyles.Italic;

                var input = new Label();
                input.Content = $"{text}\n";
                input.Foreground = Brushes.White;
                input.FontSize = 14;
                input.VerticalAlignment = VerticalAlignment.Top;
                input.VerticalContentAlignment = VerticalAlignment.Top;


                StackPanelfield.Children.Add(timeStamp);
                StackPanelfield.Children.Add(input);
                // bson to db with "text"
                
                txtUserMessage.Clear();
            }
        }

        private void txtUserMessage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtUserMessage.Text == "Enter your message")
            {
                txtUserMessage.Clear();
                txtUserMessage.FontStyle = FontStyles.Normal;
            }
        }

        public void friendsList()
        {
            //foreach for the users firstnames
            //customer numbers etc
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User/{CurrentName}"));
            UserClass user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserClass>(jsonStr);
            foreach (var item in user.FriendsList)
            {
                APICall(item);
            }

            var sortedDic = FriendsList.Keys.OrderBy(x => x).ToDictionary(y => y, y => FriendsList[y]);

            foreach (var item in sortedDic)
            {
                var input = new Button();
                input.Content = item.Key;
                input.Name = $"LbFriendsListItem{item.Value}";
                input.Background = Brushes.Transparent;
                input.Foreground = Brushes.White;
                input.Click += btn_MouseOver;
                /*
                OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect();
                effect.GlowColor = Colors.Gold;
                effect.GlowSize = 10;

                Setter setter = new Setter();
                setter.Property = UIElement.BitmapEffectProperty;
                setter.Value = effect;

                Trigger trigger = new Trigger();
                trigger.Property = UIElement.IsMouseOverProperty;
                trigger.Value = true;
                trigger.Setters.Add(setter);

                Style style = new Style();
                style.Triggers.Add(trigger);


                input.Style = style;

    */
                //add click function for when the user clicks on the person on the friendslists to open their chat

                stackPanelFriendsList.Children.Add(input);
            }
        }


        private void btn_MouseOver(object sender, RoutedEventArgs e)
        {
            // removes the letters of the current button in use, leaving only the numbers left, which is their unique ID number
            Button button = e.Source as Button;
            string id = Regex.Replace(button.Name, "[^0-9.]","");
            MessageBox.Show(id);
        }

        public Dictionary<string, int> FriendsList = new Dictionary<string, int>();

        public void APICall(int id)
        {
            //Post request to send
            string test = $"{{ \"id\" : \"{id}\"}}";
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

            FriendsList.Add(responseString, id);


        }

        private void BtnShowFriends_Click(object sender, RoutedEventArgs e)
        {
            if (stackPanelBorder.Visibility == Visibility.Visible)
            {
                stackPanelBorder.Visibility = Visibility.Hidden;
            }
            else
            {
                stackPanelBorder.Visibility = Visibility.Visible;
            }
        }
    }
}
