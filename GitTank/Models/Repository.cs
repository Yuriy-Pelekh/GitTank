using Newtonsoft.Json;

namespace GitTank.Models
{
    public class Repository
    {
        [JsonProperty("RepositoryName")]
        public string RepositoryName { get; set; }

        [JsonProperty("RepositoryPath")]
        public string RepositoryPath { get; set; }

        [JsonProperty("StatusForCheckBox")]
        public bool StatusForCheckBox { get; set; }
    }
}
