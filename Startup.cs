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
                endpoints.Map("/{str}", async context =>
                {
                    var str = context.GetRouteValue("str").ToString();
                    context.Response.StatusCode = 301;
                    context.Response.Redirect(str == "mili" ? "https://github.com/mili-tan" : "https://github.com/");
                    await context.Response.WriteAsync("Moved");
                });
            });
        }
    }
}
