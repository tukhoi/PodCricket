using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using PodCricket.Utilities.Extensions;
using System.IO;
using Microsoft.Phone.BackgroundTransfer;
using PodCricket.Utilities.Helpers;
using PodCricket.ApplicationServices.Helper;
using PodCricket.Utilities.AppLicense;

namespace PodCricket.ApplicationServices
{
    public class Stream
    {
        internal SyndicationItem _syndicationItem;

        public Guid PodId { get; set; }
        //public bool Downloaded { get; set; }
        public DownloadState DownloadState { get; set; }
        public DateTime StartDownloadDate { get; set; }
        public DateTime FinishedDownloadDate { get; set; }
        public bool Listened { get; set; }
        public DateTime ListenDate { get; set; }
        public string LocalDownloadPath { get; set; }
        public bool IsVideo { get; set; }

        //public BackgroundTransferRequest DownloadRequest { get; set; }

        public Stream(SyndicationItem syndicationItem, ref bool licenseRequired)
        {
            _syndicationItem = syndicationItem;
            LoadFromSyndicationItem(ref licenseRequired);
        }

        public void CheckDownloadedStreams()
        {
            if (DownloadUri == null)
                this.DownloadState = ApplicationServices.DownloadState.CannotDownload;

            if (this.DownloadState == ApplicationServices.DownloadState.Downloaded)
            {
                var podcastPath = StreamHelper.GetPodcastPath(DownloadUri);
                if (!StorageHelper.CheckLocalFileExist(podcastPath))
                    this.DownloadState = ApplicationServices.DownloadState.Failed;
            }
        }

        #region Syndication properties

        private void LoadFromSyndicationItem(ref bool licenseRequired)
        {
            UpdateFromSyndicationItem(_syndicationItem, ref licenseRequired);
        }

        public void UpdateFromSyndicationItem(SyndicationItem syndicationItem, ref bool licenseRequired)
        {
            try
            {

                if (syndicationItem == null) return;

                this.Id = syndicationItem.Id;

                var authors = new List<string>();
                syndicationItem.Authors.ForEach(a => authors.Add(a.Name));

                this.Authors = string.Join(",", authors.ToArray());

                this.PublishDate = syndicationItem == null ? string.Empty : syndicationItem.PublishDate.ToString();

                var title = syndicationItem.Title;
                if (title.Type.Equals("text"))
                    this.Title = title.Text;
                else this.Title = string.Empty;

                var summary = syndicationItem.Summary;
                if (summary != null && summary.Type.Equals("text"))
                    this.Summary = summary.Text;
                else
                    this.Summary = string.Empty;



                //if (LicenseHelper.Purchased(AppConfig.PRO_VERSION))
                //{
                //    downloadLink = syndicationItem.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.MediaType)
                //    && (AppConfig.STREAM_SUPPORTED_TYPES.Contains(l.MediaType) || l.MediaType.StartsWith("audio")
                //    || l.MediaType.StartsWith("video")));
                //}
                //else
                //{
                //    downloadLink = syndicationItem.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.MediaType)
                //    && (AppConfig.STREAM_SUPPORTED_TYPES.Contains(l.MediaType) || l.MediaType.StartsWith("audio")));

                //    if (downloadLink == null && syndicationItem.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.MediaType) &&
                //        l.MediaType.StartsWith("video")) != null)
                //        licenseRequired = true;
                //}

                SyndicationLink downloadLink = null;
                downloadLink = syndicationItem.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.MediaType)
                    && (l.MediaType.StartsWith("audio") || l.MediaType.StartsWith("video")));
                this.DownloadUri = downloadLink == null ? null : downloadLink.Uri;

                this.IsVideo = downloadLink != null
                    && syndicationItem.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.MediaType)
                        && l.MediaType.StartsWith("video")) != null ? true : false;

                licenseRequired = this.IsVideo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Id { get; set; }
        public string Authors { get; set; }
        public string PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public Uri DownloadUri { get; set; }

        #endregion
    }

    public class StreamList : List<Stream>
    {
        public StreamList GetFrom(IEnumerable<SyndicationItem> items, Pod pod, ref bool licenseRequired)
        {
            //items.ForEach(i => { 
            //    var persistentStream = pod.StreamList.FirstOrDefault(x => x.Id == i.Id);
            //    if (persistentStream == null)
            //    {
            //        persistentStream = new Stream(i, ref licenseRequired);
            //        persistentStream.PodId = pod.Id;
            //    }

            //    base.Add(persistentStream);
            //});

            foreach (var item in items)
            {
                var persistentStream = pod.StreamList.FirstOrDefault(x => x.Id == item.Id);
                if (persistentStream == null)
                {
                    persistentStream = new Stream(item, ref licenseRequired);
                    persistentStream.PodId = pod.Id;
                }
                else
                    persistentStream.UpdateFromSyndicationItem(item, ref licenseRequired);

                base.Add(persistentStream);
            }

            return this;
        }

        public StreamList GetFrom(IList<Stream> streams)
        {
            streams.ForEach(s => base.Add(s));
            return this;
        }
    }

    public enum DownloadState
    { 
        None,
        Failed,
        Downloading,
        Downloaded,
        CannotDownload
    }
}
