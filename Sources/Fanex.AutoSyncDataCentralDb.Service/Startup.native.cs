using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Fanex.AutoSyncDataCentralDb
{
    public class Startup
    {
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/",
                    async context => { await context.Response.WriteAsync("Fanex.AutoSyncDataCentralDb.Service"); });
            });
        }
    }
}
