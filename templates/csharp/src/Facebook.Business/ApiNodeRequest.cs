using System;
using System.Collections.Generic;

namespace Facebook.Business
{
    public class ApiNodeRequest
    {
        public DateTime? Since { get; set; }

        public DateTime? Until { get; set; }

        public List<string> Fields { get; } = new List<string>();

        public IDictionary<string, object> Params { get; } = new Dictionary<string, object>();

        public string? After { get; set; }

        public string? Before { get; set; }

        public uint? Limit { get; set; }
    }
}
