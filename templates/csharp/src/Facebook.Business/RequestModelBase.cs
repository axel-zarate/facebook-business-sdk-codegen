using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    public abstract class RequestModelBase
    {
        [JsonExtensionData]
        private IDictionary<string, object?> CustomParams { get; } = new Dictionary<string, object?>();

        internal IDictionary<string, object> ToParams()
        {
            var jObject = JObject.FromObject(this, JsonUtils.Serializer);
            return jObject.Properties().Select(prop => KeyValuePair.Create(prop.Name, GetValue(prop.Value)))
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);
        }

        public RequestModelBase SetParam(string name, object value)
        {
            CustomParams[name] = value;
            return this;
        }

        private static object? GetValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:
                    return token;

                case JTokenType.Integer:
                    return token.Value<int>();

                case JTokenType.Float:
                    return token.Value<double>();

                case JTokenType.String:
                    return token.Value<string>();

                case JTokenType.Boolean:
                    return token.Value<bool>();

                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.Date:
                    return token.Value<DateTime>();

                case JTokenType.Bytes:
                    return token;

                case JTokenType.Guid:
                    return token.Value<Guid>();

                case JTokenType.Uri:
                    return token.Value<Uri>();

                case JTokenType.TimeSpan:
                    return token.Value<TimeSpan>();

                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Raw:
                    throw new NotSupportedException();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }

    public static class RequestModelBaseExtensions
    {
        public static RequestModelBase SetParams(this RequestModelBase model, IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var (key, value) in values)
            {
                model.SetParam(key, value);
            }

            return model;
        }

        public static RequestModelBase SetParamIfNotEmpty(this RequestModelBase model, string name, object? value)
        {
            if (value != null && !string.Empty.Equals(value))
            {
                model.SetParam(name, value);
            }

            return model;
        }
    }
}
