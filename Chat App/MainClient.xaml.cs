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
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Chat_App.Methods.Logs;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for MainClient.xaml
    /// </summary>
    /// 
    /// 
    /// NOTE
    /// Set up the combobox in the clientside to show depending on the method used by index. Maybe use the same stackpanel while clearing it upon changing index
    public partial class MainClient : Window
    {
        public MainClient()
        {
            InitializeComponent();
            comboBoxMode.SelectedIndex = 1;
        }
        public string CurrentName = "jpomfrey5@uol.com.br";
        public int CurrentID = 6;

        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User/{CurrentName}"));
            UserClass user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserClass>(jsonStr);

            StackChatItems.Children.Clear();
            FriendsList.Clear();
            foreach (var item in user.FriendsList)
            {
                APICall(item);
            }

            if (comboBoxMode.SelectedIndex == 0)
            {
                ShowAllUsers(user);
            }
            else if (comboBoxMode.SelectedIndex == 1)
            {
                formerChatHistorySetup(user);
            }
        }

        #region API CALL
        public void APICall(int id)
        {
            //Post request to send
            string jsonStr = $"{{ \"id\" : \"{id}\"}}";
            JObject json = JObject.Parse(jsonStr);

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
        public chatListsMethod Logs()
        {
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/Chat/{CurrentID}"));
            chatListsMethod logs = JsonConvert.DeserializeObject<chatListsMethod>(jsonStr);
            return logs;
        }
        #endregion

        #region Show all Users FriendsList
        public Dictionary<string, int> FriendsList = new Dictionary<string, int>();
        public void ShowAllUsers(UserClass user)
        {
            var sortedDic = FriendsList.Keys.OrderBy(x => x).ToDictionary(y => y, y => FriendsList[y]);
            foreach (var item in sortedDic)
            {
                var input = new Button();
                input.Content = item.Key;
                input.Name = $"LbFriendsListItem{item.Value}";
                input.Background = Brushes.Transparent;
                input.Foreground = Brushes.White;
                input.Click += btn_FriendsList_click;
                input.HorizontalContentAlignment = HorizontalAlignment.Left;
                StackChatItems.Children.Add(input);
            }
        }
        #endregion

        #region Show Recent Chats In FriendsList
        public void formerChatHistorySetup(UserClass users)
        {
            StackChatItems.Children.Clear();
            var logs = Logs();
        
            List<messages> Unsort = new List<messages>();
            foreach (var item in logs.Chat)
            {
                Unsort.Add(item);
            }
            List<chatMessages> chatMessages = new List<chatMessages>();

            foreach (var item in Unsort)
            {
                Chats chats;
                chats = item.chatLists.OrderByDescending(p => Convert.ToDateTime(p.Timestamp)).FirstOrDefault();

                chatMessages messagesToList = new chatMessages(chats.message, Convert.ToDateTime(chats.Timestamp), item._id);
                chatMessages.Add(messagesToList);
            }
            // add the items to the string list then display last message to the stackpanel
            // takes the object with  smallest value, to then use it on the History of last message recieved
            var SortedChatMessages = chatMessages.OrderByDescending(x => x.TimeStamp).ToList();

            foreach (var item in SortedChatMessages)
            {
                Label content = new Label();
          
                Label name = new Label();
                var nameStr = FriendsList.FirstOrDefault(x => x.Value == item._id);
                Button button = new Button();
                button.Content = $"{nameStr.Key} - {item.TimeStamp}\n{item.message}";
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.White;
                button.Name = $"lbHistorthey{item._id}";
                button.Click += btn_FriendsList_click;
                StackChatItems.Children.Add(button);
            }

        }
        #endregion

        private void txtSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.Clear();
            txtSearch.FontStyle = FontStyles.Normal;
        }
        
        #region Chat Old Interface
        private void txtUserMessageSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {/*
            if (e.Key == Key.Enter)
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
            */
        }
        #endregion
        

        private void txtUserMessage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtUserMessage.Text == "Enter your message")
            {
                txtUserMessage.Clear();
                txtUserMessage.FontStyle = FontStyles.Normal;
            }
        }

        #region New Chat Interface
        private void btn_FriendsList_click(object sender, RoutedEventArgs e)
        {
            // removes the letters of the current button in use, leaving only the numbers left, which is their unique ID number
            Button button = e.Source as Button;
            string id = Regex.Replace(button.Name, "[^0-9.]","");

            // used to send the request to recieve the chat logs
            StackPanelfield.Children.Clear();
            var logs = Logs();
            var message =
                from c in logs.Chat
                where c._id == Convert.ToInt32(id)
                select c.chatLists;
            var test = logs.Chat.Where(x => x._id == Convert.ToInt32(id)).ToList();

           var test2 = test.Cast<messages>().ToArray();

            //Find a more smooth way than by using 2 foreach loops
            foreach (var item in test2)
            {
                List<Chats> sortedList = item.chatLists.OrderBy(O => O.Timestamp).ToList();
                foreach (var item2 in sortedList)
                {
                    // Sets the format for the displaying messages sent and recieved
                    Label label = new Label();
                    label.Content = item2.message;
                    label.FontSize = 14;
                    label.Foreground = Brushes.Black;
                    label.Background = Brushes.SkyBlue;

                    Border border = new Border();
                    border.HorizontalAlignment = HorizontalAlignment.Right;
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);

                    Label timestamp = new Label();
                    timestamp.FontStyle = FontStyles.Italic;
                    timestamp.FontSize = 9;
                    timestamp.Content = item2.Timestamp;
                    timestamp.Foreground = Brushes.Black;
                    timestamp.Background = Brushes.SkyBlue;
                    if (item2.CurrentUser == false)
                    {
                        timestamp.Background = Brushes.White;
                        label.Background = Brushes.White;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                    }

                    Grid grid = new Grid();
                    RowDefinition rd = new RowDefinition();
                    rd.Height = GridLength.Auto;
                    RowDefinition rd2 = new RowDefinition();
                    rd2.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(rd);
                    grid.RowDefinitions.Add(rd2);

                    grid.Children.Add(timestamp);
                    grid.Children.Add(label);
                    Grid.SetRow(timestamp, 0);
                    Grid.SetRow(label, 1);
                    border.Child = grid;
                    StackPanelfield.Children.Add(border);
                }
            }
        }

        #endregion
    }
}
