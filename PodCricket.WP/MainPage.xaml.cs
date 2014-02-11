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
using System.Threading.Tasks;
using PodCricket.Utilities.Extensions;
using Microsoft.Phone.BackgroundTransfer;
using System.Collections.ObjectModel;
using System.IO;
using PodCricket.Utilities.Toolkit;
using Microsoft.Phone.Tasks;
using PodCricket.WP.Helper;
using PodCricket.Utilities.Helpers;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;
using PodCricket.WP.Resources;

namespace PodCricket.WP
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainPageModel _mainModel = null;
        IPodManager _podManager = null;
        StreamModel _currentPlayingStreamModel = null;

        DispatcherTimer _currentPlayingStreamPositionTimer = new DispatcherTimer();
        double _currentTrackLastKnownPosition = 0.0;

        public MainPage()
        {
            InitializeComponent();

            _mainModel = new MainPageModel();
            _podManager = PodManager.Instance();
            _currentPlayingStreamModel = null;
        }

        #region Override

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.CreateAppBar();
            this.SetProgressIndicator(true, AppResources.OpeningTitle);
            await Reload();
            this.SetProgressIndicator(false);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SaveCurrentPlaying();
            mediaElement.Stop();

            base.OnNavigatedFrom(e);
        }

        private void LoadCurrentPlaying()
        { 
            var currentPlaying = _podManager.GetCurrentPlayingStream();
            if (currentPlaying.HasError) return;

            var streamResult = _podManager.FindStream(currentPlaying.Target.Key);
            if (streamResult.HasError) return;

            _currentPlayingStreamModel = new StreamModel().GetFrom(streamResult.Target);

            double currentTrackPosition;
            if (double.TryParse(currentPlaying.Target.Value, out currentTrackPosition))
                _currentTrackLastKnownPosition = currentTrackPosition;
            else
                _currentTrackLastKnownPosition = 0;

            _podManager.QueueToPlay(streamResult.Target);
        }

        private void SaveCurrentPlaying()
        {
            string currentStream = string.Empty;
            string currentStreamPosition = "0";

            if (_currentPlayingStreamModel != null)
            {
                currentStream = _currentPlayingStreamModel.Id;
                currentStreamPosition = mediaElement.Position.TotalMinutes.ToString();
            }

            var currentPlaying = new KeyValuePair<string, string>(currentStream, currentStreamPosition);
            _podManager.SaveCurrentPlayingStream(currentPlaying);
        }

        #endregion

        #region Common

        /// <summary>
        /// This pulls back registered list from PodManager
        /// It also pulls back searched list from memory without making call to itune service
        /// (if searchterms was found)
        /// </summary>
        private void Binding()
        {
            BindSubscribedPods();
            BindSearchedPods();
            BindDownloadingStreams();
            if (AppConfig.Instance().AutoResumeStream)
                LoadCurrentPlaying();
            BindPlayingList();

            RefreshPlayPage();

            MainPanorama.DataContext = _mainModel;
        }

        private void mnuSetting_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ConfigurationPage.xaml", UriKind.Relative));
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private async Task Reload()
        {
            //await _podManager.LoadSettings();
            //await _podManager.LoadPodMapAsync();

            //if (!popMapLoadResult)
            //    MessageBox.Show("Error while loading registered pod casts.");

            //var downloadingResult = await _podManager.LoadDownloadingStreams();
            //if (!downloadingResult)
            //    MessageBox.Show("Error while loading downloading streams.");
            
            await Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(() => Binding()));
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            var settingButton = new ApplicationBarIconButton();
            settingButton.Text = AppResources.AppBarSettingTitle;
            settingButton.IconUri = new Uri("/Assets/AppBar/feature.settings.png", UriKind.Relative);
            settingButton.Click += new EventHandler(mnuSetting_Click);

            var aboutButton = new ApplicationBarIconButton();
            aboutButton.Text = AppResources.AppBarAboutTitle;
            aboutButton.IconUri = new Uri("/Assets/AppBar/like.png", UriKind.Relative);
            aboutButton.Click += new EventHandler(mnuAbout_Click);

            ApplicationBar.Buttons.Add(settingButton);
            ApplicationBar.Buttons.Add(aboutButton);
        }

        #endregion

        #region Subscribed

        private void OnPictureItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                var pod = fe.DataContext as PodModel;
                if (pod != null)
                {
                    var uri = string.Format("/PodDetails.xaml?id={0}&terms={1}", pod.Id, txtSearch.Text.Trim());
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }
        }

        private void BindSubscribedPods()
        {
            //Get subscribed pods
            var result = _podManager.GetSubscribedPod();
            if (!result.HasError)
            {
                _mainModel.ClearPod();
                result.Target.ForEach(p => _mainModel.AddPod(new PodModel().GetFrom(p)));
            }

            txtSubcribedStatus.Visibility = _mainModel.PodList.Count != 0 ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            if (AppConfig.Instance().SubscribedPodsAsGrid)
            {
                llsGridSubscribedPods.Visibility = System.Windows.Visibility.Visible;
                llsSubscribedPods.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                llsGridSubscribedPods.Visibility = System.Windows.Visibility.Collapsed;
                llsSubscribedPods.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private async void ctxRegister_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (sender as MenuItem).DataContext;
            var podModel = dataContext as PodModel;

            if (podModel == null) return;

            var podId = podModel.Id;
            if (podId.Equals(default(Guid))) return;

            AppResult<bool> result = null;
            string message = string.Empty;

            if (podModel.Subscribed)
            {
                if (MessageBox.Show(AppResources.AreYouSureTitle, AppResources.ApplicationTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    this.SetProgressIndicator(true, AppResources.UnsubscribeTitle + "...");
                    result = await _podManager.UnSubscribePodAsync(podModel.Id);
                    this.SetProgressIndicator(false);
                    message = AppResources.UnsubscribeSuccessfullyTitle;
                }
                else return;
            }
            else
            {
                this.SetProgressIndicator(true, AppResources.SubscribeTitle + "...");
                result = await _podManager.SubscribePodAsync(podModel.Id);
                this.SetProgressIndicator(false);
                message = AppResources.SubscribeSuccessfullyTitle;
            }

            if (!result.HasError)
            {
                ToastMessage.Show(message, AppResources.ApplicationTitle);
                await Reload();
            }
            else
                ToastMessage.Show(result.ErrorMessage());
        }

        #endregion

        #region Search

        private void BindSearchedPods()
        {
            //Get searched pods
            string terms = NavigationContext.QueryString.GetQueryString("terms");
            if (!string.IsNullOrEmpty(terms))
            {
                var searchResult = _podManager.GetSearchedPod(terms);
                if (!searchResult.HasError)
                {
                    _mainModel.ClearSearch();
                    searchResult.Target.ForEach(p => _mainModel.AddSearch(new PodModel().GetFrom(p)));
                }
                txtSearch.Text = terms;
            }
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            if (!ConnectivityHelper.NetworkAvailable())
            {
                ToastMessage.Show(AppResources.ErrNetworkNotAvailable);
                return;
            }

            try
            {
                this.SetProgressIndicator(true, AppResources.SearchingTitle);

                var result = await _podManager.SearchPod(txtSearch.Text);
                if (result.HasError) return;

                _mainModel.ClearSearch();
                result.Target.ForEach(p => _mainModel.AddSearch(new PodModel().GetFrom(p)));

                MainPanorama.DataContext = _mainModel;

                this.SetProgressIndicator(false);
            }
            catch (Exception)
            {
                ToastMessage.Show(AppResources.SearchingErrorTitle);
            }
        }

        private void llsPodSearchList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                var pod = fe.DataContext as PodModel;
                if (pod != null)
                {
                    var uri = string.Format("/PodDetails.xaml?id={0}&terms={1}", pod.Id, txtSearch.Text.Trim());
                    NavigationService.Navigate(new Uri(uri, UriKind.Relative));
                }
            }

            //var podId = GetPodIdFromListTapped(llsPodSearchList.SelectedItem);
            //if (podId.Equals(default(Guid))) return;

            //var uri = string.Format("/PodDetails.xaml?id={0}&terms={1}", podId, txtSearch.Text);
            //NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        #endregion

        #region Download

        private async void BindDownloadingStreams()
        {
            _mainModel.TransferMonitorList.Clear();
            var requestsToRemove = new List<BackgroundTransferRequest>();

            foreach (var request in BackgroundTransferService.Requests)
            {
                if (request.Tag == null) return;

                var streamResult = _podManager.FindStream(request.Tag);
                if (streamResult.HasError)
                {
                    requestsToRemove.Add(request);
                    break;
                }

                var stream = streamResult.Target;

                //Check again if download already finished in background while
                //app isn't in foreground, means monitor_completed hasn't been called
                //we need to call it now
                if (request.TransferStatus == TransferStatus.Completed)
                {
                    if (stream.DownloadState != DownloadState.Downloaded)
                    {
                        var downloadResult = await _podManager.MarkDownloadFinished(stream);
                        if (!downloadResult.HasError)
                        {
                            if (AppConfig.Instance().QueueStreamToPlayListAfterDownloading)
                                _podManager.QueueToPlay(stream);
                        }
                    }

                    if (AppConfig.Instance().AutoRemoveCompletedDownload)
                    {
                        requestsToRemove.Add(request);
                        break;
                    }
                }

                if (request.TransferStatus == TransferStatus.Transferring)
                {
                    if (stream.DownloadState != DownloadState.Downloaded)
                        _podManager.MarkDownloading(stream);
                }

                //Actual binding
                var monitor = new DownloadMonitor(request);
                //var monitor = new TransferMonitor(request);
                var podResult = _podManager.GetPod(stream.PodId);
                if (!podResult.HasError)
                {
                    monitor.StreamTitle = stream.Title;
                    monitor.PodName = podResult.Target.Name;
                    Uri imageUri;
                    if (Uri.TryCreate(podResult.Target.ImageUrl, UriKind.Absolute, out imageUri))
                        monitor.ImageUri = imageUri;
                    else
                        monitor.ImageUri = new Uri(@"Resources/default-pod.png", UriKind.RelativeOrAbsolute);
                }

                SubscribeMonitor(monitor);
                monitor.Tag = stream;
                monitor.RequestId = request.RequestId;
                _mainModel.TransferMonitorList.Add(monitor);
            }

            requestsToRemove.ForEach(r => BackgroundTransferService.Remove(r));

            txtDownloadStatus.Visibility = _mainModel.TransferMonitorList.Count != 0 ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            //llsDownloading.ItemsSource = _mainModel.TransferMonitorList;
        }

        private void DownloadControl_AddToPlay(object sender, EventArgs e)
        {
            var dataContext = (sender as MenuItem).DataContext;
            var monitor = dataContext as DownloadMonitor;

            if (monitor == null) return;

            var queueResult = _podManager.QueueToPlay(monitor.Tag as PodCricket.ApplicationServices.Stream);
            if (queueResult.HasError)
                ToastMessage.Show(queueResult.ErrorMessage());
            else
            {
                ToastMessage.Show(AppResources.AddedToPlayListTitle);
                BindPlayingList();
                this.DataContext = _mainModel;
            }
        }

        private void llsDownloading_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var fe = sender as LongListSelector;
            if (fe != null)
            {
                var monitor = fe.SelectedItem as DownloadMonitor;
                if (monitor != null)
                {
                    var stream = monitor.Tag as PodCricket.ApplicationServices.Stream;
                    if (stream == null) return;

                    var streamDetailsUri = string.Format("/StreamDetails.xaml?podId={0}&streamId={1}", stream.PodId, stream.Id);
                    NavigationService.Navigate(new Uri(streamDetailsUri, UriKind.Relative));
                }
            }
        }

        private void SubscribeMonitor(TransferMonitor monitor)
        {
            UnSubscribeMonitor(monitor);
            //monitor.Failed += TransferCanceled;
            monitor.Failed += monitor_Failed;
            monitor.Complete += monitor_Complete;
        }

        private void UnSubscribeMonitor(TransferMonitor monitor)
        {
            //monitor.Failed -= TransferCanceled;
            monitor.Failed += monitor_Failed;
            monitor.Complete += monitor_Complete;
        }

        void monitor_Complete(object sender, BackgroundTransferEventArgs e)
        {
            if (AppConfig.Instance().QueueStreamToPlayListAfterDownloading)
            {
                BindPlayingList();
                this.DataContext = _mainModel;
            }
        }

        void monitor_Failed(object sender, BackgroundTransferEventArgs e)
        {
            var monitor = sender as DownloadMonitor;
            if (monitor == null) return;

            var stream = monitor.Tag as PodCricket.ApplicationServices.Stream;
            if (stream == null) return;

            _podManager.MarkDownloadFailed(stream);
        }

        #endregion

        #region Play

        private void ctxRemove_Click(object sender, RoutedEventArgs e)
        {
            var streamModel = (sender as MenuItem).DataContext as StreamModel;
            if (streamModel == null) return;

            var podResult = _podManager.GetPod(streamModel.PodId);
            if (podResult.HasError) return;

            var stream = podResult.Target.StreamList.FirstOrDefault(s => s.Id == streamModel.Id);
            if (stream == null) return;

            _podManager.RemoveFromPlay(stream);

            //Remove current playing stream
            if (_currentPlayingStreamModel != null && streamModel.Id.Equals(_currentPlayingStreamModel.Id))
            {
                _currentPlayingStreamModel = null;
                RefreshPlayPage();
            }

            BindPlayingList();
            
            this.DataContext = _mainModel;
        }

        private void RefreshPlayPage()
        {
            sldPlaying.Value = 0;
            songTime.Text = "0";
            mediaElement.Stop();
            mediaElement.Source = null;

            if (_currentPlayingStreamModel == null)
            {
                txtPlayStatus.Text = AppResources.MainPagePlayStatus;
                txtTrackTimeCaption.Visibility = System.Windows.Visibility.Collapsed;

                sldPlaying.Visibility = System.Windows.Visibility.Collapsed;
                play_btn.Visibility = System.Windows.Visibility.Collapsed;
                pause_btn.Visibility = System.Windows.Visibility.Collapsed;
                songTime.Visibility = System.Windows.Visibility.Collapsed;

                play_btn.IsEnabled = false;
                pause_btn.IsEnabled = false;

                play_btn.IsHitTestVisible = false;
                pause_btn.IsHitTestVisible = false;
            }
            else
            {
                txtPlayStatus.Text = _currentPlayingStreamModel.Title;
                txtTrackTimeCaption.Visibility = System.Windows.Visibility.Visible;

                sldPlaying.Visibility = System.Windows.Visibility.Visible;
                play_btn.Visibility = System.Windows.Visibility.Visible;
                pause_btn.Visibility = System.Windows.Visibility.Collapsed;
                songTime.Visibility = System.Windows.Visibility.Visible;

                play_btn.IsEnabled = true;
                pause_btn.IsEnabled = false;

                play_btn.IsHitTestVisible = true;
                pause_btn.IsHitTestVisible = false;
            }
        }

        private void BindPlayingList()
        {
            var result = _podManager.GetPlayList();

            if (result.HasError && result.Error == ErrorCode.NoStreamInPlayList)
                _mainModel.PlayingList.Clear();

            if (!result.HasError)
            {
                _mainModel.PlayingList.Clear();
                result.Target.ForEach(s => _mainModel.PlayingList.Add(new StreamModel().GetFrom(s)));
            }
        }

        void LoadPlayingStream()
        {
            if (_currentPlayingStreamModel == null) return;

            mediaElement.MediaOpened += new RoutedEventHandler(mediaElement_MediaOpened);
            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);
            mediaElement.CurrentStateChanged += new RoutedEventHandler(mediaElement_CurrentStateChanged);
            _currentPlayingStreamPositionTimer.Tick += new EventHandler(currentPositionTimer_Tick);

            MediaHelper.BindSourceUri(mediaElement, _currentPlayingStreamModel);
        }

        void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (mediaElement.CurrentState == MediaElementState.Buffering
                || mediaElement.CurrentState == MediaElementState.Opening)
            {
                _currentPlayingStreamPositionTimer.Stop();
                this.SetProgressIndicator(true, AppResources.StreammingTitle);

                _currentTrackLastKnownPosition = LoadCurrentLastKnownPostionOfCurrentStream();
            }
            else
                this.SetProgressIndicator(false);

            if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                if (AppConfig.Instance().KeepScreenOn)
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                _currentPlayingStreamPositionTimer.Start();

                // add play position here
                //var app = Application.Current as PodCricket.WP.App;
                //if (app == null) return;
                //if (app._activated)
                //{
                //    app._activated = false;

                //    mediaElement.Stop();
                //    mediaElement.Play();
                //    mediaElement.Position = TimeSpan.FromMinutes(_currentTrackLastKnownPosition);
                //}

                songTime.Visibility = Visibility.Visible;
                play_btn.Visibility = System.Windows.Visibility.Collapsed;
                pause_btn.Visibility = System.Windows.Visibility.Visible;

                play_btn.IsEnabled = false;
                pause_btn.IsEnabled = true;

                play_btn.IsHitTestVisible = false;
                pause_btn.IsHitTestVisible = true;

                mediaElement.Position = TimeSpan.FromMinutes(_currentTrackLastKnownPosition);
            }
            else
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = App._originalIdleDectectionMode;   

                if (mediaElement.CurrentState == MediaElementState.Paused)
                {
                    _currentPlayingStreamPositionTimer.Stop();

                    play_btn.Visibility = Visibility.Visible;
                    pause_btn.Visibility = Visibility.Collapsed;

                    play_btn.IsEnabled = true;
                    pause_btn.IsEnabled = false;

                    play_btn.IsHitTestVisible = true;
                    pause_btn.IsHitTestVisible = false;
                }
                else
                    _currentPlayingStreamPositionTimer.Stop();
            }
        }

        private double LoadCurrentLastKnownPostionOfCurrentStream()
        {
            var result = _podManager.GetCurrentPlayingStream();
            if (result.HasError) return 0;

            if (_currentPlayingStreamModel != null && _currentPlayingStreamModel.Id.Equals(result.Target.Key))
            {
                double position = 0;
                if (double.TryParse(result.Target.Value, out position))
                    return position;
            }

            return 0;
        }

        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }

        void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            sldPlaying.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            mediaElement.Play();
        }

        void currentPositionTimer_Tick(object sender, EventArgs e)
        {
            sldPlaying.Value = mediaElement.Position.TotalMilliseconds;
            songTime.Text = "-" + FormatTime((int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds - (int)mediaElement.Position.TotalSeconds);
            // (int)mediaElement.Position.TotalSeconds - (int)mediaElement.Position.TotalSeconds + "";
        }

        private void sldPlaying_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            _currentPlayingStreamPositionTimer.Stop();
        }

        private void sldPlaying_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromMilliseconds(sldPlaying.Value);
            _currentTrackLastKnownPosition = mediaElement.Position.TotalMinutes;
            _currentPlayingStreamPositionTimer.Start();
        }

        string FormatTime(int time)
        {
            double dtime = (double)time;

            string min = Math.Floor(dtime / 60) < 10 ? "0" + Math.Floor(dtime / 60).ToString() : Math.Floor(dtime / 60).ToString();
            string sec = Math.Floor(dtime % 60) < 10 ? "0" + Math.Floor(dtime % 60).ToString() : Math.Floor(dtime % 60).ToString();
            return min + ":" + sec;
        }

        private void play_btn_Click(object sender, RoutedEventArgs e)
        {
            if (AppConfig.Instance().PlayStreamInApp)
            {
                if (mediaElement.Source == null)
                    LoadPlayingStream();
                mediaElement.Play();
            }
            else
                if (_currentPlayingStreamModel != null)
                    CallPlayLauncher();
        }

        private void pause_btn_Click(object sender, RoutedEventArgs e)
        {
            _currentTrackLastKnownPosition = mediaElement.Position.TotalMinutes;
            mediaElement.Pause();
        }

        private void txtStreamToPlay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var streamModel = (sender as TextBlock).DataContext as StreamModel;
            if (streamModel == null) return;

            if (_currentPlayingStreamModel != null && streamModel.Id.Equals(_currentPlayingStreamModel.Id)) return;

            _currentPlayingStreamModel = streamModel;
            RefreshPlayPage();
        }

        //private void mediaPlaying_MediaOpened(object sender, RoutedEventArgs e)
        //{
        //    txtPlayingUri.Text = "Playing " + mediaPlaying.Source.ToString();
        //}

        private void PlayStreamInApp()
        {
            //if (_playingStreamModel == null) return;

            //MediaHelper.GetMediaElement(mediaPlaying, _playingStreamModel);
            //txtPlayingUri.Text = "Streamming " + mediaPlaying.Source.ToString();
        }

        private void CallPlayLauncher()
        {
            if (_currentPlayingStreamModel == null) return;

            var mediaPlayerLauncher = new MediaPlayerLauncher();

            MediaHelper.BindSourceUri(mediaPlayerLauncher, _currentPlayingStreamModel);
            mediaPlayerLauncher.Show();
        }

        #endregion
    }
}