using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Facebook.Business
{
    public sealed class ApiContext
    {
        private string? _appSecretProof;

        public string AccessToken { get; }

        public string? AppSecret { get; }

        public string? AppId { get; }

        public HttpClient BackChannel { get; }

        public bool SendAppSecretProof { get; set; }

        public string? AppSecretProof
        {
            get
            {
                if (SendAppSecretProof && AppSecret != null && _appSecretProof is null)
                {
                    _appSecretProof = GenerateAppSecretProof(AppSecret);
                }

                return _appSecretProof;
            }
        }

        public ApiContext(string accessToken, string appSecret, string appId, HttpClient backChannel)
        {
            AccessToken = accessToken;
            AppSecret = appSecret;
            AppId = appId;
            BackChannel = backChannel;
        }

        private string GenerateAppSecretProof(string appSecret)
        {
            using var algorithm = new HMACSHA256(Encoding.ASCII.GetBytes(appSecret));
            var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(AccessToken));
            var builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }
    }
}
