using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using DatabaseDocomentService.Services;

namespace DatabaseDocomentService
{
    class GrpcAgent
    {
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGrpcService<DataBaseService>();
               // endpoints.MapGet("/", async x => await x.Response.WriteAsync("Something have happen"));

                Console.WriteLine("Hello from GrpcAgent inside maping services");

                //endpoints.MapGet("/", async x => await x.Response.WriteAsync("Something have happen"));
                //endpoints.MapGet("/", async context => {await context.Response.WriteAsync("Your are not to use this in this manner.");});
            });
        }
    }
}
