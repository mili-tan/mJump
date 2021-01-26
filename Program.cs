using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace mJump
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Startup.UID);
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(AppDomain.CurrentDomain.SetupInformation.ApplicationBase)
                .ConfigureServices(services => services.AddRouting())
                .ConfigureKestrel(options =>
                {
                    options.Listen(new IPEndPoint(IPAddress.Any, 2025), listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        //if (true) listenOptions.UseHttps();
                    });
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
