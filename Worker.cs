using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseDocomentService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace DatabaseDocomentService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        #region ServiceSetup
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //TODO: AddSSL in the "lo.UseHttps(Path,Name)"
            _logger.LogInformation("Worker running at: {time} just before Creating DocomentServce", DateTimeOffset.Now);
            await Host.CreateDefaultBuilder().ConfigureWebHostDefaults(cw =>
            {
                cw.UseKestrel().UseStartup<GrpcAgent>().ConfigureKestrel(kj =>
                {
                    kj.Listen(System.Net.IPAddress.Any, 48041, lo =>
                    {
                        //lo.UseHttps();
                        lo.Protocols = HttpProtocols.Http1;
                    });
                });
            }).Build().StartAsync(stoppingToken);           

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
        #endregion
    }
}
