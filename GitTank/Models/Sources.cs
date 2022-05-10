using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTank.Models
{
    public class Sources
    {
        [JsonProperty("sourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty("repositories")]
        public List<string> Repositories { get; set; }
    }
}
