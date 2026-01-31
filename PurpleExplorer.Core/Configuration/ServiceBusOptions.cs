using System.Collections.Generic;

namespace PurpleExplorer.Core.Configuration;

public class ServiceBusOptions
{
    public List<ServiceBusConnectionConfig> Connections { get; set; } = [];
    public ServiceBusQuerySettings Query { get; set; } = new();
}

public class ServiceBusConnectionConfig
{
    public string Name { get; set; } = string.Empty;
    public bool UseManagedIdentity { get; set; }
    public string? ConnectionString { get; set; }
    public KeyVaultSecretConfig? KeyVault { get; set; }
}

public class KeyVaultSecretConfig
{
    public string VaultUri { get; set; } = string.Empty;
    public string SecretName { get; set; } = string.Empty;
    public string? SecretVersion { get; set; }
}

public class ServiceBusQuerySettings
{
    public int QueueListFetchCount { get; set; } = 100;
    public int QueueMessageFetchCount { get; set; } = 100;
    public int TopicListFetchCount { get; set; } = 100;
    public int TopicMessageFetchCount { get; set; } = 100;
}
