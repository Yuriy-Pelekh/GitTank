using Newtonsoft.Json;
using System.Collections.Generic;

namespace GitTank.Models
{
    public class AppSettings
    {
        [JsonProperty("Sources")]
        public List<Sources> Sources { get; set; }

        [JsonProperty("DefaultRepository")]
        public string DefaultRepository { get; set; }

        [JsonProperty("DefaultBranch")]
        public string DefaultBranch { get; set; }
    }
}
