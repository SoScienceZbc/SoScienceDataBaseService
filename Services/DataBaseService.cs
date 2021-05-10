using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using System.Threading.Tasks;
using DatabaseService_Grpc;
using Microsoft.Extensions.Logging;
using SoScienceDataServer;

namespace DatabaseDocomentService.Services
{
    public class DataBaseService : GrpcDatabaseProject.GrpcDatabaseProjectBase
    {
        DataBaseManager dbm = new DataBaseManager("192.168.0.220");
        #region Logger
        //private readonly ILogger<DataBaseService> _logger;
        public DataBaseService()
        {
            Console.WriteLine("Hello from Database Service");
            //_logger = logger; ILogger<DataBaseService> logger
        }
        #endregion

        #region Project
        public override Task<D_Project> GetProject(UserDbInfomation infomation, ServerCallContext context)
        {
            D_Project DData = dbm.GetProject(infomation.ID, infomation.DbName);
            return Task.FromResult(DData);
        }
        public override Task<intger> AddProject(ProjectUserInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine("Hello from the databaser service inside a rpc call named addproject");

            var temp = new intger { Number = dbm.AddProject(infomation.User.DbName, infomation.Project) };
            return Task.FromResult(temp);
        }
        public override Task<intger> EditProject(ProjectUserInfomation infomation, ServerCallContext context)
        {

            return Task.FromResult(new intger { Number = dbm.EditProject(infomation.Project) });
        }

        public override Task<intger> RemoveProject(ProjectUserInfomation infomation, ServerCallContext context)
        {
            return Task.FromResult(new intger { Number = dbm.RemoveDocument(infomation.Project.Id, infomation.User.ID) });
        }

        public override Task< D_Projects> GetProjects(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine("Returning project ");
            List<D_Project> something = dbm.GetProjects(infomation.DbName);
            var temp = new D_Projects();
            foreach (var item in something)
            {
                temp.DProject.Add(item);
            }
            return Task.FromResult(temp);
        }

        #endregion
        #region Docoment
        D_Documents GetDocuments(UserDbInfomation infomation)
        {
            var temp = new D_Documents();
            temp.DDocuments.AddRange(dbm.GetDocuments(infomation.ID));
            return temp;
        }
        // documents
        intger AddDocument(D_Document infomation)
        {
            return new intger { Number = dbm.AddDocument(infomation) };
        }
        D_Document GetDocument(UserDbInfomation infomation) 
        {
            return dbm.GetDocument(infomation.ID); 
        }
        intger UpdateDocument(D_Document infomation) 
        { 
            return new intger {Number = dbm.UpdateDocument(infomation)}; 
        }

        intger RemoveDocument(UserDbInfomation infomation) { return new intger { Number = dbm.RemoveProject(infomation.ID, infomation.DbName) }; }
        #endregion
        #region Remote
        public override Task<intger> AddRemoteFile(D_RemoteFile infomation, ServerCallContext context) { return Task.FromResult(dbm.AddRemoteFile(infomation)); }
        public override Task<D_RemoteFile> GetRemoteFile(UserDbInfomation infomation, ServerCallContext context) { return Task.FromResult(dbm.GetRemoteFile(infomation.ID)); }
    public override Task<D_RemoteFile> UpdateRemoteFile(D_RemoteFile infomation, ServerCallContext context) { return Task.FromResult(dbm.UpdateRemoteFile(infomation)); }
        public override Task<intger> RemoveRemoteFile(UserDbInfomation infomation, ServerCallContext context) { return Task.FromResult(new intger()); }
public override Task<D_RemoteFiles> GetRemoteFiles(UserDbInfomation infomation, ServerCallContext context) { return Task.FromResult(new D_RemoteFiles()); }
        #endregion

        #region ProtobufConvert
        #endregion
    }
}
