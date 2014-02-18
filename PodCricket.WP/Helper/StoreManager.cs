using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

namespace PodCricket.WP.Helper
{
    public class StoreManager
    {
        public Dictionary<string, string> StoreItems = new Dictionary<string, string>();
        //        private const string FreeImage = "/Res/Image/1.png";

        public StoreManager()
        {
            // Populate the store
            StoreItems.Add("prov", "/Resources/4.png");
        }

        public async Task<List<string>> GetOwnedItems()
        {
            List<string> items = new List<string>();

            ListingInformation li = await CurrentApp.LoadListingInformationAsync();

            foreach (string key in li.ProductListings.Keys)
            {
                if (CurrentApp.LicenseInformation.ProductLicenses[key].IsActive && StoreItems.ContainsKey(key))
                    items.Add(StoreItems[key]);
            }

            return items;
        }
    }
}
