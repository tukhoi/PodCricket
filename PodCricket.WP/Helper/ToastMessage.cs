using Coding4Fun.Toolkit.Controls;
using PodCricket.WP.Resources;
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
            title = AppResources.ApplicationTitle;

            ToastPrompt toast = new ToastPrompt();
            toast.TextWrapping = System.Windows.TextWrapping.Wrap;
            toast.Title = title;
            toast.Message = message;
            toast.FontSize = 20;
            toast.TextOrientation = System.Windows.Controls.Orientation.Vertical;
            toast.ImageSource = new BitmapImage(new Uri("/Resources/message.png", UriKind.RelativeOrAbsolute));

            toast.Show();
        }
    }
}
