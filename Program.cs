using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseDocomentService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    //Test code below..
                    //services.AddGrpc().Services.BuildServiceProvider(new ServiceProviderOptions {ValidateOnBuild = true });
                    //Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(x =>
                    //{
                    //    x.UseStartup<GrpcAgent>();
                    //});
                    //services.AddHostedService<GrpcAgent>();                    

                });
    }
}
