using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Chat_App.LoginSite;

namespace Chat_App.Methods
{
    class labels
    {
        //Templat for creation of control elements as Texboxes, comboboxes, etc
        public static Label Labels(string message, int size, FontStyle font, Brush background, Brush foreground, HorizontalAlignment textAlignment)
        {
            Label label = new Label();
            label.FontSize = size;
            label.Foreground = foreground;
            label.Background = background;
            label.Content = message;
            label.FontStyle = font;
            label.HorizontalContentAlignment = textAlignment;
            return label;
        }

        public static TextBox TextBoxes(string template, int size, FontStyle font, Brush foreground)
        {
            TextBox textbox = new TextBox();
            textbox.FontSize = size;
            textbox.Foreground = foreground;
            textbox.FontStyle = font;
            textbox.Text = template;
            textbox.PreviewMouseDoubleClick += ClearText;
            return textbox;
        }

        public static Button Buttons(string content, int size, Brush foreground, Brush background, RoutedEventHandler click)
        {
            Button btn = new Button();
            btn.Content = content;
            btn.FontSize = size;
            btn.Foreground = foreground;
            btn.Background = background;
            btn.Click += click;
            return btn;
        }

        public static CheckBox CheckBoxes(string content, Brush foreground)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Content = content;
            checkBox.Foreground = foreground;
            return checkBox;
        }

        public static Border borders(Brush borderColor)
        {
            Border border = new Border();
            border.Background = borderColor;
            return border;
        }
    }
}
