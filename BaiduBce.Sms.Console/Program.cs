using BaiduBce.Sms;

Console.WriteLine("Hello, World!");
var sentResult = await new SmsClient().SendSms(new HttpClient(), new SmsPayload
{
    Mobile = "15821726189",
    Template = "sms-tmpl-OYNzlX33000",
    SignatureId = "sms-sign-DLwztD51234",
    ContentVar = new Dictionary<string, string>
    {
        {"code", "123456"}
    }
}, Guid.NewGuid().ToString());

Console.WriteLine($"SMS Sent result: {sentResult}");
Console.ReadLine();