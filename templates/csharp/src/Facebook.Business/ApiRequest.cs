using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Facebook.Business
{
    public abstract class ApiRequest<T>
        where T : class
    {
        public string? NodeId { get; }
        protected ApiContext Context { get; }
        protected abstract string Endpoint { get; }
        protected abstract string[] FieldNames { get; }
        protected abstract string Method { get; }
        protected abstract string[] ParamNames { get; }
        protected IDictionary<string, object> RequestParams { get; } = new Dictionary<string, object>();
        protected List<string> ReturnFields { get; } = new List<string>();
        protected bool UseVideoEndpoint { get; set; }

        protected ApiRequest(ApiContext context)
        {
            Context = context;
        }

        protected ApiRequest(ApiContext context, string? nodeId)
        {
            Context = context;
            NodeId = nodeId;
        }

        public async Task<T> ExecuteAsync()
        {
            var @params = new Dictionary<string, object>(RequestParams);
            if (ReturnFields.Count > 0)
            {
                @params.Add("fields", string.Join(",", ReturnFields));
            }
            @params["access_token"] = Context.AccessToken;
            if (Context.AppSecretProof != null)
            {
                @params["appsecret_proof"] = Context.AppSecretProof;
            }

            string? queryString = null;
            HttpContent? content = null;
            HttpMethod method;
            switch (Method)
            {
                case "GET":
                case "DELETE":
                    method = Method == "GET" ? HttpMethod.Get : HttpMethod.Delete;
                    queryString = RequestUtils.ToQueryString(@params);
                    break;

                case "POST":
                    method = HttpMethod.Post;
                    content = RequestUtils.CreateContent(@params);
                    break;

                default:
                    throw new ArgumentException($"Invalid method name '{Method}'.", nameof(Method));
            }

            var request = new HttpRequestMessage(method, NodeId + Endpoint + queryString);
            if (content != null)
            {
                request.Content = content;
            }
            var response = await Context.BackChannel.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var jToken = await JsonUtils.GetJToken(response).ConfigureAwait(false);
                // TODO: Produce a better exception
                throw new ApiRequestException(jToken.ToString());
            }

            return await JsonUtils.GetJson<T>(response).ConfigureAwait(false);
        }

        public void RequestAllFields()
        {
            ReturnFields.AddRange(FieldNames);
        }

        public void RequestField(string name)
        {
            ReturnFields.Add(name);
        }

        public void RequestFields(IEnumerable<string> names)
        {
            ReturnFields.AddRange(names);
        }

        public void SetParam(string name, object value)
        {
            RequestParams[name] = value;
        }

        public void SetParams(IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var kvp in values)
            {
                RequestParams[kvp.Key] = kvp.Value;
            }
        }
    }
}
