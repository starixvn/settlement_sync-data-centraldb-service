using AutoSyncData.Models;
using Fanex.AutoSyncDataCentralDb.Service;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFanexSql(builder.Configuration);
builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.InitializeServices(builder.Configuration);

builder.Services.AddHostedService<AutoSyncDataService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseRouting();

app.MapGet("/",
    async context => { await context.Response.WriteAsync("Fanex.AutoSyncDataCentralDb.Service"); });
#if DEBUG
app.Run();
#else
await app.RunAsync("http://0.0.0.0:5000");
#endif