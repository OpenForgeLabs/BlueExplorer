namespace PurpleExplorer.Core.Models;

public class ConnectionInfo
{
    public ConnectionInfo(string name, bool useManagedIdentity)
    {
        Name = name;
        UseManagedIdentity = useManagedIdentity;
    }

    public string Name { get; }
    public bool UseManagedIdentity { get; }
}
