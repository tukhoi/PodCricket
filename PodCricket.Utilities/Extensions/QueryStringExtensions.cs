using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Extensions
{
    public static class QueryStringExtensions
    {
        public static string GetQueryString(this IDictionary<string,string> queryString, string key) 
        {
            if (queryString.Keys.Contains(key))
                return queryString[key];
            return string.Empty;
        }
    }
}
