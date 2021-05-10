using System.Collections.Generic;

namespace SoScienceDataServer.DataClasses
{
    class D_Document
    {
        public int ID;
        public int projectID;
        public string title;
        public string type;
        public string data;
        public float completedCount;
        public List<string> completed;
        public D_Document(int projectID, string title, string data)
        {
            this.projectID = projectID;
            this.title = title;
            this.data = data;
            completed = new List<string>();
        }
        public D_Document(int projectID, string title, string data, int ID)
        {
            this.projectID = projectID;
            this.title = title;
            this.data = data;
            this.ID = ID;
            completed = new List<string>();
        }
        public D_Document(int projectID, string title, int ID, string type)
        {
            this.projectID = projectID;
            this.title = title;
            this.type = type;
            this.ID = ID;
            data = "";
            completed = new List<string>();
        }
        public D_Document(int projectID, string title, int ID, int completedCount)
        {
            this.projectID = projectID;
            this.title = title;
            this.ID = ID;
            this.completedCount = completedCount;
            data = "";
            completed = new List<string>();
        }
        public D_Document(int projectID, string title, string data, int ID, List<string> completed)
        {
            this.projectID = projectID;
            this.title = title;
            this.data = data;
            this.ID = ID;
            this.completed = completed;
        }
    }
}
