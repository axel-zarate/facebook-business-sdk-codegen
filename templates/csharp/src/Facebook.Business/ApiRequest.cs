using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Facebook.Business
{
    public sealed class ApiRequest<T> : ApiRequestBase<T>
      where T : class
    {
        protected override string Method { get; }

        protected override string Endpoint { get; }

        protected internal override ICollection<string> FieldNames => Array.Empty<string>();

        protected override ICollection<string> ParamNames => Array.Empty<string>();

        public ApiRequest(ApiContext context, HttpMethod method, string url) : base(context)
        {
            Method = method.Method.ToUpper();
            Endpoint = url;
        }
    }
}
