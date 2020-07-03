using System;
using System.Collections.Generic;

namespace Facebook.Business
{
    public abstract class ApiRequest<T>
        where T : class
    {
        protected List<string> ReturnFields { get; } = new List<string>();

        protected IDictionary<string, object> RequestParams { get; } = new Dictionary<string, object>();

        protected ApiContext Context { get; }

        protected bool UseVideoEndpoint { get; set; }

        public string? NodeId { get; }

        protected abstract string Method { get; }

        protected abstract string Endpoint { get; }

        protected abstract string[] FieldNames { get; }

        protected abstract string[] ParamNames { get; }

        protected ApiRequest(ApiContext context)
        {
            Context = context;
        }

        protected ApiRequest(ApiContext context, string? nodeId)
        {
            Context = context;
            NodeId = nodeId;
        }

        public void RequestField(string name)
        {
            ReturnFields.Add(name);
        }

        public void RequestFields(IEnumerable<string> names)
        {
            ReturnFields.AddRange(names);
        }

        public void RequestAllFields()
        {
            ReturnFields.AddRange(FieldNames);
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
