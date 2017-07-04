using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ElasticSearchGetAllIds.Response
{
    public class ScrollBase
    {
        public class Shards
        {
            public int total { get; set; }
            public int successful { get; set; }
            public int failed { get; set; }
        }

        public class Source
        {
            public string UniversalId { get; set; }
        }

        public class Hit
        {
            public string _index { get; set; }
            public string _type { get; set; }
            public string _id { get; set; }
            public object _score { get; set; }
            public Source _source { get; set; }
            public List<Int64> sort { get; set; }
        }

        public class Hits
        {
            public int total { get; set; }
            public object max_score { get; set; }
            public List<Hit> hits { get; set; }
        }

        public class RootObject
        {
            public string _scroll_id { get; set; }
            public int took { get; set; }
            public bool timed_out { get; set; }
            public Shards _shards { get; set; }
            public Hits hits { get; set; }
        }
    }
}
