﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
using Store = Windows.ApplicationModel.Store;
#endif

namespace PodCricket.Utilities.AppLicense
{
    public static class LicenseHelper
    {
        public static bool Purchased(string productId)
        { 
            return Store.CurrentApp.LicenseInformation.ProductLicenses[productId].IsActive;
        }

        public static ProductLicense GetLicense(string productId)
        {
            return Store.CurrentApp.LicenseInformation.ProductLicenses[productId];
        }
    }
}
