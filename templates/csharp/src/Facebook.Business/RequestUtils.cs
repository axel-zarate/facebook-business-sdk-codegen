using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;

namespace Facebook.Business
{
    internal static class RequestUtils
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var formEncoded = new FormUrlEncodedContent(values);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
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

        public static string AddQueryString(string url, string name, object value)
        {
            var encoded = UrlEncoder.Default.Encode(name) + "=" + UrlEncoder.Default.Encode(ParamToString(value));

            if (url.IndexOf("?") > 0)
            {
                return url + "&" + encoded;
            }

            return url + "?" + encoded;
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

            if (value is DateTime d)
            {
                return ToUnixTime(d).ToString(CultureInfo.InvariantCulture);
            }

            if (value is IEnumerable<string> values)
            {
                return string.Join(",", values);
            }

            return JsonUtils.SerializeObject(value);
        }

        private static long ToUnixTime(DateTime dateTime)
        {
            return Convert.ToInt64((dateTime - Epoch).TotalSeconds);
        }
    }
}
