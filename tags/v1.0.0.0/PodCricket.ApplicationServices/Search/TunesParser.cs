using Newtonsoft.Json;
using PodCricket.ApplicationServices.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PodCricket.Utilities.Extensions;

namespace PodCricket.ApplicationServices.Search
{
    public class TunesParser : BaseParser
    {
        private const string ITunesSearchUrl = "https://itunes.apple.com/search?term={0}&entity=podcast";

        public override string BaseUrl
        {
            get
            {
                return ITunesSearchUrl;
            }
        }

        protected internal override PodList ParseFeed(string data)
        {
            var itunesData = JsonConvert.DeserializeObject<TuneData>(data.Replace("\n", string.Empty), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            if (itunesData != null && itunesData.results != null) 
            {
                var result = new PodList();
                itunesData.results.ForEach(r => 
                    result.Add(new Pod { 
                        Name = r.trackName,
                        Author = r.artistName,
                        ImageUrl = r.artworkUrl100,
                        DisplayUrl = r.viewURL,
                        FeedUrl = r.feedUrl,
                        Genres = r.genres[0]
                }));

                return result;
            }

            return null;
        }
    }
}
