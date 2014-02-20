using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using PodCricket.ApplicationServices;
using PodCricket.Utilities.AppLicense;
using PodCricket.WP.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PodCricket.WP.Helper
{
    public class Messenger
    {
        public static void ShowToast(string message, string title="PodCricket")
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

        public static void ShowBuyLicense()
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = AppResources.ProLicenseCaption,
                Message = AppResources.ProLicenseTitle,
                LeftButtonContent = AppResources.ProLicenseLeftButton,
                RightButtonContent = AppResources.ProLicenseRightButton,
                IsFullScreen = false
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        LicenseHelper.PurchaseProduct(AppConfig.PRO_VERSION);
                        break;
                    case CustomMessageBoxResult.RightButton:
                        break;
                    case CustomMessageBoxResult.None:
                        // Do something.
                        break;
                    default:
                        break;
                }
            };

            messageBox.Show(); 
        }
    }
}
