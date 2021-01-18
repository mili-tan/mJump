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
        private static LiteDatabase MyLiteDB = new("mjump.db");
        private static ILiteCollection<JumpEntity> MyCollection = MyLiteDB.GetCollection<JumpEntity>("MJUMP");

        public void ConfigureServices(IServiceCollection services)
        {

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
                endpoints.Map("/{Name}", async context =>
                {
                    var name = context.GetRouteValue("Name").ToString();
                    if (MyCollection.Exists(x=> x.Name == name))
                    {
                        var redirectUrl = MyCollection.FindOne(x => x.Name == name).RedirectUrl;
                        context.Response.StatusCode = 301;
                        context.Response.Redirect(redirectUrl);
                        await context.Response.WriteAsync("Move to " + redirectUrl);
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
        }
    }
}
