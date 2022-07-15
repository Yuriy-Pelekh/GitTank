namespace GitTank.Common
{
    public delegate void OutputPerRepositoryEventHandler(int repositoryIndex, string line);
    public delegate void CreateBranchEventHandler();
}
