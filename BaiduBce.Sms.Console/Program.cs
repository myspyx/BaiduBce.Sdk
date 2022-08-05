using BaiduBce.Sms;
using BaiduBce.Sms.Console;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var app = HostBuilder.CreateHostBuilder(args).Build();
var smsClient = app.Services.GetService<ISmsClient>();
var phoneNo = Environment.GetEnvironmentVariable("PHONE_NO");

if (smsClient != null)
{
    // verification cade
    var sentResult = await smsClient.Send(new SmsPayload
    {
        Mobile = phoneNo,
        Template = "sms-tmpl-OYNzlX33000",
        SignatureId = "sms-sign-DLwztD51234",
        ContentVar = new Dictionary<string, string>
        {
            {"code", "787878"}
        }
    }, Guid.NewGuid().ToString());
    Console.WriteLine($"SMS Sent result: {sentResult}");

    // first time reminder msg
    var sentResult2 = await smsClient.Send(new SmsPayload
    {
        Mobile = phoneNo,
        Template = "sms-tmpl-ZePaUM26457",
        SignatureId = "sms-sign-DLwztD51234",
        ContentVar = new Dictionary<string, string>
        {
            {"redirect_link", "https://app.mymo.cn/"},
            {"web_link", "https://patient.zbmymobilitysolutions.com"}
        }
    }, Guid.NewGuid().ToString());
    Console.WriteLine($"SMS Sent result: {sentResult2}");

    // recurring reminder msg
    var sentResult3 = await smsClient.Send(new SmsPayload
    {
        Mobile = phoneNo,
        Template = "sms-tmpl-KowucB32953",
        SignatureId = "sms-sign-DLwztD51234",
        ContentVar = new Dictionary<string, string>
        {
            {"redirect_link", "https://app.mymo.cn/"},
            {"help_phone", "400-400-FAKE"}
        }
    }, Guid.NewGuid().ToString());
    Console.WriteLine($"SMS Sent result: {sentResult3}");

}

Console.ReadLine();