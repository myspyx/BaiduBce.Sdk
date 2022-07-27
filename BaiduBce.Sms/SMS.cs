using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace BaiduBce.Sms
{
    // doc: https://cloud.baidu.com/doc/SMS/s/lkijy5wvf
    public class SmsClient
    {
        public async Task<bool> SendSms(HttpClient httpClient, SmsPayload smsPayload, string clientToken)
        {
            try
            {
                const string apiHost = "smsv3.bj.baidubce.com";
                const string apiEndpoint = "/api/v3/sendSms";

                var uri = $"https://{apiHost}{apiEndpoint}";
                var param = new Dictionary<string, string> {{"clientToken", clientToken}};
                var smsApiEndpoint = new Uri(QueryHelpers.AddQueryString(uri, param));

                var response = await AuthHelper.SendDateAsync<SmsPayload, SmsResponse>(
                    httpClient,
                    HttpMethod.Post,
                    smsApiEndpoint,
                    smsPayload);

                return response != null && string.Equals(response.Code, "1000", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}