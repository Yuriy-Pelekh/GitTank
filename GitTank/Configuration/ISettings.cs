using System.Collections.Generic;
using GitTank.Models;

namespace GitTank.Configuration
{
    public interface ISettings
    {
        bool IsNewUI { get; }
        string DefaultRepository { get; }
        string DefaultBranch { get; }
        List<Sources> Sources { get; }
    }
}
