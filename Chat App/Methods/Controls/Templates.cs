using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chat_App.Methods.Controls
{
    public class Templates
    {
        // Template used for buttons and label creations
        public static Button buttonSetup(string content, int size, Brush background, Brush foreground, HorizontalAlignment contentALignment)
        {
            //Average template for creation of buttons
            Button button = new Button();
            button.Content = content;
            button.FontSize = size;
            button.Background = background;
            button.Foreground = foreground;
            button.HorizontalContentAlignment = contentALignment;
            return button;
        }

        public static Label Labels(string message, int size, FontStyle font, Brush background, Brush foreground)
        {
            Label label = new Label();
            label.FontSize = size;
            label.Foreground = foreground;
            label.Background = background;
            label.Content = message;
            label.FontStyle = font;
            return label;
        }
    }
}
