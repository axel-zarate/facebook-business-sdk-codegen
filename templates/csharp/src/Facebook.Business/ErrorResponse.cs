using Newtonsoft.Json;

namespace Facebook.Business
{
    /// <summary>
    /// https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling
    /// </summary>
    public sealed class ErrorResponse
    {
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("code")]
        public int? Code { get; set; }

        [JsonProperty("error_subcode")]
        public int? Subcode { get; set; }

        [JsonProperty("error_user_title")]
        public string? ErrorUserTitle { get; set; }

        [JsonProperty("error_user_msg")]
        public string? ErrorUserMessage { get; set; }

        [JsonProperty("fbtrace_id")]
        public string? TraceId { get; set; }
    }
}
