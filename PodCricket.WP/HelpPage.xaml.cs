using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Globalization;
using PodCricket.Utilities.Extensions;
using PodCricket.WP.Resources;

namespace PodCricket.WP
{
    public partial class HelpPage : PhoneApplicationPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SetProgressIndicator(true, AppResources.OpeningTitle);
            if (CultureInfo.CurrentUICulture.ToString().StartsWith("vi"))
                content.Source = new Uri(@"Resources\help\help-vi.html", UriKind.Relative);
            else
                content.Source = new Uri(@"Resources\help\help.html", UriKind.Relative);
            this.SetProgressIndicator(false);
            base.OnNavigatedTo(e);
        }

        private void OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalVelocity > 0)
                    this.BackToPreviousPage();
            }
        }
    }
}