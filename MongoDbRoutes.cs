using System.IO;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;

namespace mJump
{
    class MongoDbRoutes
    {
        public static MongoClient Client;
        public static IMongoDatabase Database;
        private static IMongoCollection<JumpEntity> Collection;

        public static void Route(IEndpointRouteBuilder endpoints)
        {
            endpoints.Map($"/{Startup.UID}/add", async context =>
            {
                var query = context.Request.Query;
                if (query.TryGetValue("url", out var url) && query.TryGetValue("name", out var name))
                {
                    if (await Collection.Find(x => x.Name == name).CountDocumentsAsync() > 0)
                        await context.Response.WriteAsync("Already Exists");
                    else
                    {
                        await Collection.InsertOneAsync(new JumpEntity
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
                var find = Collection.Find(x => x.Name == name);
                if (await find.CountDocumentsAsync() > 0)
                {
                    var entity = find.FirstOrDefault();
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

        public static void Init(string connection, string dbName = "mJump", string collection = "mJump")
        {
            Client = new MongoClient(connection);
            Database = Client.GetDatabase(dbName);
            Collection = Database.GetCollection<JumpEntity>(collection);
            Collection.Indexes.CreateOne(
                new CreateIndexModel<JumpEntity>(Builders<JumpEntity>.IndexKeys.Text(x => x.Name)));
        }

        public class JumpEntity
        {
            public MongoDB.Bson.ObjectId Id { get; set; }
            public string Name { get; set; }
            public string RedirectUrl { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
