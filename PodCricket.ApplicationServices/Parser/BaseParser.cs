using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PodCricket.Utilities.Extensions;
using PodCricket.Utilities.Tasks;


namespace PodCricket.ApplicationServices.Parser
{
    public abstract class BaseParser : IParser
    {
        #region Properties

        public ParserData Data { get; set; }

        public abstract string BaseUrl
        {
            get;
        }

        public virtual string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        #endregion

        #region Public methods

        public virtual async Task<PodList> GetFeed(string keyword)
        {
            PodList feedResult = null;
            var feedUrl = "";
            try
            {
                feedUrl = GetFeedUrl(keyword);
                var responseData = await GetRawResult(feedUrl);
                if (!string.IsNullOrEmpty(responseData.Trim()))
                    feedResult = ParseFeed(responseData);
            }
            catch (Exception ex)
            {
                //Log Exception
            }
            
            return feedResult;
        }

        public virtual async Task<string> GetRawResult(string requestUri)
        {
            //var client = new WebClient();
            //return await client.DownloadStringTaskAsync(new Uri(requestUri));

            return await WebTasks.GetRawResult(requestUri);
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internals methods

        internal virtual string GetFeedUrl(string keyword)
        {
            return string.Format(BaseUrl, keyword);
        }

        protected internal abstract PodList ParseFeed(string data);

        #endregion
    }
}
