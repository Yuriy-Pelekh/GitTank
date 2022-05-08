using System.Collections.Generic;

namespace GitTank.Models
{
    public class Sources
    {
        public string sourcePath { get; set; }
        public List<string> repositories { get; set; }
    }
}
