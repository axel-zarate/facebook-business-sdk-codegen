using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    internal sealed class ApiNodeListConverter<T> : JsonConverter<ApiNodeList<T>>
        where T : ApiNode
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override ApiNodeList<T> ReadJson(JsonReader reader, Type objectType, ApiNodeList<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            if (jToken.Type == JTokenType.Object)
            {
                return FromObject((JObject)jToken, existingValue, serializer);
            }
            if (jToken.Type == JTokenType.Array)
            {
                var jArray = (JArray)jToken;
                return FromList(jArray.ToObject<List<T>>(serializer), existingValue);
            }

            throw new MalformedResponseException("Invalid response string: " + jToken.ToString());
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] ApiNodeList<T> value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private static ApiNodeList<T> FromObject(JObject jObject, ApiNodeList<T>? existingValue, JsonSerializer serializer)
        {
            if (jObject.TryGetValue("data", out var d))
            {
                if (d.Type == JTokenType.Array)
                {
                    // Most common case; we deserialize each property one by one to avoid re-entrance
                    var value = FromList(d.ToObject<List<T>>(serializer), existingValue);
                    if (jObject.TryGetValue("paging", out var p) && p is JObject paging)
                    {
                        value.Paging = paging.ToObject<ApiPaging>(serializer);
                    }
                    if (jObject.TryGetValue("summary", out var s))
                    {
                        value.Summary = s;
                    }
                    return value;
                }
                if (d.Type == JTokenType.Object)
                {
                    // Sometimes the API spec states the request returns a list when in fact it returns a single object
                    var obj = d.ToObject<T>();
                    return new ApiNodeList<T> { Data = new List<T> { obj! } };
                }
            }

            if (jObject.TryGetValue("images", out var i))
            {
                var imagesObj = (JObject)i;
                var list = jObject.PropertyValues()
                    .Cast<JObject>()
                    .Select(x => x.ToObject<T>(serializer)!)
                    .ToList();

                return FromList(list, existingValue);
            }

            var isIdIndexedArray = true;

            var data = new List<T>();
            foreach (var prop in jObject.Properties())
            {
                var key = prop.Name;
                if (prop.Value != null && prop.Value is JObject objectValue
                    && objectValue.TryGetValue("id", out var id) && id.ToString() == prop.Name)
                {
                    data.Add(objectValue.ToObject<T>(serializer)!);
                }
                else
                {
                    isIdIndexedArray = false;
                    break;
                }
            }

            if (!isIdIndexedArray)
            {
                // Treat it as a single object
                var item = jObject.ToObject<T>(serializer);
                data.Add(item!);
            }

            return FromList(data, existingValue);
            //throw new MalformedResponseException("Invalid response string: " + jObject.ToString());
        }

        private static ApiNodeList<T> FromList(List<T>? list, ApiNodeList<T>? existingValue)
        {
            var value = existingValue ?? new ApiNodeList<T>();
            value.Data = list;
            return value;
        }
    }

    [Serializable]
    public class MalformedResponseException : ApiRequestException
    {
        public MalformedResponseException()
        {
        }

        public MalformedResponseException(string message) : base(message)
        {
        }

        public MalformedResponseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MalformedResponseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public MalformedResponseException(JToken response) : base(response)
        {
        }
    }
}
