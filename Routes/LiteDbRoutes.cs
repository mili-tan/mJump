using System;
using System.Linq;
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
        public static ILiteCollection<BsonDocument> Collection;
        public static void Route(IEndpointRouteBuilder endpoints)
        {
            endpoints.Map(Startup.IsPublic ? "/add" : $"/{Startup.UID}/add", async context =>
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                var query = context.Request.Query;
                if (query.TryGetValue("url", out var url))
                {
                    var urlDecode = HttpUtility.UrlDecode(url.ToString());
                    var nameStr = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "-")
                        .Replace("+", "_").Replace("=", "").Substring(0, 8);
                    if (query.TryGetValue("name", out var name)) nameStr = name.ToString().Split('.').FirstOrDefault();
                    else if (Collection.Exists(x => x["RedirectUrl"] == urlDecode))
                    {
                        var findName = Collection.FindOne(x => x["RedirectUrl"] == urlDecode)["Name"].AsString;
                        await context.Response.WriteAsync(Startup.BaseURL + "/" + findName);
                        return;
                    }
                    if (Collection.Exists(x => x["Name"] == nameStr)) await context.Response.WriteAsync("Already Exists");
                    else
                    {
                        Collection.Insert(new BsonDocument
                        {
                            ["StatusCode"] = query.TryGetValue("code", out var code) ? int.Parse(code) : 302,
                            ["RedirectUrl"] = urlDecode,
                            ["Name"] = nameStr
                        });
                        await context.Response.WriteAsync(Startup.BaseURL + "/" + nameStr);
                    }
                }
                else await context.Response.WriteAsync("Invalid");
            });
            endpoints.Map("/{Name}", async context =>
            {
                var name = context.GetRouteValue("Name").ToString().Split('.').FirstOrDefault();
                if (Collection.Exists(x => x["Name"] == name))
                {
                    var entity = Collection.FindOne(x => x["Name"] == name);
                    context.Response.Redirect(entity["RedirectUrl"].AsString);
                    context.Response.StatusCode = entity["StatusCode"].AsInt32;
                    await context.Response.WriteAsync("Move to " + entity["RedirectUrl"].AsString);
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
            Collection = Database.GetCollection<BsonDocument>(collection);
            Collection.EnsureIndex(x => x["Name"], true);
        }
    }
}
