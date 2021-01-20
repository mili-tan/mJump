using System.Web;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace mJump
{
    class LiteDbRoutes
    {
        public static LiteDatabase Database;
        public static ILiteCollection<JumpEntity> Collection;
        public static void Route(IEndpointRouteBuilder endpoints)
        {
            endpoints.Map($"/{Startup.UID}/add", async context =>
            {
                var query = context.Request.Query;
                if (query.TryGetValue("url", out var url) && query.TryGetValue("name", out var name))
                {
                    if (Collection.Exists(x => x.Name == name))
                        await context.Response.WriteAsync("Already Exists");
                    else
                    {
                        Collection.Insert(new JumpEntity
                        {
                            StatusCode = query.TryGetValue("code", out var code) ? int.Parse(code) : 302,
                            RedirectUrl = HttpUtility.UrlDecode(url.ToString()),
                            Name = name.ToString()
                        });
                        await context.Response.WriteAsync("OK");
                    }
                }
                else await context.Response.WriteAsync("Invalid");
            });
            endpoints.Map("/{Name}", async context =>
            {
                var name = context.GetRouteValue("Name").ToString();
                if (Collection.Exists(x => x.Name == name))
                {
                    var entity = Collection.FindOne(x => x.Name == name);
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
        }

        public static void Init(string dbName = "mJump.db", string collection = "mJump")
        {
            Database = new LiteDatabase(dbName);
            Collection = Database.GetCollection<JumpEntity>(collection);
            Collection.EnsureIndex(x => x.Name, true);
        }

        public class JumpEntity
        {
            public string Name { get; set; }
            public string RedirectUrl { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
