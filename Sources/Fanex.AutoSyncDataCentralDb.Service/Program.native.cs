using AutoSyncData.Models;
using Fanex.AutoSyncDataCentralDb.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddFanexLogSentry(builder.Configuration);

services.AddFanexSql(builder.Configuration);

services.AddConfigRedisCache(builder.Configuration);

services.Configure<AppSettingsKey>(builder.Configuration);

services.InitializeServices(builder.Configuration);

services.AddHostedService<AutoSyncDataService>();
services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/healthz");
await app.RunAsync();