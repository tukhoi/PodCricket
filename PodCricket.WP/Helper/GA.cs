using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.WP.Helper
{
    public class GA
    {
        public static void LogException(Exception exception, string addtionalMessage = "")
        {
            LogException(exception, false, addtionalMessage);
        }

        public static void LogException(Exception exception, bool fatal, string addtionalMessage = "")
        {
            var message = new StringBuilder();
            if (!string.IsNullOrEmpty(addtionalMessage))
                message.AppendLine("***" + addtionalMessage + "***");
            message.AppendLine("*****Message: " + exception.Message + "*****");
            message.AppendLine("*****Source: " + exception.Source + "*****");
            message.AppendLine("*****Stack trace: " + exception.StackTrace + "*****");

            GoogleAnalytics.EasyTracker.GetTracker().SendException(message.ToString(), fatal);
        }

        public static void BackgroundAgentFailToStart(string reason)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("BackgroundAgent", reason, null, 0);
        }

        public static void LogPage(string pageName)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView(pageName);
        }

        public static void LogAdsClicked(string pageName)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent(pageName, "Ads clicked", null, 0);
        }
    }
}
