using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Facebook.Business
{
    public abstract class ApiRequestBase<T>
        where T : class
    {
        public string? NodeId { get; }
        protected ApiContext Context { get; }
        protected abstract string Endpoint { get; }
        protected internal abstract ICollection<string> FieldNames { get; }
        protected abstract string Method { get; }
        protected abstract ICollection<string> ParamNames { get; }
        protected IDictionary<string, object> RequestParams { get; } = new Dictionary<string, object>();
        protected ICollection<string> ReturnFields { get; } = new HashSet<string>();
        protected bool UseVideoEndpoint { get; set; }

        protected ApiRequestBase(ApiContext context)
        {
            Context = context;
        }

        protected ApiRequestBase(ApiContext context, string? nodeId)
        {
            Context = context;
            NodeId = nodeId;
        }

        public Task<T> ExecuteAsync()
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

            return ExecuteInternal(method, NodeId + Endpoint + queryString, content);
        }

        public Task<T> ExecuteAsync(string url)
        {
            HttpMethod method;
            switch (Method)
            {
                case "GET":
                    method = HttpMethod.Get;
                    break;

                case "DELETE":
                    method = HttpMethod.Delete;
                    break;

                case "POST":
                    throw new InvalidOperationException();

                default:
                    throw new ArgumentException($"Invalid method name '{Method}'.", nameof(Method));
            }

            return ExecuteInternal(method, url, null);
        }

        public ApiRequestBase<T> RequestField(string name)
        {
            ReturnFields.Add(name);
            return this;
        }

        public ApiRequestBase<T> SetParam(string name, object value)
        {
            RequestParams[name] = value;
            return this;
        }

        protected void SetModelInternal(RequestModelBase model)
        {
            var @params = model.ToParams();
            foreach (var (key, value) in @params)
            {
                RequestParams[key] = value;
            }
        }

        private async Task<T> ExecuteInternal(HttpMethod method, string url, HttpContent? content)
        {
            var request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                request.Content = content;
            }
            var response = await Context.BackChannel.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var jToken = await JsonUtils.GetJToken(response).ConfigureAwait(false);
                ThrowError(jToken);
            }

            return await JsonUtils.GetJson<T>(response).ConfigureAwait(false);
        }

        private static void ThrowError(JToken response)
        {
            if (response is JObject jObject && jObject.TryGetValue("error", out var error)
                    && error is JObject errorObject)
            {
                throw new ApiRequestException(errorObject);
            }

            // TODO: Produce a better exception
            throw new ApiRequestException(response.ToString());
        }
    }

    public static class ApiRequestExtensions
    {
        public static ApiRequestBase<T> SetParams<T>(this ApiRequestBase<T> apiRequest, IEnumerable<KeyValuePair<string, object>> values)
            where T : class
        {
            foreach (var (key, value) in values)
            {
                apiRequest.SetParam(key, value);
            }

            return apiRequest;
        }

        public static ApiRequestBase<T> RequestFields<T>(this ApiRequestBase<T> apiRequest, params string[] names)
            where T : class
        {
            foreach (var name in names)
            {
                apiRequest.RequestField(name);
            }

            return apiRequest;
        }

        public static ApiRequestBase<T> RequestFields<T>(this ApiRequestBase<T> apiRequest, IEnumerable<string> names)
            where T : class
        {
            foreach (var name in names)
            {
                apiRequest.RequestField(name);
            }

            return apiRequest;
        }

        public static ApiRequestBase<T> RequestAllFields<T>(this ApiRequestBase<T> apiRequest)
            where T : class
        {
            return apiRequest.RequestFields(apiRequest.FieldNames);
        }
    }
}
