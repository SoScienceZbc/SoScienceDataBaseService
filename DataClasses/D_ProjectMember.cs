namespace SoScienceDataServer.DataClasses
{
    class D_ProjectMember
    {
        public int projectID;
        public string user;
        public D_ProjectMember(string user, int projectID)
        {
            this.projectID = projectID;
            this.user = user;
        }
    }
}
