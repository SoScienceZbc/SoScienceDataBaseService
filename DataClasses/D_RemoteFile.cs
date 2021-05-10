using System;
using System.Collections.Generic;
using System.Text;

namespace SoScienceDataServer.DataClasses
{
    class D_RemoteFile
    {
        public int ID;
        public string title;
        public string type;
        public int projectID;
        public string path;
        public D_RemoteFile(int ID, string title, string type, int projectID)
        {
            this.ID = ID;
            this.title = title;
            this.type = type;
            this.projectID = projectID;
            this.path = "";
        }
        public D_RemoteFile(int ID, string title, string type, int projectID,string path)
        {
            this.ID = ID;
            this.title = title;
            this.type = type;
            this.projectID = projectID;
            this.path = path;
        }
    }
}
