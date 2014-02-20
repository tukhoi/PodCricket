using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PodCricket.WP.ViewModels;
using PodCricket.Utilities.Extensions;
using PodCricket.ApplicationServices;
using Microsoft.Phone.Tasks;
using PodCricket.WP.Helper;
using PodCricket.WP.Resources;

namespace PodCricket.WP
{
    public partial class StreamDetails : PhoneApplicationPage
    {
        PodDetailModel _podDetailModel;
        private int _currentIndex;
        private IPodManager _podManager;

        public StreamDetails()
        {
            InitializeComponent();
            
            _podManager = PodManager.Instance();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CreateAppBar();
            this.SetProgressIndicator(true, AppResources.LoadingTitle);

            var pod = ReloadPod();
            if (pod == null) this.BackToMainPage();

            //var refreshResult = await _podManager.GetStreamList(pod);
            //if (refreshResult.HasError)
            //    ToastMessage.Show(refreshResult.ErrorMessage);

            BindData();

            this.SetProgressIndicator(false);

            base.OnNavigatedTo(e);
        }

        private void OnFlick(object sender, FlickGestureEventArgs e) {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.HorizontalVelocity < 0)
                    LoadNextStream();
                else
                    LoadPreviousStream();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.BackToPreviousPage();
        }

        private void mnuPlay_Click(object sender, EventArgs e)
        {
            var streamModel = _podDetailModel.StreamList[_currentIndex];
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

        private void mnuDownload_Click(object sender, EventArgs e)
        {
            if (_currentIndex < 0 || _currentIndex > _podDetailModel.StreamList.Count)
                return;

            var podResult = _podManager.GetPod(_podDetailModel.Id);
            if (podResult.HasError){
                Messenger.ShowToast(podResult.ErrorMessage());
                return;
            }

            var streamId = _podDetailModel.StreamList[_currentIndex].Id;
            var stream = podResult.Target.StreamList.FirstOrDefault(s => s.Id.Equals(streamId));
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
            }

            //if (queueResult.HasError)
            //    if (queueResult.Error == ErrorCode.StreamAlreadyInDownloading &&
            //        MessageBox.Show(AppResources.ReDownloadTitle, AppResources.ApplicationTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            //        queueResult = _podManager.QueueToDownload(stream, true);
            //    else
            //    Messenger.ShowToast(queueResult.ErrorMessage());
            
            //if (!queueResult.HasError)
            //    Messenger.ShowToast(AppResources.AddedToDownloadTitle);
            //else
            //    Messenger.ShowToast(queueResult.ErrorMessage());
        }

        private void LoadNextStream()
        {
            if (_currentIndex == _podDetailModel.StreamList.Count() - 1)
                _currentIndex = 0;

            else _currentIndex++;

            BindData();
        }

        private void LoadPreviousStream() 
        {
            if (_currentIndex == 0 && _podDetailModel.StreamList.Count() > 0)
                _currentIndex = _podDetailModel.StreamList.Count - 1;
            else
                _currentIndex--;

            BindData();
        }

        private void BindData()
        {
            //_podDetailModel = new PodDetailModel().GetFrom(pod);
            var streamModel = _podDetailModel.StreamList[_currentIndex];
            this.DataContext = streamModel;
        }

        private Pod ReloadPod()
        {
            string podIdString = NavigationContext.QueryString.GetQueryString("podId");
            string streamId = NavigationContext.QueryString.GetQueryString("streamId");

            if (string.IsNullOrEmpty(podIdString)) return null;

            var podId = new Guid(podIdString);
            if (podId.Equals(default(Guid))) return null;

            var result = _podManager.GetPod(podId);
            if (result.HasError) return null;

            _podDetailModel = new PodDetailModel().GetFrom(result.Target);

            _currentIndex = _podDetailModel.StreamList.IndexOf(_podDetailModel.StreamList.FirstOrDefault(x => x.Id == streamId));
            if (_currentIndex == -1 && _podDetailModel.StreamList.Count > 0)
                _currentIndex = 0;

            return result.Target;
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            var downloadButton = new ApplicationBarIconButton();
            downloadButton.Text = AppResources.AppBarDownloadTitlle;
            downloadButton.IconUri = new Uri("/Assets/AppBar/download.png", UriKind.Relative);
            downloadButton.Click += new EventHandler(mnuDownload_Click);

            var playButton = new ApplicationBarIconButton();
            playButton.Text = AppResources.AppBarPlayTitle;
            playButton.IconUri = new Uri("/Assets/AppBar/transport.play.png", UriKind.Relative);
            playButton.Click += new EventHandler(mnuPlay_Click);

            ApplicationBar.Buttons.Add(downloadButton);
            ApplicationBar.Buttons.Add(playButton);
        }
    }
}