using System;
using Newtonsoft.Json;

namespace Facebook.Business
{
    public sealed class ApiCursors
    {
        [JsonProperty("after")]
        public string? After { get; set; }

        [JsonProperty("before")]
        public string? Before { get; set; }
    }
}
