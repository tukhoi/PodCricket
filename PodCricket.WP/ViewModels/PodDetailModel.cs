using PodCricket.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PodCricket.Utilities.Extensions;
using System.Windows;
using System.Windows.Media;
using PodCricket.Utilities.Helpers;
using System.IO;
using PodCricket.ApplicationServices.Helper;
using PodCricket.WP.Resources;

namespace PodCricket.WP.ViewModels
{
    public class PodDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string DisplayUrl { get; set; }
        public string Genres { get; set; }

        public Visibility AuthorVisibility { get; set; }
        public Visibility DisplayUrlVisibility { get; set; }
        public Visibility GenresVisibility { get; set; }


        public BitmapImage Image { get; set; }
        public string RegisterCaption { get; set; }
        public bool Subscribed { get; set; }
        public ObservableCollection<StreamModel> StreamList { get; set; }

        public PodDetailModel()
        {
            StreamList = new ObservableCollection<StreamModel>();
        }

        public PodDetailModel GetFrom(Pod pod)
        {
            if (pod == null) return null;

            this.Id = pod.Id == default(Guid) ? Guid.NewGuid() : pod.Id;
            this.Name = pod.Name;

            if (string.IsNullOrEmpty(pod.Author))
                this.AuthorVisibility = Visibility.Collapsed;
            else
                this.Author = AppResources.AuthorTitle + ": " + pod.Author;

            if (string.IsNullOrEmpty(pod.DisplayUrl))
                this.DisplayUrlVisibility = Visibility.Collapsed;
            else
                this.DisplayUrl = AppResources.UrlTitle + ": " + pod.DisplayUrl;

            if (string.IsNullOrEmpty(pod.Genres))
                this.GenresVisibility = Visibility.Collapsed;
            else
                this.Genres = AppResources.GenresTitle + ": " + pod.Genres;

            Uri imageUri;
            if (Uri.TryCreate(pod.ImageUrl, UriKind.Absolute, out imageUri))
                this.Image = new BitmapImage(imageUri);
            else
                this.Image = new BitmapImage(new Uri(@"Resources/default-pod.png", UriKind.RelativeOrAbsolute));

            this.RegisterCaption = pod.Subscribed ? AppResources.UnsubscribeTitle : AppResources.SubscribeTitle;
            this.Subscribed = pod.Subscribed;
            if (pod.StreamList != null && pod.StreamList.Count > 0)
                pod.StreamList.ForEach(s => this.StreamList.Add(new StreamModel().GetFrom(s)));

            return this;
        }
    }

    public class StreamModel
    {
        //public bool Downloaded { get; set; }
        public DownloadState DownloadState { get; set; }
        public DateTime DownloadDate { get; set; }
        public bool Listened { get; set; }
        public DateTime ListenDate { get; set; }
        public string LocalDownloadPath { get; set; }

        public Guid PodId { get; set; }
        public string Id { get; set; }
        public string Authors { get; set; }
        public string PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set;}
        public Uri DownloadUri { get; set; }

        public Visibility  DownloadVisibility { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }

        public StreamModel GetFrom(PodCricket.ApplicationServices.Stream stream)
        {
            this.PodId = stream.PodId;
            //this.Downloaded = stream.Downloaded;
            this.DownloadState = stream.DownloadState;
            this.DownloadDate = stream.FinishedDownloadDate;
            this.Listened = stream.Listened;
            this.ListenDate = stream.ListenDate;
            this.LocalDownloadPath = stream.LocalDownloadPath;

            this.Id = stream.Id ?? string.Empty;
            this.Authors = stream.Authors ?? AppResources.AuthorTitle + ": " + stream.Authors;
            this.PublishDate = stream.PublishDate ?? AppResources.PublishedTitle + ": " + stream.PublishDate;
            this.Title = stream.Title ?? stream.Title;
            this.Summary = stream.Summary ?? stream.Summary;
            this.DownloadUri = stream.DownloadUri ?? stream.DownloadUri;

            this.DownloadVisibility = (stream.DownloadState == ApplicationServices.DownloadState.Downloaded 
                                    || stream.DownloadState == ApplicationServices.DownloadState.Downloading)
                                        ? Visibility.Collapsed : Visibility.Visible;

            switch (this.DownloadState)
            { 
                case ApplicationServices.DownloadState.None:
                    this.BackgroundColor = new SolidColorBrush(Colors.Black);
                    break;
                case ApplicationServices.DownloadState.Downloading:
                    this.BackgroundColor = new SolidColorBrush(Colors.Gray);
                    break;
                case ApplicationServices.DownloadState.Downloaded:
                    this.BackgroundColor = new SolidColorBrush(Colors.Green);
                    break;
                default:
                    this.BackgroundColor = new SolidColorBrush(Colors.Black);
                    break;
            }

            return this;
        }

        public Uri GetLocalPlayUri()
        {
            var podcastPath = StreamHelper.GetPodcastPath(DownloadUri);
            if (string.IsNullOrEmpty(podcastPath)) return null;

            return new Uri(podcastPath, UriKind.RelativeOrAbsolute);
        }

        public System.IO.Stream GetLocalPlayStream()
        {
            if (DownloadState != ApplicationServices.DownloadState.Downloaded)
                return null;

            var podcastPath = StreamHelper.GetPodcastPath(DownloadUri);
            if (string.IsNullOrEmpty(podcastPath)) return null;

            if (string.IsNullOrEmpty(podcastPath)) return null;

            return StorageHelper.GetFileStream(podcastPath);
        }
    }
}
