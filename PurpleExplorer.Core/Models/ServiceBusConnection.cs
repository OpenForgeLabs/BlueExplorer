namespace PurpleExplorer.Core.Models;

public class ServiceBusConnection
{
    public ServiceBusConnection(string name, string connectionString, bool useManagedIdentity)
    {
        Name = name;
        ConnectionString = connectionString;
        UseManagedIdentity = useManagedIdentity;
    }

    public string Name { get; }
    public string ConnectionString { get; }
    public bool UseManagedIdentity { get; }
}
