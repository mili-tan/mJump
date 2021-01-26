using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace mJump
{
    public class Startup
    {
        public static Guid UID = Guid.NewGuid();
        public static string BaseURL = File.Exists("url.txt") ? File.ReadAllText("url.txt").TrimEnd('/') : string.Empty;
        public static bool IsPublic = File.Exists("public.txt");
        private static bool isMongo = File.Exists("mongo.txt");

        public void ConfigureServices(IServiceCollection services)
        {
            if (isMongo) MongoDbRoutes.Init(File.ReadAllText("mongo.txt"));
            else LiteDbRoutes.Init();
            Console.WriteLine("IsMongo:" + isMongo);

            if (File.Exists("token.txt")) UID = Guid.Parse(File.ReadAllText("token.txt"));
            else File.WriteAllText("token.txt", UID.ToString());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseRouting().UseEndpoints(endpoints =>
            {
                endpoints.Map("/", async context =>
                {
                    if (IsPublic)
                    {
                        context.Response.Redirect(await File.ReadAllTextAsync("public.txt"));
                        await context.Response.WriteAsync("Mova to mJump Dash");
                    }
                    else
                    {
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync("Welcome to mJump");
                    }
                });
                endpoints.Map("/IsMongo", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("IsMongo:" + isMongo);
                });
            }).UseEndpoints(isMongo ? MongoDbRoutes.Route : LiteDbRoutes.Route);
        }
    }
}
