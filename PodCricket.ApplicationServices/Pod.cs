using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using PodCricket.Utilities.Extensions;

namespace PodCricket.ApplicationServices
{
    public class Pod
    {
        #region Management Properties

        public bool Subscribed { get; set; }
        public string SearchTerms { get; set; }

        #endregion

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string ImageUrl { get; set; }
        public string DisplayUrl { get; set; }
        public string FeedUrl { get; set; }
        public string Genres { get; set; }
        public bool IsVideo { get; set; }

        public StreamList StreamList { get; set; }

        public Pod()
        {
            StreamList = new StreamList();
        }

        public Pod CloneToSave(IList<string> streamsToSave)
        {
            this.StreamList.ForEach(s => s.CheckDownloadedStreams());

            var clonedPod = new Pod() { 
                Subscribed = this.Subscribed,
                SearchTerms = this.SearchTerms,
                Id = this.Id,
                Name = this.Name,
                Author = this.Author,
                ImageUrl = this.ImageUrl,
                DisplayUrl = this.DisplayUrl,
                FeedUrl = this.FeedUrl,
                Genres = this.Genres
            };

            var streams = this.StreamList.Where(s => ((s.DownloadState == DownloadState.Downloading ||
                                                s.DownloadState == DownloadState.Downloaded) && 
                                                streamsToSave.FirstOrDefault(streamId => s.Id.Equals(streamId)) == null))
                                                .ToList();

            clonedPod.StreamList = new StreamList().GetFrom(streams);
            return clonedPod;
        }
    }

    public class PodList : List<Pod>
    {
        public PodList()
        {

        }

        public PodList(List<Pod> pods)
        {
            if (pods == null || pods.Count == 0) return;

            pods.ForEach(p => this.Add(p));
        }
    }

    public class PodDictionary : Dictionary<Guid, Pod>
    {
        //public static bool Loaded = false;

        public PodDictionary()
        {
        }

        public void UpdateFrom(PodDictionary pods)
        {
            base.Clear();
            foreach (var pod in pods)
            {
                Add(pod.Key, pod.Value);
                //pod.Value.StreamList.ForEach(s => s.CheckDownloadedStreams());

                System.Threading.Tasks.Task.Factory.StartNew(() => {
                    var downloadedStreams = pod.Value.StreamList.Where(s => s.DownloadState == DownloadState.Downloaded).ToList();
                    downloadedStreams.ForEach(s => s.CheckDownloadedStreams());
                });
            }
        }

        public PodDictionary GetFrom(IList<Pod> pods)
        {
            base.Clear();
            pods.ForEach(p => Add(p.Id, p));

            return this;
        }

        public PodDictionary CloneToSave(IList<string> streamsToSave)
        {
            var podMap = new PodDictionary();
            this.Values.Where(p=>p.Subscribed).ToList().ForEach(p => {
                podMap.Add(p.Id, p.CloneToSave(streamsToSave));
            });

            return podMap;
        }
    }
}
