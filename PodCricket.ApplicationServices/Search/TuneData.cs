using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PodCricket.ApplicationServices.Search
{
    [DataContract]
    internal class TuneData
    {
        [DataMember]
        public string resultCount { get; set; }

        [DataMember]
        public IList<Result> results { get; set; }
    }

    [DataContract]
    internal class Result
    {
        [DataMember]
        public string trackName { get; set; }

        [DataMember]
        public string artistName { get; set; }

        [DataMember]
        public string artworkUrl100 { get; set; }

        [DataMember]
        public string viewURL { get; set; }

        [DataMember]
        public string feedUrl { get; set; }

        [DataMember]
        public IList<string> genres { get; set; }
    }
}
