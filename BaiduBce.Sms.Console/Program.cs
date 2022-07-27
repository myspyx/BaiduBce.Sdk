using BaiduBce.Sms;
using BaiduBce.Sms.Console;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var app = HostBuilder.CreateHostBuilder(args).Build();
var smsClient = app.Services.GetService<ISmsClient>();

if (smsClient != null)
{
    var sentResult = await smsClient.Send(new SmsPayload
    {
        Mobile = "15821726189",
        Template = "sms-tmpl-OYNzlX33000",
        SignatureId = "sms-sign-DLwztD51234",
        ContentVar = new Dictionary<string, string>
        {
            {"code", "787878"}
        }
    }, Guid.NewGuid().ToString());

    Console.WriteLine($"SMS Sent result: {sentResult}");
}

Console.ReadLine();