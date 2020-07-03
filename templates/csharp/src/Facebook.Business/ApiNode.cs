using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    public abstract class ApiNode
    {
        [JsonExtensionData]
        public IDictionary<string, JToken>? ExtensionData { get; set; }
    }
}
