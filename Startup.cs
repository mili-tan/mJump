using System;
using System.IO;
using System.Web;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace mJump
{
    public class Startup
    {
        public static Guid UID = Guid.NewGuid();
        private static LiteDatabase MyLiteDB = new("mjump.db");
        private static ILiteCollection<JumpEntity> MyCollection = MyLiteDB.GetCollection<JumpEntity>("MJUMP");

        public void ConfigureServices(IServiceCollection services)
        {
            MyCollection.EnsureIndex(x => x.Name, true);
            File.WriteAllText("token.txt", UID.ToString());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseRouting().UseEndpoints(endpoints =>
            {
                endpoints.Map("/", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("Welcome to mJump");
                });
                endpoints.Map($"/{UID}/add", async context =>
                {
                    var query = context.Request.Query;
                    if (query.TryGetValue("url", out var url) && query.TryGetValue("name", out var name))
                    {
                        if (MyCollection.Exists(x => x.Name == name))
                            await context.Response.WriteAsync("Already Exists");
                        else
                        {
                            MyCollection.Insert(new JumpEntity
                            {
                                StatusCode = query.TryGetValue("code", out var code) ? int.Parse(code) : 302,
                                RedirectUrl = HttpUtility.UrlDecode(url.ToString()),
                                Name = name.ToString()
                            });
                            await context.Response.WriteAsync("OK");
                        }
                    }
                    else
                    {
                        await context.Response.WriteAsync("Invalid");
                    }
                });
                endpoints.Map("/{Name}", async context =>
                {
                    var name = context.GetRouteValue("Name").ToString();
                    if (MyCollection.Exists(x=> x.Name == name))
                    {
                        var entity = MyCollection.FindOne(x => x.Name == name);
                        context.Response.StatusCode = entity.StatusCode;
                        context.Response.Redirect(entity.RedirectUrl);
                        await context.Response.WriteAsync("Move to " + entity.RedirectUrl);
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Not found");
                    }
                });
            });
        }

        public class JumpEntity
        {
            public string Name { get; set; }
            public string RedirectUrl { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
