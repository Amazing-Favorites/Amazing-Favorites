using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newbe.BookmarkManager.WebApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => options.AddDefaultPolicy(policyBuilder =>
    policyBuilder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(x => true)));
await using var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
string fileName = app.Configuration["Data:Filename"];
app.MapGet("/bk", (async context =>
{
    if (!File.Exists(fileName))
    {
        await context.Response.WriteAsJsonAsync(new GetCloudOutput
        {
            EtagVersion = 0,
            LastUpdateTime = 0
        });
        return;
    }

    await using var file = File.OpenRead(fileName);
    var re = await JsonSerializer.DeserializeAsync<GetCloudOutput>(file);

    if (context.Request.Query.TryGetValue("etagVersion", out var etagVersionStr)
        && long.TryParse(etagVersionStr, out var etagVersion))
    {
        if (etagVersion == re!.EtagVersion)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            return;
        }

        if (etagVersion > re.EtagVersion)
        {
            re.CloudBkCollection = null;
        }
    }

    await context.Response.WriteAsJsonAsync(re);
}));
app.MapPost("/bk", (Func<CloudBkCollection, Task<SaveToCloudOutput>>)(async collection =>
{
    if (File.Exists(fileName))
    {
        File.Delete(fileName);
    }

    await using var file = File.Create(fileName);
    await JsonSerializer.SerializeAsync(file, new GetCloudOutput
    {
        EtagVersion = collection.EtagVersion,
        LastUpdateTime = collection.LastUpdateTime,
        CloudBkCollection = collection
    });
    await file.FlushAsync();
    return new SaveToCloudOutput
    {
        IsOk = true
    };
}));
await app.RunAsync();