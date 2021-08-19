using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using DatabaseService_Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace SoScienceDataServer
{
    class DataBaseManager
    {
        private string con;
        static IConfiguration Config;
        //private IHashing hashing;
        private SHA256 hashing;
        public DataBaseManager(string db)
        {
#if DEBUG
            Config = new ConfigurationBuilder().AddJsonFile("./AppCode.json").Build();
#else
            Config = new ConfigurationBuilder().AddJsonFile("/home/soscienceadmin/Services/AppCode.json").Build();
#endif

            con =
                $"SERVER={db};DATABASE={Config.GetSection("DbConnectionConfig")["Database"]};" +
                $"UID={Config.GetSection("DbConnectionConfig")["User Id"]};" +
                $"PASSWORD={Config.GetSection("DbConnectionConfig")["Password"]}";
            hashing = SHA256.Create();
        }

        static string getRootPath(string rootFilename)
        {
            string _root;
            var rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex matchThepath = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = matchThepath.Match(rootDir).Value;
            _root = Path.Combine(appRoot, rootFilename);

            return _root;
        }


#region project
        public int AddProject(string username, D_Project project)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = project.Name;
                    cmd.Parameters.Add("@completed", MySqlDbType.Bit).Value = project.Completed;
                    cmd.Parameters.Add("@lastEdited", MySqlDbType.DateTime).Value = new DateTime();
                    cmd.Parameters.Add("@ProjectThemeID", MySqlDbType.Int32).Value = project.ProjectThemeID;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }

        public int EditProject(D_Project project)
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPUpdateProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = project.Id;
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = project.Name;
                    cmd.Parameters.Add("@completed", MySqlDbType.Bit).Value = project.Completed;
                    

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32("ID");
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }

        public D_Project GetProject(int id, string username)
        {
            D_Project project = null;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        project = new D_Project { Id = reader.GetInt32("ID"), Name = reader.GetString("name"), Completed = reader.GetBoolean("completed"), Lastedited = reader.GetDateTime("lastEdited").ToString(), EndDate = GetProjectEndDate(reader.GetInt32("ProjectThemeID")) };
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            if (project != null)
            {

                project.Documents.AddRange(GetDocuments(id));
                project.Documents.AddRange(GetRemoteFiles(id));
            }
            return project;
        }
        public D_Projects GetProjectsFromProjectTheme(int id)
        {
            D_Projects projects = new D_Projects();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProjectsFromProjectTheme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        projects.DProject.Add(new D_Project { 
                            Id = reader.GetInt32("ID"), 
                            Name = reader.GetString("name"), 
                            Completed = reader.GetBoolean("completed"), 
                            Lastedited = reader.GetDateTime("lastEdited").ToString(), 
                            EndDate = reader.GetDateTime("EndDate").ToString(), 
                            CloseToDeletion = reader.GetBoolean("CloseToDeletion") });
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return projects;
        }
        private string GetProjectEndDate(int id)
        {
            string endDate = "";
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProjectTheme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        endDate = reader.GetDateTime("Enddate").ToString();
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return endDate;
        }
        /// <summary>
        /// deletes a project in the database
        /// </summary>
        /// <param name="id">The projectId</param>
        /// <param name="username">The unilogin Name</param>
        /// <returns></returns>
        public int RemoveProject(int id, string username)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPDeleteProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32("ID");
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }
        public List<D_Project> GetProjects(string user)
        {
            List<D_Project> projects = new List<D_Project>();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProjects", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(user)));

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        projects.Add(new D_Project { 
                            Id = reader.GetInt32("ID"), 
                            Name = reader.GetString("name"), 
                            Completed = reader.GetBoolean("completed"), 
                            Lastedited = reader.GetDateTime("lastEdited").ToString(), 
                            EndDate = reader.GetDateTime("EndDate").ToString(), 
                            CloseToDeletion = reader.GetBoolean("CloseToDeletion") });
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return projects;
        }

        public List<D_Document> GetDocuments(int id)
        {
            List<D_Document> documents = new List<D_Document>();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetDocumentsSimple", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = id;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        documents.Add(new D_Document
                        {
                            ProjectID = reader.GetInt32("ProjectID"),
                            Title = reader.GetString("Title"),
                            ID = reader.GetInt32("ID"),
                            CompletedCount = reader.GetInt32("completed")
                        });
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                for (int i = 0; i < documents.Count; i++)
                {

                    con.Close();
                    using (MySqlCommand cmd = new MySqlCommand("SPGetCompletedParts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //@id int, @name nvarchar(255), @completed BIT
                        cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = documents[i].ID;

                        con.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            documents[i].Completed.Add(reader.GetString("Title"));
                        }
                        documents[i].CompletedCount = documents[i].Completed.Count;
                        reader.Close();
                        cmd.Dispose();
                    }
                }
            }
            return documents;
        }
        public int AddProjectMember(int id, string username)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertProjectMember", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return id;
        }
        public int RemoveProjectMember(int id, string username)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPDeleteProjectMember", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return id;
        }
#endregion
#region document
        public int AddDocument(D_Document document)
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = document.ProjectID;
                    cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = document.Title;
                    cmd.Parameters.Add("@data", MySqlDbType.LongText).Value = document.Data;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = Convert.ToInt32(reader.GetDecimal(0));
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                foreach (string item in document.Completed)
                {

                    using (MySqlCommand cmd = new MySqlCommand("SPInsertCompleted", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@did", MySqlDbType.Int32).Value = id;
                        cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = item;

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
            }
            return id;
        }

        public int UpdateDocument(D_Document document)
        {
            int did = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPUpdateDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = document.ID;
                    cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = document.Title;
                    cmd.Parameters.Add("@data", MySqlDbType.LongText).Value = document.Data;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        did = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }

                using (MySqlCommand cmd = new MySqlCommand("SPClearCompleted", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@did", MySqlDbType.Int32).Value = did;

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                foreach (string item in document.Completed)
                {

                    using (MySqlCommand cmd = new MySqlCommand("SPInsertCompleted", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@did", MySqlDbType.Int32).Value = did;
                        cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = item;

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
            }
            return document.ID;
        }

        public D_Document GetDocument(int id)
        {
            D_Document document = null;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        document = new D_Document
                        {
                            ProjectID = reader.GetInt32("ProjectID"),
                            Title = reader.GetString("title"),
                            Data = reader.GetString("data"),
                            ID = reader.GetInt32("ID"),
                        };
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                if (document != null)
                {
                    using (MySqlCommand cmd = new MySqlCommand("SPGetCompletedParts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //@id int, @name nvarchar(255), @completed BIT
                        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = document.ID;


                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            document.Completed.Add(reader.GetString("title"));
                        }
                        reader.Close();
                        cmd.Dispose();
                    }
                    document.CompletedCount = document.Completed.Count;
                }
            }
            return document;
        }

        public int RemoveDocument(int documentId, int projectId)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPDeleteDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = documentId;
                    cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = projectId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return documentId;
        }
#endregion
#region remoteFile
        public intger AddRemoteFile(D_RemoteFile remoteFile)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = remoteFile.ProjectID;
                    cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = remoteFile.Title;
                    cmd.Parameters.Add("@type", MySqlDbType.VarChar).Value = remoteFile.Type;
                    cmd.Parameters.Add("@path", MySqlDbType.VarChar).Value = remoteFile.Path;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        remoteFile.ID = Convert.ToInt32(reader.GetDecimal(0));
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            //return /*remoteFile*/;
            return new intger { Number = 1 };
        }

        public D_RemoteFile GetRemoteFile(int id)
        {
            D_RemoteFile remoteFile = new D_RemoteFile();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        remoteFile.ID = reader.GetInt32("ID");
                        remoteFile.Title = reader.GetString("Title");
                        remoteFile.Type = reader.GetString("Type");
                        remoteFile.Path = reader.GetString("Path");
                        remoteFile.ProjectID = reader.GetInt32("ProjectID");
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return remoteFile;
        }

        public D_RemoteFile UpdateRemoteFile(D_RemoteFile remoteFile)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPUpdateRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = remoteFile.ID;
                    cmd.Parameters.Add("@title", MySqlDbType.VarChar).Value = remoteFile.Title;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        remoteFile.ID = reader.GetInt32("ID");
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return remoteFile;
        }
        public int RemoveRemoteFile(int RemoteFileId, int projectId)
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPDeleteRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = RemoteFileId;
                    cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = projectId;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32("ID");
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }

        public List<D_Document> GetRemoteFiles(int id)
        {
            List<D_Document> files = new List<D_Document>();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetRFiles", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = id;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        //ID,Title,ProjectID,Type
                        files.Add(new D_Document { ProjectID = reader.GetInt32("ProjectID"), Title = reader.GetString("Title"), ID = reader.GetInt32("ID"), Type = reader.GetString("Type") });
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return files;
        }
#endregion
#region Teacher
        public int CheckTeacher(string username)
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPCheckTeacher", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }

        public int AddTeacher(string username, string school = "ZBC Slagelse")
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertTeacher", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    cmd.Parameters.Add("@schoolname", MySqlDbType.VarChar).Value = school;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }
#endregion
#region Subject
        public int AddSubject(string name)
        {
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertSubject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SName", MySqlDbType.VarChar).Value = name;
                    

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }

        public List<D_Subject> GetSubjects()
        {
            List<D_Subject> subjects = new List<D_Subject>();
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetSubjects", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        subjects.Add(new D_Subject() { Name = reader.GetString(1), ID = reader.GetInt32(0) });
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return subjects;
        }

#endregion
#region Project Theme
        public int AddProjectTheme(string name, string endDate, string teacherName, string subject)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertProjectTheme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ProjectName", MySqlDbType.VarChar).Value = name;
                    cmd.Parameters.Add("@ProjectThemeEndDate", MySqlDbType.DateTime).Value = DateTime.ParseExact(endDate, "dd/MM/yyyy HH:mm:ss", provider);
                    cmd.Parameters.Add("@TeacherName", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(teacherName)));
                    cmd.Parameters.Add("@SubjectName", MySqlDbType.VarChar).Value = subject;


                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return id;
        }
        public List<D_ProjectTheme> GetProjectThemesFromSubject(D_Subject subject)
        {
            List<D_ProjectTheme> themes = new List<D_ProjectTheme>();
            int id = 0;
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProjectThemeFromSubject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SubjectName", MySqlDbType.VarChar).Value = subject.Name;
                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        themes.Add(new D_ProjectTheme() { Name = reader.GetString(1), ID = reader.GetInt32(0) });
                        id = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return themes;
        }
        public List<D_ProjectTheme> GetProjectThemes(string teacherName)
        {
            List<D_ProjectTheme> themes = new List<D_ProjectTheme>();
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPGetProjectTheme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@teacher", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(teacherName)));
                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        themes.Add(new D_ProjectTheme() { Name = reader.GetString(1), ID = reader.GetInt32(0), EndDate = reader.GetDateTime(2).ToString("dd/MM/yyyy HH:mm:ss"), LastEdited = reader.GetDateTime(3).ToString("dd/MM/yyyy HH:mm:ss"), Subject = reader.GetString(4) });
                    }
                    reader.Close();
                    cmd.Dispose();
                }
            }
            return themes;
        }
        public int RemoveProjectTheme(int id)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPDeleteProjectTheme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return id;

        }
        public int AddProjectThemeCoTeacher(int id, string username)
        {

            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using (MySqlCommand cmd = new MySqlCommand("SPInsertCoTeacher", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return id;
        }
        public int RemoveProjectThemeCoTeacher(string username, int id)
        {
            using (MySqlConnection con = new MySqlConnection(this.con))
            {
                using(MySqlCommand cmd = new MySqlCommand("SPDeleteProjectThemeCoTeacher", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@P_ID", MySqlDbType.Int32).Value = id;
                    cmd.Parameters.Add("@T_Name", MySqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return id;
        }
#endregion
    }
}
