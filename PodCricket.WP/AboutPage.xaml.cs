using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PodCricket.Utilities.AppLicense;
using PodCricket.ApplicationServices;
using PodCricket.WP.Resources;
using PodCricket.Utilities.Extensions;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
using PodCricket.WP.Helper;
#else
using Windows.ApplicationModel.Store;
using Store = Windows.ApplicationModel.Store;
using PodCricket.WP.Helper;
#endif

namespace PodCricket.WP
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Binding();   
            base.OnNavigatedTo(e);
        }

        private void btnRating_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask oRateTask = new MarketplaceReviewTask();
            oRateTask.Show();
        }

        private void btnPro_Click(object sender, RoutedEventArgs e)
        {
            LicenseHelper.PurchaseProduct(AppConfig.PRO_VERSION);

            Binding();

            //if (!LicenseHelper.Purchased(AppConfig.PRO_VERSION))
            //{
            //    try
            //    {
            //        ListingInformation productListing = await Store.CurrentApp.LoadListingInformationAsync();
            //        if (productListing != null && productListing.ProductListings.ContainsKey(AppConfig.PRO_VERSION))
            //        {
            //            string proProduct = productListing.ProductListings[AppConfig.PRO_VERSION].ProductId;
            //            string receipt = await Store.CurrentApp.RequestProductPurchaseAsync(proProduct, false);

            //            Binding();
            //            CurrentApp.ReportProductFulfillment(AppConfig.PRO_VERSION);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        //ToastMessage.Show();
            //    }
            //}
        }

        private void Binding()
        {
            var purchased = LicenseHelper.Purchased(AppConfig.PRO_VERSION);
            if (purchased)
            {
                btnPro.Visibility = System.Windows.Visibility.Collapsed;
                abtVersion.Text = AppResources.AbtVersionPro;
            }
            else
            {
                btnPro.Visibility = System.Windows.Visibility.Visible;
                abtVersion.Text = AppResources.AbtVersion;
            }
        }

        private void OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalVelocity > 0)
                    this.BackToPreviousPage();
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            GA.LogPage(this.ToString());
        }
    }
}