using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using DatabaseService_Grpc;

namespace SoScienceDataServer
{
    class DataBaseManager
    {
        private string con;
        //private IHashing hashing;
        private SHA256 hashing;
        public DataBaseManager(string db)
        {
            con = @"Server=" + db + ";Database=SoScience;User Id=SoScienceExecuter;Password=k6UwAf4K*puBTEb^";
            //hashing = new SHA256();
            hashing = SHA256.Create();
        }

        #region project
        public int AddProject(string username, D_Project project)
        {
            int id = 0;
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPInsertProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@username", SqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = project.Name;
                    cmd.Parameters.Add("@completed", SqlDbType.Bit).Value = project.Completed;
                    cmd.Parameters.Add("@lastEdited", SqlDbType.DateTime).Value = project.Lastedited;
                    cmd.Parameters.Add("@endDate", SqlDbType.DateTime).Value = project.EndDate;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPUpdateProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = project.Id;
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = project.Name;
                    cmd.Parameters.Add("@completed", SqlDbType.Bit).Value = project.Completed;
                    cmd.Parameters.Add("@lastEdited", SqlDbType.DateTime).Value = project.Lastedited;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@username", SqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        project = new D_Project { Id = reader.GetInt32("ID"), Name = reader.GetString("name"), Completed = reader.GetBoolean("completed"), Lastedited = reader.GetDateTime("lastEdited").ToString(), EndDate = reader.GetDateTime("EndDate").ToString() };
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

        public int RemoveProject(int id, string username)
        {
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPDeleteProject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@username", SqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(username)));
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetProjects", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@username", SqlDbType.VarChar).Value = Convert.ToBase64String(hashing.ComputeHash(Encoding.Unicode.GetBytes(user)));

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        projects.Add(new D_Project { Id = reader.GetInt32("ID"), Name = reader.GetString("name"), Completed = reader.GetBoolean("completed"), Lastedited = reader.GetDateTime("lastEdited").ToString(), EndDate = reader.GetDateTime("EndDate").ToString()});
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetDocumentsSimple", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        documents.Add(new D_Document{ ProjectID=reader.GetInt32("ProjectID"), Title=reader.GetString("Title"), ID=reader.GetInt32("ID"), CompletedCount=reader.GetInt32("completed")});
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                for (int i = 0; i < documents.Count; i++)
                {

                    con.Close();
                    using (SqlCommand cmd = new SqlCommand("SPGetCompletedParts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //@id int, @name nvarchar(255), @completed BIT
                        cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = documents[i].ID;

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
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
        #endregion
        #region document
        public int AddDocument(D_Document document)
        {
            int id = 0;
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPInsertDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = document.ProjectID;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = document.Title;
                    cmd.Parameters.Add("@data", SqlDbType.Text).Value = document.Data;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = Convert.ToInt32(reader.GetDecimal(0));
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                foreach (string item in document.Completed)
                {

                    using (SqlCommand cmd = new SqlCommand("SPInsertCompleted", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@did", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = item;

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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPUpdateDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = document.ID;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = document.Title;
                    cmd.Parameters.Add("@data", SqlDbType.Text).Value = document.Data;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        did = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd.Dispose();
                }

                using (SqlCommand cmd = new SqlCommand("SPClearCompleted", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@did", SqlDbType.Int).Value = did;

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                foreach (string item in document.Completed)
                {

                    using (SqlCommand cmd = new SqlCommand("SPInsertCompleted", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@did", SqlDbType.Int).Value = did;
                        cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = item;

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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        document = new D_Document { ProjectID = reader.GetInt32("ProjectID"), Title = reader.GetString("title"), Data = reader.GetString("data"), ID = reader.GetInt32("ID") };
                    }
                    reader.Close();
                    cmd.Dispose();
                }
                if (document != null)
                {
                    using (SqlCommand cmd = new SqlCommand("SPGetCompletedParts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //@id int, @name nvarchar(255), @completed BIT
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = document.ID;


                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            document.Completed.Add(reader.GetString("title"));
                        }
                        reader.Close();
                        cmd.Dispose();
                    }
                }
            }
            return document;
        }

        public int RemoveDocument(int documentId, int projectId)
        {
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPDeleteDocument", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = documentId;
                    cmd.Parameters.Add("@pid", SqlDbType.Int).Value = projectId;

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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPInsertRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@pid", SqlDbType.Int).Value = remoteFile.ProjectID;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = remoteFile.Title;
                    cmd.Parameters.Add("@type", SqlDbType.VarChar).Value = remoteFile.Type;
                    cmd.Parameters.Add("@path", SqlDbType.VarChar).Value = remoteFile.Path;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPUpdateRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = remoteFile.ID;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = remoteFile.Title;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPDeleteRFile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = RemoteFileId;
                    cmd.Parameters.Add("@pid", SqlDbType.Int).Value = projectId;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
            using (SqlConnection con = new SqlConnection(this.con))
            {
                using (SqlCommand cmd = new SqlCommand("SPGetRFiles", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //@id int, @name nvarchar(255), @completed BIT
                    cmd.Parameters.Add("@pid", SqlDbType.Int).Value = id;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
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
    }
}
