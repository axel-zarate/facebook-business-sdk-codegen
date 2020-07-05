using System;

namespace Facebook.Business
{
    [Serializable]
    public class ApiRequestException : Exception
    {
        public ApiRequestException()
        {
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
