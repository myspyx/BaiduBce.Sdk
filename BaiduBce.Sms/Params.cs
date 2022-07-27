using System.Collections.Generic;

namespace BaiduBce.Sms
{
    public class SmsPayload
    {
        public string Mobile { get; set; }
        public string Template { get; set; }
        public string SignatureId { get; set; }
        public Dictionary<string, string> ContentVar { get; set; }
    }

    public class SmsResponse
    {
        public string RequestId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public List<SendMessageItem> Data { get; set; }
    }

    public class SendMessageItem
    {
        public string Mobile { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string MessageId { get; set; }
    }
}