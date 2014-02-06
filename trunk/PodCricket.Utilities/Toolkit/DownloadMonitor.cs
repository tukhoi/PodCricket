
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PodCricket.Utilities.Toolkit
{
    public class DownloadMonitor : TransferMonitor
    {
        public DownloadMonitor(BackgroundTransferRequest request) : base(request)
        {
        }

        public DownloadMonitor(BackgroundTransferRequest request, string name) : base(request, name)
        {        
        }

        public TransferMonitor Monitor { get { return this; } }
        public object Tag { get; set; }
        public string RequestId { get; set; }
        public string PodName { get; set; }
        public string StreamTitle { get; set; }
        public Uri ImageUri { get; set; }
    }
}
