using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices.Helper
{
    public class StreamHelper
    {
        public static string GetLocalDownloadPath(Uri downloadUri)
        {
            if (downloadUri == null) return string.Empty;
            return AppConfig.TEMP_DOWNLOAD_ROOT + Path.GetFileName(downloadUri.LocalPath);
        }

        public static string GetPodcastPath(Uri downloadUri)
        {
            if (downloadUri == null) return string.Empty;
            return AppConfig.PODCAST_DIRECTORY + Path.GetFileName(downloadUri.LocalPath);
        }
    }
}
