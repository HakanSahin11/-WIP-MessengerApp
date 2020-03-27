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
    public partial class MainClient : Window
    {
        public MainClient()
        {
            InitializeComponent();
            comboBoxMode.SelectedIndex = 1;
        }
        public string CurrentName = "jpomfrey5@uol.com.br";
        public int CurrentID = 6;
        public UserClass MessagedUser;
        public UserClass CurrentUser()
        {
            var email = CurrentName;
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/user/{email}"));
            return JsonConvert.DeserializeObject<UserClass>(jsonStr);
        }

        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User/{CurrentName}"));
            //  CurrentUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserClass>(jsonStr);
            var user = CurrentUser();
            StackChatItems.Children.Clear();
            friendsList.Clear();
            foreach (var item in user.FriendsList)
            {
                var response = APICall(item, "friendsList", "");
            }


            if (comboBoxMode.SelectedIndex == 0)
            {
                ShowAllUsers(user);
            }
            else if (comboBoxMode.SelectedIndex == 1)
            {
                formerChatHistorySetup(user);
            }
            else if (comboBoxMode.SelectedIndex == 2)
            {
                APICall(CurrentID, "PostSentReq", null);
            }
            else if (comboBoxMode.SelectedIndex == 3)
            {
                APICall(CurrentID, "RecievedReq", null);
            }
        }
        List<PostReturnUsers> SearchResults = new List<PostReturnUsers>();

        #region API CALL
        public PostReturnUsers APICall(int id, string usage, string value)
        {
            //Post request to send
            string jsonStr = $"{{ \"id\" : \"{id}\", \"usage\" : \"{usage}\", \"value\" : \"{value}\" }}";
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

            if (usage == "friendsList")
            {
                friendsList.Add(new FriendsListMethod(responseString, id));
            }

            else if (usage == "AddFriends")
            {
                if (StackPanelfield.Children.Count >= 4)
                {
                    StackPanelfield.Children.RemoveAt(StackPanelfield.Children.Count - 1);
                }

                var ListOfUsers = JsonConvert.DeserializeObject<List<PostReturnUsers>>(responseString);
                var CurrentUserPost = ListOfUsers.SingleOrDefault(x => x.id == CurrentID);
                if (CurrentUserPost != null)
                    ListOfUsers.Remove(CurrentUserPost);

                //Used to select the textbox of the searched person for
                string searchResult = "";
                foreach (var item in StackPanelfield.Children)
                {
                    if (item is TextBox)
                    {
                        TextBox txtBox = item as TextBox;
                        //  MessageBox.Show(txtBox.Text);
                        searchResult = txtBox.Text;
                    }

                }
                StackPanel stackPanel = new StackPanel();
                var user = CurrentUser();
                ListOfUsers.RemoveAll(c => friendsList.ToList().Exists(n => n.Names == c.json) ||
                            !c.json.Contains(searchResult, StringComparison.OrdinalIgnoreCase) ||
                             user.SentFriendReq.ToList().Exists(n => n == c.id));


                stackPanel.Children.Add(labelSetup("Search Results:", 16, FontStyles.Normal, Brushes.Transparent, Brushes.White));
                stackPanel.Children.Add(new Separator());
                foreach (var item in ListOfUsers)
                {
                    Grid grid = new Grid();
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);
                    Label lbNames = labelSetup(item.json, 16, FontStyles.Normal, Brushes.Transparent, Brushes.Black);
                    lbNames.HorizontalAlignment = HorizontalAlignment.Left;

                    Button btn = buttonSetup("Add Friend", 14, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Right);
                    btn.HorizontalAlignment = HorizontalAlignment.Right;
                    btn.Click += addFriendsSelected_Click;
                    btn.Name = $"addFriend{item.id}";
                    border.Background = Brushes.White;
                    grid.Children.Add(lbNames);
                    grid.Children.Add(btn);
                    border.Child = grid;
                    stackPanel.Children.Add(border);
                }
                StackPanelfield.Children.Add(stackPanel);
            }
            else if (usage == "PostSentReq")
            {
                var result = JsonConvert.DeserializeObject<List<PostReturnUsers>>(responseString);
                //   return( new PostReturnUsers(result.FirstName, result))

                foreach (var item in result)
                {
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Children.Add(labelSetup(item.json, 14, FontStyles.Normal, Brushes.Transparent, Brushes.White));
                    Button btnCancel = buttonSetup("Cancel", 14, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Center);
                    btnCancel.Name = $"friendReqcancel{item.id}";
                    btnCancel.Click += BtnCancel_Click;
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);
                    border.Child = btnCancel;
                    Separator separator = new Separator();
                    stackPanel.Children.Add(border);
                    stackPanel.Children.Add(separator);
                    StackChatItems.Children.Add(stackPanel);
                }
            }
            else if (usage == "RecievedReq")
            {
                RecievedReqSection(JsonConvert.DeserializeObject<List<PostReturnUsers>>(responseString));
                //   return( new PostReturnUsers(result.FirstName, result))
            }
            return (new PostReturnUsers(responseString, id));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            string id = Regex.Replace(btn.Name, "[^0-9.]", "");
            var APIResponse = APICall(CurrentID, "CancelSentReq", id);
            StackChatItems.Children.Clear();
            APICall(CurrentID, "PostSentReq", null);
        }

        public void RecievedReqSection(List<PostReturnUsers> list)
        {
            foreach (var item in list)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(labelSetup(item.json, 14, FontStyles.Normal, Brushes.Transparent, Brushes.White));
                Grid grid = new Grid();
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(5, GridUnitType.Star);
                grid.ColumnDefinitions.Add(c1);
                ColumnDefinition c2 = new ColumnDefinition();
                c2.Width = new GridLength(5, GridUnitType.Star);
                grid.ColumnDefinitions.Add(c2);

                Button btnAccept = buttonSetup("Accept", 14, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Center);
                btnAccept.Name = $"friendReqAccept{item.id}";
                btnAccept.Click += BtnAccept_Click;
                grid.Children.Add(btnAccept);

                Button btnDecline = buttonSetup("Decline", 14, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Center);
                btnDecline.Name = $"friendReqDecline{item.id}";
                btnDecline.Click += BtnCancel_Click;
                grid.Children.Add(btnDecline);

                Grid.SetColumn(btnDecline, 1);
                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = new Thickness(1);
                border.Child = grid;

                Separator separator = new Separator();
                stackPanel.Children.Add(border);
                stackPanel.Children.Add(separator);
                StackChatItems.Children.Add(stackPanel);
            }
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            string id = Regex.Replace(button.Name, "[^0-9.]", "");
            APICall(CurrentID, "FriendReqAccep", id);
            StackChatItems.Children.Clear();
            APICall(CurrentID, "RecievedReq", null);
        }

        private void addFriendsSelected_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            string id = Regex.Replace(button.Name, "[^0-9.]", "");

            var APIResponse = APICall(CurrentID, "PostAddFriend", id);

            //Runs the code again, to clear the selected user
            if (APIResponse.json == "true")
            {
                StackChatItems.Children.Clear();
                MessageBox.Show("Successfully added!");
            }
            else { MessageBox.Show("Error! Unexpected error has eccoured: Error code 10"); }
        }
        public chatListsMethod Logs()
        {
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/Chat/{CurrentID}"));
            chatListsMethod logs = JsonConvert.DeserializeObject<chatListsMethod>(jsonStr);
            return logs;
        }
        #endregion

        #region Show all Users FriendsList
        public List<FriendsListMethod> friendsList = new List<FriendsListMethod>();
        public void ShowAllUsers(UserClass user)
        {
            var sortedDic = friendsList.OrderBy(x => x.Names).ToList();
            foreach (var item in sortedDic)
            {
                var input = new Button();
                input.Content = item.Names;
                input.Name = $"LbFriendsListItem{item.Id}";
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
                var nameStr = friendsList.FirstOrDefault(x => x.Id == item._id);
                Button button = new Button();
                button.Content = $"{nameStr.Names} - {item.TimeStamp}\n{item.message}";
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
            TextBox tb = sender as TextBox;
            if (tb != null) tb.Text = "";
            tb.FontStyle = FontStyles.Normal;
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
            StackPanelfield.VerticalAlignment = VerticalAlignment.Bottom;
       //     var name = FriendsList.Where(x => x.Value == Convert.ToInt32(id)).ToDictionary();
            var name = friendsList.Where(x => x.Id == Convert.ToInt32(id)).ToList();
            string FullName = "";
            foreach (var item in name)
            {
                FullName = item.Names;
            }
            Separator separator = new Separator();
            StackPanelfield.Children.Add(labelSetup($"This is the start of your conversation with {FullName}", 14, FontStyles.Italic, Brushes.Transparent, Brushes.White));
            StackPanelfield.Children.Add(separator);
            StackPanelfield.Margin = new Thickness(0, 0, 0, 0);

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
                    var label = labelSetup(item2.message, 14, FontStyles.Normal, Brushes.SkyBlue, Brushes.Black);
                    var timestamp = labelSetup(item2.Timestamp, 9, FontStyles.Italic, Brushes.SkyBlue, Brushes.Black);

                    Border border = new Border();
                    border.HorizontalAlignment = HorizontalAlignment.Right;
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);

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

        public Label labelSetup(string message, int size, FontStyle font, Brush background, Brush foreground)
        {
            Label label = new Label();
            label.FontSize = size;
            label.Foreground = foreground;
            label.Background = background;
            label.Content = message;
            label.FontStyle = font;
            return label;
        }

        private void btnAddNewFr_Click(object sender, RoutedEventArgs e)
        {
            StackPanelfield.Children.Clear();
            StackPanelfield.VerticalAlignment = VerticalAlignment.Top;
           
            txtUserMessage.Visibility = Visibility.Hidden;
            var lb = labelSetup("Search for friends to Add", 20, FontStyles.Italic, Brushes.Transparent, Brushes.White);
            lb.HorizontalAlignment = HorizontalAlignment.Center;
            TextBox textBox = new TextBox();
            textBox.Text = "Search for user";
            textBox.FontStyle = FontStyles.Italic;
            textBox.PreviewMouseDown += txtSearch_MouseDown;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            textBox.FontSize = 18;
            textBox.Background = Brushes.White;
            textBox.Name = "test";
            textBox.Uid = "test2";
            
            var btn = buttonSetup("Search", 18, Brushes.LightGray, Brushes.Black, HorizontalAlignment.Center);
            btn.Click += Addfriends_Click;
            textBox.KeyDown += searchBox_KeyDown;

            StackPanelfield.Margin = new Thickness(50, 10, 50, 10);
            StackPanelfield.Children.Add(lb);
            StackPanelfield.Children.Add(textBox);
            StackPanelfield.Children.Add(btn);

        }

        private void Addfriends_Click(object sender, RoutedEventArgs e)
        {
            FriendsSearchsetup();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FriendsSearchsetup();
            }
        }

        public void FriendsSearchsetup()
        {
            var apiResponse = APICall(0, "AddFriends", "");
            List<FriendsListMethod> ListofUsers = new List<FriendsListMethod>();
            //       chatListsMethod logs = JsonConvert.DeserializeObject<chatListsMethod>(jsonStr);
      //      JObject json = JObject.Parse(apiResponse);
       //     ListofUsers.Add(JsonConvert.DeserializeObject<FriendsListMethod>(apiResponse.json));

        }

        public Button buttonSetup(string content, int size, Brush background, Brush foreground, HorizontalAlignment contentALignment)
        {

            Button button = new Button();
            button.Content = content;
            button.FontSize = size;
            button.Background = background;
            button.Foreground = foreground;
            button.HorizontalContentAlignment = contentALignment;
            return button;
        }
    }
}
