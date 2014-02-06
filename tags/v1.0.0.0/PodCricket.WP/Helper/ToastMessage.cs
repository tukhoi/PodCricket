﻿using Coding4Fun.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PodCricket.WP.Helper
{
    public class ToastMessage
    {
        public static void Show(string message, string title="PodCricket")
        {
            ToastPrompt toast = new ToastPrompt();
            toast.Title = title;
            toast.Message = message;
            toast.FontSize = 20;
            toast.TextOrientation = System.Windows.Controls.Orientation.Vertical;
            toast.ImageSource = new BitmapImage(new Uri("/Resources/message.png", UriKind.RelativeOrAbsolute));

            toast.Show();
        }
    }
}