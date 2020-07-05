using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;

namespace Facebook.Business
{
    internal static class RequestUtils
    {
        public static string ToQueryString(IEnumerable<KeyValuePair<string, object>> values)
        {
            var builder = new StringBuilder();
            foreach (var kvp in values)
            {
                if (builder.Length == 0)
                {
                    builder.Append("?");
                }
                else
                {
                    builder.Append('&');
                }

                builder.Append(UrlEncoder.Default.Encode(kvp.Key))
                    .Append('=');
                builder.Append(UrlEncoder.Default.Encode(ParamToString(kvp.Value)));
            }

            if (builder.Length == 0)
            {
                return string.Empty;
            }

            return builder.ToString();
        }

        public static HttpContent CreateContent(IEnumerable<KeyValuePair<string, object>> @params)
        {
            var files = new Dictionary<string, Stream>();
            var values = new Dictionary<string, string>();
            foreach (var (key, value) in @params)
            {
                if (value is Stream s)
                {
                    files.Add(key, s);
                }
                else
                {
                    values.Add(key, ParamToString(value));
                }
            }

            var formEncoded = new FormUrlEncodedContent(values);
            if (files.Count > 0)
            {
                var multiPart = new MultipartFormDataContent
                {
                    formEncoded
                };

                foreach (var (key, stream) in files)
                {
                    // TODO: Include file name?
                    multiPart.Add(new StreamContent(stream), key);
                }
                return multiPart;
            }

            return formEncoded;
        }

        public static string ParamToString(object value)
        {
            if (value is null)
            {
                return "null";
            }
            if (value is string s)
            {
                return s;
            }

            if (value is bool b)
            {
                return b ? "true" : "false";
            }

            if (value is IEnumerable<string> values)
            {
                return string.Join(",", values);
            }

            return JsonUtils.SerializeObject(value);
        }
    }
}
