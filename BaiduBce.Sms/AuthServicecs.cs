using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BaiduBce.Sms
{
    public interface IAuthService
    {
        Task<T2> SendDateAsync<T1, T2>(
            HttpClient httpClient,
            HttpMethod httpMethod,
            Uri apiEndpoint,
            T1 payload = default);
    }

    // doc: https://cloud.baidu.com/doc/Reference/s/njwvz1yfu
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }

        public async Task<T2> SendDateAsync<T1, T2>(
            HttpClient httpClient,
            HttpMethod httpMethod,
            Uri apiEndpoint,
            T1 payload = default)
        {
            var (signDate, authorization) = SignDate<T1>((httpMethod.Method, apiEndpoint));

            var request = new HttpRequestMessage(httpMethod, apiEndpoint);
            request.Headers.TryAddWithoutValidation("x-bce-date", signDate);
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            if (payload != null)
            {
                request.Content = JsonContent.Create(payload);
            }
            else
            {
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            }

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T2>();
            }

            _logger.LogError("Call Return status code: {ResponseStatusCode} in {ApiEndpoint}", response.StatusCode,
                apiEndpoint);
            return default;
        }

        private (string signDate, string authorization) SignDate<T>((string httpMethod, Uri apiUri) requestInfo)
        {
            var accessKeyId = Environment.GetEnvironmentVariable("BCE_ACCESS_KEY_ID");
            var secretAccessKey = Environment.GetEnvironmentVariable("BCE_SECRET_ACCESS_KEY");

            if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey))
            {
                throw new ArgumentException("BCE_ACCESS_KEY_ID or BCE_SECRET_ACCESS_KEY is not set");
            }

            var now = DateTime.Now;
            const int expirationInSeconds = 1200; // 20 minutes
            var signDate = now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
            var authString = "bce-auth-v1/" + accessKeyId + "/" + signDate + "/" + expirationInSeconds;
            var signingKey = Hex(
                new HMACSHA256(Encoding.UTF8.GetBytes(secretAccessKey)).ComputeHash(
                    Encoding.UTF8.GetBytes(authString)));

            var canonicalRequestString = CanonicalRequest(requestInfo);

            var signature = Hex(new HMACSHA256(Encoding.UTF8.GetBytes(signingKey)).ComputeHash(
                Encoding.UTF8.GetBytes(canonicalRequestString)));
            var authorization = authString + "/host/" + signature;
            return (signDate, authorization);
        }

        private string Hex(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var b in data) sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        private string CanonicalRequest((string httpMethod, Uri apiUri) requestInfo)
        {
            var uri = requestInfo.apiUri;
            var canonicalReq = new StringBuilder();
            canonicalReq.Append(requestInfo.httpMethod).Append("\n")
                .Append(UriEncode(Uri.UnescapeDataString(uri.AbsolutePath)))
                .Append("\n");

            var parameters = QueryHelpers.ParseQuery(uri.Query);
            var parameterStrings = new List<string>();
            foreach (var entry in parameters) parameterStrings.Add(UriEncode(entry.Key) + '=' + UriEncode(entry.Value));

            parameterStrings.Sort();
            canonicalReq.Append(string.Join("&", parameterStrings.ToArray())).Append("\n");

            var host = uri.Host;
            if (!(uri.Scheme == "https" && uri.Port == 443) && !(uri.Scheme == "http" && uri.Port == 80))
                host += ":" + uri.Port;

            canonicalReq.Append("host:" + UriEncode(host));
            return canonicalReq.ToString();
        }

        private string UriEncode(string input, bool encodeSlash = false)
        {
            var builder = new StringBuilder();
            foreach (var b in Encoding.UTF8.GetBytes(input))
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_' ||
                    b == '-' || b == '~' || b == '.')
                {
                    builder.Append((char) b);
                }
                else if (b == '/')
                {
                    if (encodeSlash)
                        builder.Append("%2F");
                    else
                        builder.Append((char) b);
                }
                else
                {
                    builder.Append('%').Append(b.ToString("X2"));
                }

            return builder.ToString();
        }
    }
}