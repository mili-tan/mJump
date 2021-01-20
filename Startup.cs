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
using MongoDB.Driver;

namespace mJump
{
    public class Startup
    {
        public static Guid UID = Guid.NewGuid();
        private static LiteDatabase myLiteDB;
        private static ILiteCollection<JumpEntity> liteCollection;
        private static MongoClient myMongoClient;
        private static IMongoDatabase myMongoDatabase;
        private static IMongoCollection<MJumpEntity> mongoCollection;
        private static bool IsMongo = File.Exists("mongo.txt");

        public void ConfigureServices(IServiceCollection services)
        {
            if (IsMongo)
            {
                myMongoClient = new MongoClient(File.ReadAllText("mongo.txt"));
                myMongoDatabase = myMongoClient.GetDatabase("mJump");
                mongoCollection = myMongoDatabase.GetCollection<MJumpEntity>("mJump");
                mongoCollection.Indexes.CreateOne(
                    new CreateIndexModel<MJumpEntity>(Builders<MJumpEntity>.IndexKeys.Text(x => x.Name)));
            }
            else
            {
                myLiteDB = new LiteDatabase("mjump.db");
                liteCollection = myLiteDB.GetCollection<JumpEntity>("MJUMP");
                liteCollection.EnsureIndex(x => x.Name, true);
            }
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
                endpoints.Map("/IsMongo", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("IsMongo:" + IsMongo);
                });
                endpoints.Map($"/{UID}/add", async context =>
                {
                    var query = context.Request.Query;
                    if (query.TryGetValue("url", out var url) && query.TryGetValue("name", out var name))
                    {
                        if (IsMongo
                            ? await mongoCollection.Find(x => x.Name == name).CountDocumentsAsync() > 0
                            : liteCollection.Exists(x => x.Name == name))
                            await context.Response.WriteAsync("Already Exists");
                        else
                        {
                            if (IsMongo)
                            {
                                await mongoCollection.InsertOneAsync(new MJumpEntity
                                {
                                    StatusCode = query.TryGetValue("code", out var code) ? int.Parse(code) : 302,
                                    RedirectUrl = HttpUtility.UrlDecode(url.ToString()),
                                    Name = name.ToString()
                                });
                            }
                            else
                            {
                                liteCollection.Insert(new JumpEntity
                                {
                                    StatusCode = query.TryGetValue("code", out var code) ? int.Parse(code) : 302,
                                    RedirectUrl = HttpUtility.UrlDecode(url.ToString()),
                                    Name = name.ToString()
                                });
                            }

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
                    var find = IsMongo ? mongoCollection.Find(x => x.Name == name) : null;
                    if (IsMongo ? await find.CountDocumentsAsync() > 0 : liteCollection.Exists(x => x.Name == name))
                    {
                        dynamic entity = IsMongo ? find.FirstOrDefault() : liteCollection.FindOne(x => x.Name == name);
                        context.Response.StatusCode = entity.StatusCode;
                        context.Response.Redirect(entity.RedirectUrl);
                        await context.Response.WriteAsync("Move to " + (entity.RedirectUrl as string));
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

        public class MJumpEntity
        {
            public MongoDB.Bson.ObjectId Id { get; set; }
            public string Name { get; set; }
            public string RedirectUrl { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
