using System;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    [Serializable]
    public class ApiRequestException : Exception
    {
        public ErrorResponse? Response { get; }

        public int? Code => Response?.Code;

        public int? Subcode => Response?.Code;

        internal ApiRequestException()
        {
        }

        public ApiRequestException(JToken response) : base(response.ToString())
        {
            Response = response.ToObject<ErrorResponse>();
        }

        public ApiRequestException(string message) : base(message)
        {
        }

        public ApiRequestException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ApiRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
