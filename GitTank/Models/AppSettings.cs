using System.Collections.Generic;

namespace GitTank.Models
{
    public class AppSettings
    {
        public List<SourcePaths> SourcePaths { get; set; }
        public string DefaultRepository { get; set; }
        public string DefaultBranch { get; set; }
    }
}
