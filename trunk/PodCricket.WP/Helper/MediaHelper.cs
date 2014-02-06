using Microsoft.Phone.Tasks;
using PodCricket.WP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PodCricket.WP.Helper
{
    public class MediaHelper
    {
        public static void BindSourceUri(MediaElement mediaElement, StreamModel streamModel)
        {
            if (streamModel.DownloadState == ApplicationServices.DownloadState.Downloaded)
            {
                var localPlayStream = streamModel.GetLocalPlayStream();
                if (localPlayStream != null)
                    mediaElement.SetSource(localPlayStream);
            }
            else
                mediaElement.Source = streamModel.DownloadUri;
        }

        public static void BindSourceUri(MediaPlayerLauncher mediaLauncher, StreamModel streamModel)
        {
            mediaLauncher.Controls = MediaPlaybackControls.All;
            mediaLauncher.Location = MediaLocationType.Data;

            if (streamModel.DownloadState == ApplicationServices.DownloadState.Downloaded)
                mediaLauncher.Media = streamModel.GetLocalPlayUri();
            else
                mediaLauncher.Media = streamModel.DownloadUri;
        }
    }
}
