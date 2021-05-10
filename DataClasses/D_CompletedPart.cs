namespace SoScienceDataServer.DataClasses
{
    class D_CompletedPart
    {
        public string partTitle;
        public int documentID;
        public D_CompletedPart(string partTitle, int documentID)
        {
            this.partTitle = partTitle;
            this.documentID = documentID;
        }
    }
}
