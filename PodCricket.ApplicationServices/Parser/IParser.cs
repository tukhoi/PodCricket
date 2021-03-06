﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices.Parser
{
    public interface IParser
    {
        Task<PodList> GetFeed(string keyword);
        ParserData Data { get; set; }
    }
}
