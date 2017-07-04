using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchGetAllIds.Request
{
    public class Scroll
    {
        public class RootObject
        {
            public string scroll { get; set; }
            public string scroll_id { get; set; }
        }
    }
}
