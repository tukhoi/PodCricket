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
using PodCricket.WP.ViewModels;
using PodCricket.Utilities.Extensions;
using System.Collections.ObjectModel;
using PodCricket.WP.Helper;
using PodCricket.Utilities.Helpers;
using PodCricket.WP.Resources;

namespace PodCricket.WP
{
    public partial class PodDetails : PhoneApplicationPage
    {
        IPodManager _podManager;
        PodDetailModel _model;

        public PodDetails()
        {
            InitializeComponent();

            _podManager = PodManager.Instance();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Binding();
            base.OnNavigatedTo(e);
        }

        private async void btnShowStream_Click(object sender, EventArgs e)
        {
            if (_model == null) this.BackToMainPage();

            if (!ConnectivityHelper.NetworkAvailable())
            {
                Messenger.ShowToast(AppResources.ErrNetworkNotAvailable);
                return;
            }

            try
            {
                var result = _podManager.GetPod(_model.Id);
                if (result.HasError) this.BackToMainPage();

                this.SetProgressIndicator(true, AppResources.LoadingTitle);
                var refreshResult = await _podManager.GetStreamList(result.Target);

                if (refreshResult.HasError)
                    if (refreshResult.Error == ErrorCode.LicenseRequiredForVideo)
                        Messenger.ShowBuyLicense();
                    else
                        Messenger.ShowToast(refreshResult.ErrorMessage());

                _model = new PodDetailModel().GetFrom(result.Target);

                this.DataContext = _model;
            }
            catch (Exception)
            {
                Messenger.ShowToast(AppResources.LoadingErrorTitle);
            }
            this.SetProgressIndicator(false);
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            var message = string.Empty;
            if (!_model.Subscribed)
            {
                this.SetProgressIndicator(true, AppResources.SubscribeTitle + "...");
                var result = await _podManager.SubscribePodAsync(_model.Id);
                this.SetProgressIndicator(false);
                message = result.HasError ? result.ErrorMessage() : AppResources.SubscribeSuccessfullyTitle;
            }

            if (_model.Subscribed)
            {
                if (MessageBox.Show(AppResources.AreYouSureTitle, AppResources.ApplicationTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    this.SetProgressIndicator(true, AppResources.UnsubscribeTitle + "...");
                    var result = await _podManager.UnSubscribePodAsync(_model.Id);
                    this.SetProgressIndicator(false);
                    message = result.HasError ? result.ErrorMessage() : AppResources.UnsubscribeSuccessfullyTitle;
                }
                else
                    return;
            }

            Messenger.ShowToast(message);
            Binding();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as MenuItem).DataContext;
            var streamModel = dataContext as StreamModel;

            if (streamModel == null) return;

            var podResult = _podManager.GetPod(streamModel.PodId);
            if (podResult.HasError) return;

            var stream = podResult.Target.StreamList.FirstOrDefault(x => x.Id == streamModel.Id);
            if (stream == null) return;

            var queueResult = _podManager.QueueToDownload(stream);
            if (queueResult.HasError)
            {
                if (queueResult.Error == ErrorCode.StreamAlreadyInDownloading &&
                    MessageBox.Show(AppResources.ReDownloadTitle, AppResources.ApplicationTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    queueResult = _podManager.QueueToDownload(stream, true);
                    if (queueResult.HasError)
                        Messenger.ShowToast(queueResult.ErrorMessage());
                    else
                        Messenger.ShowToast(AppResources.AddedToDownloadTitle);
                    return;
                }
                else 
                    if (queueResult.Error == ErrorCode.LicenseRequiredForVideo)
                    {
                        Messenger.ShowBuyLicense();
                    }
                    else
                        Messenger.ShowToast(queueResult.ErrorMessage());
            }
            else// if (!queueResult.HasError)
            {
                Messenger.ShowToast(AppResources.AddedToDownloadTitle);
                Binding();
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as MenuItem).DataContext;
            var streamModel = dataContext as StreamModel;

            if (streamModel == null) return;
            
            var podResult = _podManager.GetPod(streamModel.PodId);
            if (podResult.HasError) return;

            var stream = podResult.Target.StreamList.FirstOrDefault(x => x.Id == streamModel.Id);
            if (stream == null) return;

            var queueResult = _podManager.QueueToPlay(stream);
            if (!queueResult.HasError)
                Messenger.ShowToast(AppResources.AddedToPlayListTitle);
            else
                if (queueResult.Error == ErrorCode.LicenseRequiredForVideo)
                    Messenger.ShowBuyLicense();
                else
                    Messenger.ShowToast(queueResult.ErrorMessage());
        }

        private void btnDeleteDownloadedFile_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as MenuItem).DataContext;
            var streamModel = dataContext as StreamModel;

            if (streamModel == null) return;

            var streamResult = _podManager.FindStream(streamModel.Id);
            if (streamResult.HasError) return;
            if (streamResult.Target.DownloadState == DownloadState.None)
            {
                Messenger.ShowToast(AppResources.ErrStreamHasNotDownloaded);
                return;
            }

            var deleteResult = _podManager.DeleteDownloadedStream(streamResult.Target);

            if (deleteResult.HasError)
                Messenger.ShowToast(deleteResult.ErrorMessage());
            else
            {
                Messenger.ShowToast(AppResources.DeletedDownloadedFile);
                Binding();
            }
        }

        private void llsPodStreamList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var streamModel = llsPodStreamList.SelectedItem as StreamModel;

            if (streamModel == null) return;

            var streamId = streamModel.Id;
            if (string.IsNullOrEmpty(streamId)) return;

            var streamDetailsUri = string.Format("/StreamDetails.xaml?podId={0}&streamId={1}", _model.Id.ToString(), streamId);
            NavigationService.Navigate(new Uri(streamDetailsUri, UriKind.Relative));
        }

        private void OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalVelocity > 0)
                    this.BackToPreviousPage();
            }
        }

        private void Binding()
        {
            string id = NavigationContext.QueryString.GetQueryString("id");

            if (string.IsNullOrEmpty(id)) return;

            var podId = new Guid(id);
            if (podId.Equals(default(Guid))) return;

            var result = _podManager.GetPod(podId);
            if (result.HasError) this.BackToMainPage();

            _model = new PodDetailModel().GetFrom(result.Target);
            CreateAppBar(_model.Subscribed);
            this.DataContext = _model;
        }

        private void CreateAppBar(bool subscribed)
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            var refreshButton = new ApplicationBarIconButton();
            refreshButton.Text = AppResources.AppBarRefreshTitle;
            refreshButton.IconUri = new Uri("/Assets/AppBar/refresh.png", UriKind.Relative);
            refreshButton.Click += new EventHandler(btnShowStream_Click);

            var subscribeCaption = subscribed ? AppResources.UnsubscribeTitle : AppResources.SubscribeTitle;
            var iconUri = subscribed ? new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative) : 
                new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative);

            ApplicationBarIconButton subscribeButton = new ApplicationBarIconButton();
            subscribeButton.Text = subscribeCaption;
            subscribeButton.IconUri = iconUri;
            subscribeButton.Click += new EventHandler(btnRegister_Click);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(subscribeButton);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            GA.LogPage(this.ToString());
        }
    }
}