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
using static Chat_App.LoginSite;
using static Chat_App.Methods.Api_elements.APICallReq;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using javax.xml.transform.stream;
using org.w3c.dom.css;
using static Chat_App.Methods.Controls.Templates;

namespace Chat_App
{
    public partial class MainClient : Window
    {
        public MainClient()
        {
            InitializeComponent();
            comboBoxMode.SelectedIndex = 1;
        }
        public string CurrentName()
        {
               return LoginSite.username;  //Use this for when the login system is activated:
           // return "jpomfrey5@uol.com.br";  // Use this as a testing tool, to skip login phase
        }

        public int CurrentID;
        public UserClass MessagedUser;
        public UserClass CurrentUser()
        {
            //Gets current logged in user information
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/user/{CurrentName()}"));
            return JsonConvert.DeserializeObject<UserClass>(jsonStr);
        }
        private void comboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Changes mainclients left stackpanel to show elements of either Friendslist, Former chat (Last message sent / recied to all),
            //Sent friends request, recievied friends request.
            var user = CurrentUser();
            CurrentID = user._id;
            StackChatItems.Children.Clear();
            friendsList.Clear();
            if (user.FriendsList != null)
            {
                foreach (var item in user.FriendsList)
                {
                    APICall(item, "friendsList", "", string.Empty);
                }
                switch (comboBoxMode.SelectedIndex)
                {
                    case 0:
                        ShowAllUsers();
                        break;
                    case 1:
                        formerChatHistorySetup(user);
                        break;
                    case 2:
                        APICall(CurrentID, "PostSentReq", null, string.Empty);
                        break;
                    case 3:
                        APICall(CurrentID, "RecievedReq", null, string.Empty);
                        break;
                }
            }
        }
        #region API CALL
        public void APICall(int id, string usage, string value, string jsonStr)
        {
            try
            {
                //Post request to send
                HttpWebRequest httpWebRequest;
                //Checks if the API is to call between the user info collection or the chat history collection
                if (jsonStr == string.Empty)
                {
                    jsonStr = $"{{ \"id\" : \"{id}\", \"usage\" : \"{usage}\", \"value\" : \"{value}\" }}";
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/User");
                }
                else
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44371/api/PersonalChat");
                }
                //Sends request and receive the response
                var reader = streamReader(jsonStr, httpWebRequest);
                string responseString = reader.ReadToEnd();
                reader.Close();

                if (usage != string.Empty)
                {
                    Postreturn(id, usage, value, responseString);
                }
            }
            catch
            {
                //Failsafe if API is currently not running / connectable
                MessageBox.Show("Error! No connection to the API could be made!");
            }
        }

        public PostReturnUsers Postreturn(int id, string usage, string value, string responseString)
        {
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


                stackPanel.Children.Add(Labels("Search Results:", 16, FontStyles.Normal, Brushes.Transparent, Brushes.White));

                stackPanel.Children.Add(new Separator());
                foreach (var item in ListOfUsers)
                {
                    Grid grid = new Grid();
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);
                    border.Name = $"borderAddUser{item.id}";
                    Label lbNames = Labels(item.json, 16, FontStyles.Normal, Brushes.Transparent, Brushes.Black);
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
                    stackPanel.Children.Add(Labels(item.json, 14, FontStyles.Normal, Brushes.Transparent, Brushes.White));
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

            else if (usage == "PostAddFriend")
            {
                if (responseString == "true")
                {
                    StackChatItems.Children.Clear();
                    MessageBox.Show("Successfully added!");
                }
                else { MessageBox.Show("Error! Unexpected error has eccoured: Error code 10"); }
            }
            return (new PostReturnUsers(responseString, id));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            string id = Regex.Replace(btn.Name, "[^0-9.]", "");
            APICall(CurrentID, "CancelSentReq", id, string.Empty);
            StackChatItems.Children.Clear();
            APICall(CurrentID, "PostSentReq", null, string.Empty);
        }

        public void RecievedReqSection(List<PostReturnUsers> list)
        {
            foreach (var item in list)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(Labels(item.json, 14, FontStyles.Normal, Brushes.Transparent, Brushes.White));
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
            APICall(CurrentID, "FriendReqAccep", id, string.Empty);
            StackChatItems.Children.Clear();
            APICall(CurrentID, "RecievedReq", null, string.Empty);
        }

        private void addFriendsSelected_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            string id = Regex.Replace(button.Name, "[^0-9.]", "");

            APICall(CurrentID, "PostAddFriend", id, string.Empty);
            btnAddNewFr_Click(btnAddNewFr, null);
            //here
            FriendsSearchsetup();
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
        public void ShowAllUsers()
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
            try {
                foreach (var item in SortedChatMessages)
                {

                    var nameStr = friendsList.FirstOrDefault(x => x.Id == item._id);


                    Button button = new Button();
                    if (item.message.Contains("/9j/4AA") || item.message.Contains("R0lGODlh"))
                    {
                        button.Content = $"{nameStr.Names} - {item.TimeStamp}\nImg";
                    }
                    else
                    {
                        button.Content = $"{nameStr.Names} - {item.TimeStamp}\n{item.message}";
                    }
                    button.HorizontalContentAlignment = HorizontalAlignment.Left;
                    button.Background = Brushes.Transparent;
                    button.Foreground = Brushes.White;
                    button.Name = $"lbHistorthey{item._id}";
                    button.Click += btn_FriendsList_click;
                    StackChatItems.Children.Add(button);
                }
            }
            catch { }

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
        {
            if (e.Key == Key.Enter && txtUserMessage.Text != string.Empty)
            {
                MessageSendToAPI();
            }
        }

        private void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageSendToAPI();
        }

        public void MessageSendToAPI()
        {
            var label = Labels(txtUserMessage.Text, 14, FontStyles.Normal, Brushes.SkyBlue, Brushes.Black);
            var timestamp = Labels(DateTime.Now.ToString(), 9, FontStyles.Italic, Brushes.SkyBlue, Brushes.Black);

            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);

            Grid grid = msgGridSetup(timestamp, label, null, null);
            border.Child = grid;
            StackPanelfield.Children.Add(border);
            txtUserMessage.Clear();

            messageToApi(timestamp.Content.ToString(), label.Content.ToString());
            //add message to personalapicontroller here
        }
        void messageToApi(string dateTime, string Message)
        {
            try
            {
                //current users format
                string json = $" {{ \"CurrentID\":{CurrentID}, \"MatchID\":{CurrentMessagingUserID}, \"message\":\"{Message}\", \"Timestamp\":\"{dateTime}\", \"CurrentUser\":true }}";
                APICall(CurrentID, string.Empty, string.Empty, json);

                //recieving users format (for log)
                json = $" {{ \"CurrentID\":{CurrentMessagingUserID}, \"MatchID\":{CurrentID}, \"message\":\"{Message}\", \"Timestamp\":\"{dateTime}\", \"CurrentUser\":false }}";
                APICall(CurrentMessagingUserID, "Bot", string.Empty, json);

                //refresh
                FriendsChat();
                var current = comboBoxMode.SelectedIndex;
                if (comboBoxMode.SelectedIndex! <= 4)
                {
                    comboBoxMode.SelectedIndex = comboBoxMode.SelectedIndex + 1;
                }
                else
                {
                    comboBoxMode.SelectedIndex = 3;
                }
                comboBoxMode.SelectedIndex = current;
                //scrollviewer to bottom
                scrollViewer.ScrollToEnd();
            }
            catch
            {
                MessageBox.Show("Error at MessageToAPI, connection could not be made");
            }
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
        int CurrentMessagingUserID;

        private void btn_FriendsList_click(object sender, RoutedEventArgs e)
        {
            // removes the letters of the current button in use, leaving only the numbers left, which is their unique ID number
            Button button = e.Source as Button;

            //    string id = Regex.Replace(button.Name, "[^0-9.]","");
            CurrentMessagingUserID = Convert.ToInt32(Regex.Replace(button.Name, "[^0-9.]", ""));
            FriendsChat();
        }
        public void FriendsChat()
        {
            // used to send the request to recieve the chat logs
            StackPanelfield.Children.Clear();
            StackPanelfield.VerticalAlignment = VerticalAlignment.Bottom;
            //     var name = FriendsList.Where(x => x.Value == Convert.ToInt32(id)).ToDictionary();
            var name = friendsList.Where(x => x.Id == CurrentMessagingUserID).ToList();
            string FullName = "";
            foreach (var item in name)
            {
                FullName = item.Names;
            }
            Separator separator = new Separator();
            StackPanelfield.Children.Add(Labels($"This is the start of your conversation with {FullName}", 14, FontStyles.Italic, Brushes.Transparent, Brushes.White));
            StackPanelfield.Children.Add(separator);
            StackPanelfield.Margin = new Thickness(0, 0, 0, 0);

            var logs = Logs();
            var message =
                from c in logs.Chat
                where c._id == Convert.ToInt32(CurrentMessagingUserID)
                select c.chatLists;
            var first = logs.Chat.Where(x => x._id == Convert.ToInt32(CurrentMessagingUserID)).ToList();

            var second = first.Cast<messages>().ToArray();
            //Find a more smooth way than by using 2 foreach loops
            foreach (var item in second)
            {
                List<Chats> sortedList = item.chatLists.OrderBy(O => Convert.ToDateTime(O.Timestamp)).ToList();

                foreach (var item2 in sortedList)
                {
                    var label = Labels(item2.message, 14, FontStyles.Normal, Brushes.SkyBlue, Brushes.Black);
                    Border border = new Border();
                    border.HorizontalAlignment = HorizontalAlignment.Right;
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);
                    Grid grid = new Grid();
                    // Determinds which to use by the first few characters in the base64string 
                    // /9j/4AA: Pictures
                    //R0lGODlh : Gif
                    if (item2.message.Contains("/9j/4AA") || item2.message.Contains("R0lGODlh"))
                    {
                        grid =  msgGridSetup(label, null, ImgFromBuffer(Convert.FromBase64String(item2.message)), Convert.FromBase64String(item2.message));
                        border.BorderBrush = Brushes.Transparent;
                    }
                    else
                    {
                        // Sets the format for the displaying messages sent and recieved
                        var timestamp = Labels(item2.Timestamp, 9, FontStyles.Italic, Brushes.SkyBlue, Brushes.Black);
                        if (item2.CurrentUser == false)
                        {
                            timestamp.Background = Brushes.White;
                            label.Background = Brushes.White;
                            border.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        grid = msgGridSetup(timestamp, label, null, null);
                    }
                        border.Child = grid;
                        StackPanelfield.Children.Add(border);
                }
            }
            scrollViewer.Visibility = Visibility.Visible;
            scrollViewer.ScrollToEnd();
        }
        #endregion

        public Grid msgGridSetup(Label timestamp, Label? msg, BitmapImage? BitMap, Byte[]? bytes)
        {
            Grid grid = new Grid();
            try
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                RowDefinition rd2 = new RowDefinition();
                rd2.Height = GridLength.Auto;
                grid.RowDefinitions.Add(rd);
                grid.RowDefinitions.Add(rd2);
                if (msg != null)
                {
                    // Normal messages
                    grid.Children.Add(timestamp);
                    grid.Children.Add(msg);
                    Grid.SetRow(timestamp, 0);
                    Grid.SetRow(msg, 1);
                }
                //("/9j/4AA") || item2.message.Contains("R0lGODlh")
                
                else
                {
                    //Img / gifs
                    Image img = new Image();
                    img.Source = BitMap;
                    img.Width = 350;
                    img.Height = 250;

                    /*
                    if (Convert.ToBase64String(bytes).Contains("R0lGODlh"))
                    {
                        // Gif images
                        GifBitmapDecoder gifBitmapDecoder = new GifBitmapDecoder(BitMap.StreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource bitmapSource = gifBitmapDecoder.Frames[0];
                        img.Source = bitmapSource;
                    }
                    */
                    grid.Children.Add(img);
                }
                return grid;
            }
            catch
            {
                return grid;
            }
        }
        private void btnAddNewFr_Click(object sender, RoutedEventArgs e)
        {
            //Searchbar / textfield for searching for new users to add
            StackPanelfield.Children.Clear();
            StackPanelfield.Visibility = Visibility.Visible;
            StackPanelfield.VerticalAlignment = VerticalAlignment.Top;

            txtUserMessage.Visibility = Visibility.Hidden;
            sendMessage.Visibility = Visibility.Hidden;
            var lb = Labels("Search for friends to Add", 20, FontStyles.Italic, Brushes.Transparent, Brushes.White);
            lb.HorizontalAlignment = HorizontalAlignment.Center;
            TextBox textBox = new TextBox();
            textBox.Text = "Search for user";
            textBox.FontStyle = FontStyles.Italic;
            textBox.PreviewMouseDown += txtSearch_MouseDown;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            textBox.FontSize = 18;
            textBox.Background = Brushes.White;
            textBox.Visibility = Visibility.Visible;
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
            APICall(0, "AddFriends", "", string.Empty);
        }
        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            //When clicked, it will open windows explorer, where you can select a picture / image 
            //which you want to send to the party on the recieving end, and which will be stored into the logs
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files|*.bmp *.png *.jpg *.gif *.jpg;*.gif;*.png;*.tif|All files|*.*";
            dlg.RestoreDirectory = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                //Future todo: Add a Gif function 
              //  if (!dlg.FileName.Contains(".gif"))
                //{
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = System.IO.File.OpenRead(dlg.FileName);
                    Byte[] BytesToSave = BufferFromImg(bi);
                    bi.DecodePixelHeight = 250;
                    bi.DecodePixelWidth = 350;
                    bi.EndInit();
                    ImgToMainPage(ImgFromBuffer(BytesToSave), BytesToSave);
                    messageToApi(Convert.ToString(DateTime.Now), ($"{Convert.ToBase64String(BytesToSave)}"));
          /*      }
                else
                {
                    //gif
                }*/
            }
        }

        public void ImgToMainPage(BitmapImage image, Byte[] bytes)
        {
            var timestamp = Labels(DateTime.Now.ToString(), 9, FontStyles.Italic, Brushes.Transparent, Brushes.Black);
            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.BorderBrush = Brushes.Transparent;
            border.BorderThickness = new Thickness(1);

            Grid grid = msgGridSetup(timestamp, null, image, bytes);
            border.Child = grid;
            StackPanelfield.Children.Add(border);
            txtUserMessage.Clear();
            
        }
        public BitmapImage ImgFromBuffer(Byte[] bytes)
        {
            //Uses input from DB
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }

        public Byte[] BufferFromImg(BitmapImage img)
        {
            // Uses the path from the BitmapImage stated (from the selected file by the user) 
            // and converts it into bytes, which will be used for storing images on the database for chats
            Stream stream = img.StreamSource;
            Byte[] buffer = null; 
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }
            return buffer;
        }
    }
}