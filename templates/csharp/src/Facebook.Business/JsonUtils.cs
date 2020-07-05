using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Facebook.Business
{
    internal static class JsonUtils
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new MyContractResolver(Settings)
        });

        public static string SerializeObject(object value)
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var jsonWriter = new JsonTextWriter(sw);
            Serializer.Serialize(jsonWriter, value);
            jsonWriter.Flush();

            return sb.ToString();
        }

        public static async Task<T> GetJson<T>(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return Serializer.Deserialize<T>(jsonReader)!;
        }

        public static async Task<JToken> GetJToken(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            return await JToken.LoadAsync(jsonReader).ConfigureAwait(false);
        }

        private sealed class MyContractResolver : IContractResolver
        {
            private readonly IContractResolver _innerContractResolver;
            private readonly JsonSerializer _serializer;

            public MyContractResolver(JsonSerializerSettings settings)
            {
                _innerContractResolver = settings.ContractResolver!;
                _serializer = JsonSerializer.CreateDefault(settings);
            }

            public JsonContract ResolveContract(Type type)
            {
                var contract = _innerContractResolver.ResolveContract(type);
                if (typeof(ApiNode).IsAssignableFrom(type))
                {
                    var jsonObjectContract = (JsonObjectContract)contract;
                    if (jsonObjectContract.ExtensionDataSetter != null)
                    {
                        var oldSetter = jsonObjectContract.ExtensionDataSetter;
                        jsonObjectContract.ExtensionDataSetter = (o, key, value) =>
                        {
                            //var newKey = regex.Replace(key, replacement);
                            var newObject = TransformExtensionData(value);
                            oldSetter(o, key, newObject);
                        };
                    }
                }
                return contract;
            }

            private object? TransformExtensionData(object? value)
            {
                if (value is JObject jObject && jObject.TryGetValue("data", out var data) && data is JArray
                    && (jObject.ContainsKey("paging") || jObject.ContainsKey("summary")))
                {
                    var o = jObject.ToObject<Edge>();
                    return JObject.FromObject(o!, _serializer);
                }

                return value;
            }
        }

        private sealed class Edge
        {
            [JsonProperty("data")]
            public List<JToken>? Data { get; set; }

            [JsonProperty("paging")]
            public ApiPaging? Paging { get; set; }

            [JsonProperty("summary")]
            public JToken? Summary { get; set; }

            [JsonProperty("nextPageCursor")]
            public string? NextPageCursor
            {
                get
                {
                    if (!string.IsNullOrEmpty(Paging?.Next))
                    {
                        return Paging.Cursors?.After;
                    }
                    return null;
                }
            }

            public bool ShouldSerializeData()
            {
                return Data?.Count > 0;
            }

            public bool ShouldSerializePaging()
            {
                return false;
            }
        }
    }
}
