using System.Collections.Generic;

namespace ElasticSearchGetAllIds.Request
{
        public class MatchAll
        {
        }

        public class Query
        {
            public MatchAll match_all { get; set; }
        }

        public class RootObject
        {
            public int size { get; set; }
            public Query query { get; set; }
            public List<string> _source { get; set; }
            public string sort { get; set; }
        }
}
