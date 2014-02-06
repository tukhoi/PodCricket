using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PodCricket.Utilities.Extensions;

namespace PodCricket.Utilities.Tasks
{
    public class WebTasks
    {
        public static async Task<string> GetRawResult(string requestUri)
        {
            var client = new WebClient();
            return await client.DownloadStringTaskAsync(new Uri(requestUri));
        }
    }
}
