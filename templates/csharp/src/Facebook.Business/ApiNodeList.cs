using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    public sealed class ApiNodeList<T> where T : ApiNode
    {
        [JsonProperty("data")]
        public List<T>? Data { get; set; }

        [JsonProperty("paging")]
        public ApiPaging? Paging { get; set; }

        [JsonProperty("summary")]
        public JToken? Summary { get; set; }
    }
}
