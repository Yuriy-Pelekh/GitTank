using System.Collections.Generic;

namespace GitTank.Models
{
    public class AppSettings
    {
        public List<Sources> Sources { get; set; }
        public string DefaultRepository { get; set; }
        public string DefaultBranch { get; set; }
        public bool newUI { get; set; }
    }
}
