using System.Collections.Generic;
using GitTank.Models;
using Microsoft.Extensions.Configuration;

namespace GitTank.Configuration
{
    public class Settings : ISettings
    {
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsNewUI => _configuration.GetValue<bool>("appSettings:newUI");
        public string DefaultRepository => _configuration.GetValue<string>("appSettings:defaultRepository");
        public string DefaultBranch => _configuration.GetValue<string>("appSettings:defaultBranch");

        public List<Sources> Sources => _configuration
            .GetSection("appSettings")
            .GetSection("sources")
            .Get<List<Sources>>();
    }
}
