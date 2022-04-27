using System.Collections.Generic;

namespace GitTank.Dto
{
    public class ConfigurationDto
    {
        public AppSettings appSettings { get; set; }
    }

    public class AppSettings
    {
        public List<SourcePathes> sourcePathes { get; set; }
        public string defaultRepository { get; set; }
        public string defaultBranch { get; set; }
    }

    public class SourcePathes
    {
        public string sourcePath { get; set; }
        public List<string> repositories { get; set; }
    }
}
