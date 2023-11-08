using AutoSyncData.Models;
using Fanex.AutoSyncDataCentralDb.Service;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFanexSql(builder.Configuration);
builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.InitializeServices(builder.Configuration);

builder.Services.AddHostedService<AutoSyncDataService>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

app.Run();