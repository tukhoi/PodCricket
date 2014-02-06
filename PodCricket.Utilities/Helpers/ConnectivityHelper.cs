using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Net.NetworkInformation;

namespace PodCricket.Utilities.Helpers
{
    public class ConnectivityHelper
    {
        public static bool NetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        
    }
}
