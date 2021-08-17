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
#if DEBUG
        DataBaseManager dbm = new DataBaseManager("192.168.0.220");
#else
        DataBaseManager dbm = new DataBaseManager("localhost");
#endif
#region Logger
        //private readonly ILogger<DataBaseService> _logger;
        public DataBaseService()
        {
            //_logger = logger; ILogger<DataBaseService> logger
        }
#endregion

#region Project
        public override Task<D_Project> GetProject(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            D_Project DData = dbm.GetProject(infomation.ID, infomation.DbName);
            Console.WriteLine(DData.Name);
            return Task.FromResult(DData);
        }
        public override Task<intger> AddProject(ProjectUserInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");

            var temp = new intger { Number = dbm.AddProject(infomation.User.DbName, infomation.Project) };
            return Task.FromResult(temp);
        }
        public override Task<intger> EditProject(ProjectUserInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger { Number = dbm.EditProject(infomation.Project) });
        }

        public override Task<intger> RemoveProject(ProjectUserInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger { Number = dbm.RemoveProject(infomation.Project.Id, infomation.User.DbName) });
        }

        public override Task<D_Projects> GetProjects(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            List<D_Project> something = dbm.GetProjects(infomation.DbName);
            var temp = new D_Projects();
            foreach (var item in something)
            {
                List<D_Document> tempDocs = dbm.GetDocuments(item.Id);
                foreach (D_Document d_Document in tempDocs)
                {
                    item.Documents.Add(d_Document);
                }
                temp.DProject.Add(item);

            }
            return Task.FromResult(temp);
        }

#endregion
#region Docoment
        public override Task<D_Documents> GetDocuments(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            var temp = new D_Documents();
            temp.DDocuments.AddRange(dbm.GetDocuments(infomation.ID));
            return Task.FromResult(temp);
        }
        // documents
        public override Task<intger> AddDocument(D_Document infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger { Number = dbm.AddDocument(infomation) });
        }
        public override Task<D_Document> GetDocument(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(dbm.GetDocument(infomation.ID));
        }
        public override Task<intger> UpdateDocument(D_Document infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger { Number = dbm.UpdateDocument(infomation) });
        }

        public override Task<intger> RemoveDocument(ProjectUserInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger { Number = dbm.RemoveDocument(infomation.Project.Documents[0].ID, infomation.Project.Id) });
        }
#endregion
#region Remote
        public override Task<intger> AddRemoteFile(D_RemoteFile infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(dbm.AddRemoteFile(infomation));
        }
        public override Task<D_RemoteFile> GetRemoteFile(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(dbm.GetRemoteFile(infomation.ID));
        }
        public override Task<D_RemoteFile> UpdateRemoteFile(D_RemoteFile infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(dbm.UpdateRemoteFile(infomation));
        }
        public override Task<intger> RemoveRemoteFile(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger());
        }
        public override Task<D_RemoteFiles> GetRemoteFiles(UserDbInfomation infomation, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new D_RemoteFiles());
        }
#endregion
#region Teacher
        public override Task<D_Teacher> CheckAndInsertTeacher(D_Teacher request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            int count = dbm.CheckTeacher(request.Username);
            if (count <= 0)
            {
                if (string.IsNullOrEmpty(request.School))
                {
                    request.ID = dbm.AddTeacher(request.Username);
                }
                else
                {
                    request.ID = dbm.AddTeacher(request.Username, request.School);
                }
            }
            return Task.FromResult(request);
        }
#endregion
#region Subject
        public override Task<intger> AddSubject(D_Subject request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            var temp = new intger { Number = dbm.AddSubject(request.Name) };
            return Task.FromResult(temp);
        }
        public override Task<D_Subjects> GetSubjects(UserDbInfomation request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            List<D_Subject> subjects = dbm.GetSubjects();
            var temp = new D_Subjects();
            foreach (var item in subjects)
            {
                temp.Subject.Add(item);
            }
            return Task.FromResult(temp);
        }
#endregion
#region Project Theme
        public override Task<intger> AddProjectTheme(D_ProjectTheme request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            var temp = new intger { Number = dbm.AddProjectTheme(request.Name, request.EndDate, request.Teacher, request.Subject) };
            return Task.FromResult(temp);
        }
        public override Task<D_ProjectThemes> GetProjectThemesFromSubject(ThemeFromSubject request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            List<D_ProjectTheme> themes = dbm.GetProjectThemesFromSubject(request.Subject);
            var temp = new D_ProjectThemes();
            foreach (var item in themes)
            {
                temp.ProjectTheme.Add(item);
            }
            return Task.FromResult(temp);
        }
        public override Task<D_ProjectThemes> GetProjectThemes(UserDbInfomation request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            List<D_ProjectTheme> themes = dbm.GetProjectThemes(request.DbName);
            var temp = new D_ProjectThemes();
            foreach (var item in themes)
            {
                item.Projects = dbm.GetProjectsFromProjectTheme(item.ID);
                temp.ProjectTheme.Add(item);
            }
            return Task.FromResult(temp);
        }
        public override Task<intger> AddProjectThemeCoTeacher(ProjectThemeUserInfomation request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger() { Number = dbm.AddProjectThemeCoTeacher(request.Theme.ID, request.Username) });
        }
        public override Task<intger> RemoveProjectTheme(ProjectThemeUserInfomation request, ServerCallContext context)
        {
            Console.WriteLine($"Host:{context.Host}\nMethod: {context.Method}");
            return Task.FromResult(new intger() { Number = dbm.RemoveProjectTheme(request.Theme.ID) });
        }
#endregion
    }
}
