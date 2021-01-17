using System;
using System.Net;
using LiteDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace mJump
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new LiteDatabase(@"MURL.db");
            var col = db.GetCollection<MURL>("MURL");
            if (!col.Exists(x => x.Name == "test")) col.Insert(new MURL {Id = "test", Name = "test"});
            var i = col.FindOne(x => x.Name == "test");
            Console.WriteLine(i.Name);
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(AppDomain.CurrentDomain.SetupInformation.ApplicationBase)
                .ConfigureServices(services => services.AddRouting())
                .ConfigureKestrel(options =>
                {
                    options.Listen(new IPEndPoint(IPAddress.Loopback, 2025), listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        //if (true) listenOptions.UseHttps();
                    });
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        public class MURL
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
