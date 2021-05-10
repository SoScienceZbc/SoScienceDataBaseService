using System;
using System.Collections.Generic;

namespace SoScienceDataServer.DataClasses
{
    class D_Project
    {
        public int id;
        public string name;
        public bool completed;
        public DateTime lastedited;
        public DateTime endDate;
        public List<D_Document> documents;
        public D_Project(string name, bool completed)
        {
            this.name = name;
            this.completed = completed;
            lastedited = DateTime.Now;
            endDate = DateTime.Now;
            documents = new List<D_Document>();
        }
        public D_Project(int id, string name, bool completed, DateTime lastedited)
        {
            this.id = id;
            this.name = name;
            this.completed = completed;
            this.lastedited = lastedited;
            endDate = DateTime.Now;
            documents = new List<D_Document>();
        }
        public D_Project(int id, string name, bool completed, DateTime lastedited, DateTime endDate)
        {
            this.id = id;
            this.name = name;
            this.completed = completed;
            this.lastedited = lastedited;
            this.endDate = endDate;
            documents = new List<D_Document>();
        }
        public D_Project(int id, string name, bool completed, DateTime lastedited, List<D_Document> documents)
        {
            this.id = id;
            this.name = name;
            this.completed = completed;
            this.lastedited = lastedited;
            this.documents = documents;
            endDate = DateTime.Now;
        }
        public D_Project(int id, string name, bool completed, DateTime lastedited, List<D_Document> documents, DateTime endDate)
        {
            this.id = id;
            this.name = name;
            this.completed = completed;
            this.lastedited = lastedited;
            this.documents = documents;
            this.endDate = endDate;
        }
    }
}
