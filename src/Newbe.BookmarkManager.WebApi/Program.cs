using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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
string fileName = "data.json";
app.MapGet("/bk", (Func<long, Task<GetCloudOutput>>) (async etagVersion =>
{
    if (!File.Exists(fileName))
    {
        return new GetCloudOutput
        {
            EtagVersion = 0,
            LastUpdateTime = 0
        };
    }

    await using var file = File.OpenRead(fileName);
    var re = await JsonSerializer.DeserializeAsync<GetCloudOutput>(file);
    if (etagVersion <= re!.EtagVersion)
    {
        re.CloudBkCollection = null;
    }
    return re;
}));
app.MapPost("/bk", (Func<CloudBkCollection, Task<SaveToCloudOutput>>) (async collection =>
{
    await using var file = !File.Exists(fileName) ? File.Create(fileName) : File.OpenWrite(fileName);
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