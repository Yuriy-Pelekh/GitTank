using Newtonsoft.Json;

namespace GitTank.Models
{
    public class Configuration
    {
        [JsonProperty("AppSettings")]
        public AppSettings AppSettings { get; set; }
    }
}
