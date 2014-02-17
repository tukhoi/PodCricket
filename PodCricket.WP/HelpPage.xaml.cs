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
            if (CultureInfo.CurrentUICulture.ToString().StartsWith("vi"))
                content.Source = new Uri(@"Resources\help\help-vi.html", UriKind.Relative);
            else
                content.Source = new Uri(@"Resources\help\help.html", UriKind.Relative);

            base.OnNavigatedTo(e);
        }
    }
}