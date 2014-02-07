using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PodCricket.ApplicationServices;
using PodCricket.Utilities.Extensions;
using System.Threading.Tasks;
using PodCricket.WP.Resources;

namespace PodCricket.WP
{
    public partial class ConfigurationPage : PhoneApplicationPage
    {
        public ConfigurationPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SetProgressIndicator(true, AppResources.OpeningTitle);
            SetEnable(false);

            await Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(() => LoadConfig()));

            SetEnable(true);
            this.SetProgressIndicator(false);
            base.OnNavigatedTo(e);
        }

        private void LoadConfig() 
        {
            chkSubscribedPodsAsGrid.IsChecked = AppConfig.Instance().SubscribedPodsAsGrid;
            chkQueueAfterDownload.IsChecked = AppConfig.Instance().QueueStreamToPlayListAfterDownloading;
            chkAutoRemoveCompletedDownload.IsChecked = AppConfig.Instance().AutoRemoveCompletedDownload;
            chkJustDownloadWithWifi.IsChecked = AppConfig.Instance().JustDownloadWithWifi;
            chkPlayStreamInApp.IsChecked = AppConfig.Instance().PlayStreamInApp;
            chkAutoResumeStream.IsChecked = AppConfig.Instance().AutoResumeStream;
            chkAutoBackup.IsChecked = AppConfig.Instance().AutoBackup;
            chkKeepScreenOn.IsChecked = AppConfig.Instance().KeepScreenOn;
        }

        private void SetEnable(bool enabled)
        {
            this.chkSubscribedPodsAsGrid.IsEnabled = enabled;
            this.chkQueueAfterDownload.IsEnabled = enabled;
            this.chkAutoResumeStream.IsEnabled = enabled;
            this.chkAutoRemoveCompletedDownload.IsEnabled = enabled;
            this.chkPlayStreamInApp.IsEnabled = enabled;
            this.chkJustDownloadWithWifi.IsEnabled = enabled;
            this.chkAutoBackup.IsEnabled = enabled;
            this.chkKeepScreenOn.IsEnabled = enabled;
        }

        #region Event Handler

        private void chkSubscribedPodsAsGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().SubscribedPodsAsGrid = chkSubscribedPodsAsGrid.IsChecked.HasValue ?
                chkSubscribedPodsAsGrid.IsChecked.Value : true;
        }

        private void chkQueueAfterDownload_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().QueueStreamToPlayListAfterDownloading = chkQueueAfterDownload.IsChecked.HasValue ?
                chkQueueAfterDownload.IsChecked.Value : true;
        }

        private void chkPlayStreamInApp_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().PlayStreamInApp = chkPlayStreamInApp.IsChecked.HasValue ?
                chkPlayStreamInApp.IsChecked.Value : true;
        }

        private void chkAutoResumeStream_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().AutoResumeStream = chkAutoResumeStream.IsChecked.HasValue ?
                chkAutoResumeStream.IsChecked.Value : true;
        }

        private void chkAutoRemoveCompletedDownload_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().AutoRemoveCompletedDownload = chkAutoRemoveCompletedDownload.IsChecked.HasValue ?
                chkAutoRemoveCompletedDownload.IsChecked.Value : false;
        }

        private void chkJustDownloadWithWifi_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().JustDownloadWithWifi = chkJustDownloadWithWifi.IsChecked.HasValue ?
                chkJustDownloadWithWifi.IsChecked.Value : true;
        }

        private void chkAutoBackup_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().AutoBackup = chkAutoBackup.IsChecked.HasValue ?
                chkAutoBackup.IsChecked.Value : true;
        }

        private void chkKeepScreenOn_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AppConfig.Instance().KeepScreenOn = chkKeepScreenOn.IsChecked.HasValue ?
                chkKeepScreenOn.IsChecked.Value : true;
        }

        #endregion
    }
}