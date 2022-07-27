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