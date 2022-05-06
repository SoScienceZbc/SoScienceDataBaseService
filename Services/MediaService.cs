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
        public override Task<MediaRequests> GetMedias(ProjectInformation project, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host} called Method:{context.Method}");
            MediaRequests medias = new MediaRequests();
            medias.AllMedias.AddRange(dbm.GetMedias(project));
            return Task.FromResult(medias);
        }
        public override Task<RetrieveMediaReply> RetrieveMedia(RetrieveMediaRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host} called Method:{context.Method}");
            return Task.FromResult(dbm.RetrieveMedia(request));
        }
        public override Task<MediaReply> DeleteMedia(RetrieveMediaRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host} called Method:{context.Method}");
            return Task.FromResult(dbm.DeleteMedia(request));
        }
        public override Task<MediaReply> UpdateMedia(ChangeTitleRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host} called Method:{context.Method}");
            return Task.FromResult(dbm.UpdateMedia(request));
        }
    }
}
