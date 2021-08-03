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

            services.AddGrpc(op =>
            {
                op.MaxReceiveMessageSize = null;
                op.MaxSendMessageSize = null;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGrpcService<DataBaseService>().EnableGrpcWeb();

            });
        }
    }
}
