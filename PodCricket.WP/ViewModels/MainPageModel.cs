using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PodCricket.ApplicationServices;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using PodCricket.Utilities.Toolkit;
using PodCricket.WP.Resources;

namespace PodCricket.WP.ViewModels
{
    public class MainPageModel
    {
        public ObservableCollection<PodModel> SearchList { get; set; }
        public ObservableCollection<PodModel> PodList { get; set; }
        public ObservableCollection<DownloadMonitor> TransferMonitorList { get; set; }
        public ObservableCollection<StreamModel> PlayingList { get; set; }

        public StreamModel SelectedPlayingStream { get; set; }

        public MainPageModel()
        {
            SearchList = new ObservableCollection<PodModel>();
            PodList = new ObservableCollection<PodModel>();
            TransferMonitorList = new ObservableCollection<DownloadMonitor>();
            PlayingList = new ObservableCollection<StreamModel>();
        }

        public void AddSearch(PodModel pod)
        {
            AddPodToList(pod, SearchList);
        }

        public void ClearSearch()
        {
            ClearList(SearchList);
        }

        public void AddPod(PodModel pod)
        {
            AddPodToList(pod, PodList);
        }

        public void ClearPod()
        {
            ClearList(PodList);
        }

        #region Private

        private void AddPodToList(PodModel pod, ObservableCollection<PodModel> list)
        {
            if (pod != null) 
                list.Add(pod);
        }

        private void ClearList(ObservableCollection<PodModel> list)
        {
            list.Clear();
        }

        #endregion

        public PodModel GetFromSearchList(Guid id)
        {
            return SearchList.Where(x => x.Id == id).FirstOrDefault();
        }
    }

    public class PodModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string ImageUrl { get; set; }
        public string DisplayUrl { get; set; }
        public string Genres { get; set; }
        public string FeedUrl { get; set; }

        public string Details { get; set; }
        public BitmapImage Image { get; set; }
        public bool Subscribed { get; set; }
        public System.Windows.Visibility RegisterVisibility { get; set; }
        public string SubscribeCaption { get; set; }

        public PodModel GetFrom(Pod pod)
        {
            this.Id = pod.Id;
            this.Name = pod.Name;
            this.Author = pod.Author;
            this.FeedUrl = pod.FeedUrl;
            this.ImageUrl = pod.ImageUrl;
            this.DisplayUrl = pod.DisplayUrl;
            this.Genres = pod.Genres;

            Uri imageUri;
            if (Uri.TryCreate(pod.ImageUrl, UriKind.Absolute, out imageUri))
                this.Image = new BitmapImage(imageUri);
            else 
                this.Image = new BitmapImage(new Uri(@"Resources/default-pod.png", UriKind.RelativeOrAbsolute));

            this.Details = CreatePodDetails();
            this.Subscribed = pod.Subscribed;
            this.RegisterVisibility = pod.Subscribed ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            this.SubscribeCaption = pod.Subscribed ? AppResources.UnsubscribeTitle : AppResources.SubscribeTitle;

            return this;
        }

        private string CreatePodDetails()
        {
            var details = new StringBuilder();
            if (!string.IsNullOrEmpty(DisplayUrl)) details.AppendLine(AppResources.UrlTitle + ": " + DisplayUrl);
            if (!string.IsNullOrEmpty(Author)) details.AppendLine(AppResources.AuthorTitle + ": " + Author);
            if (!string.IsNullOrEmpty(Genres)) details.AppendLine(AppResources.GenresTitle + ": " + Genres);

            return details.ToString();
        }

        public Pod ConvertToPod()
        {
            var pod = new Pod() { 
                Id = this.Id,
                Name = this.Name,
                FeedUrl = this.FeedUrl,
                Author = this.Author,
                ImageUrl = this.ImageUrl,
                DisplayUrl = this.DisplayUrl,
                Genres = this.Genres
            };

            return pod;
        }
    }
}
