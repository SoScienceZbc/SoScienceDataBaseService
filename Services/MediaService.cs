using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoScienceDataServer;
using Proto;
using Grpc.Core;

namespace DatabaseDocomentService.Services
{
    class MediaService : RemoteMediaService.RemoteMediaServiceBase
    {
#if DEBUG
        DataBaseManager dbm = new DataBaseManager("10.108.239.199");
#else
        DataBaseManager dbm = new DataBaseManager("localhost");
#endif

        public override Task<MediaReply> SendMedia(MediaRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host} called Method:{context.Method}");
            return Task.FromResult(dbm.SendMedia(request));
        }
    }
}
