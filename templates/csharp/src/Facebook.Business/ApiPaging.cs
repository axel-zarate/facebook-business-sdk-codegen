using System;
using Newtonsoft.Json;

namespace Facebook.Business
{
    public sealed class ApiPaging
    {
        [JsonProperty("cursors")]
        public ApiCursors? Cursors { get; set; }

        [JsonProperty("previous")]
        public string? Previous { get; set; }

        [JsonProperty("next")]
        public string? Next { get; set; }
    }
}
