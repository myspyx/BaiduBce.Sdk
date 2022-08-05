using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BaiduBce.Sms
{
    public interface ISmsClient
    {
        /// <summary>
        /// Send sms message.
        /// </summary>
        /// <param name="smsPayload">The sms payload will be sent</param>
        /// <param name="clientToken">The idempotence parameter prevents the client from sending the same text message multiple times
        /// when the http response times out and retries.</param>
        /// <returns>Sms send result</returns>
        Task<bool> Send(SmsPayload smsPayload, string clientToken);
    }

    // doc: https://cloud.baidu.com/doc/SMS/s/lkijy5wvf
    public class SmsClient : ISmsClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<SmsClient> _logger;

        public SmsClient(HttpClient httpClient, ILogger<SmsClient> logger, IAuthService authService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authService = authService;
        }

        public async Task<bool> Send(SmsPayload smsPayload, string clientToken)
        {
            if (smsPayload == null
                || string.IsNullOrWhiteSpace(smsPayload.Mobile)
                || string.IsNullOrWhiteSpace(smsPayload.SignatureId)
                || string.IsNullOrWhiteSpace(smsPayload.Template))
            {
                throw new ArgumentNullException(nameof(smsPayload), "smsPayload is null or empty");
            }

            const string apiHost = "smsv3.bj.baidubce.com";
            const string apiEndpoint = "/api/v3/sendSms";

            var uri = $"https://{apiHost}{apiEndpoint}";
            var param = new Dictionary<string, string> {{"clientToken", clientToken}};
            var smsApiEndpoint = new Uri(QueryHelpers.AddQueryString(uri, param));

            var response = await _authService.SendDateAsync<SmsPayload, SmsResponse>(
                _httpClient,
                HttpMethod.Post,
                smsApiEndpoint,
                smsPayload);

            if (response != null && string.Equals(response.Code, "1000", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            _logger.LogError("Send sms failed, response: {Response}", JsonSerializer.Serialize(response));
            return false;
        }
    }
}