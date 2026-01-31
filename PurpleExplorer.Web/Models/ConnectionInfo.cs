namespace PurpleExplorer.Web.Models;

public class ConnectionInfo
{
    public string Name { get; set; } = string.Empty;
    public bool UseManagedIdentity { get; set; }
    public bool IsEditable { get; set; }
    public string Source { get; set; } = string.Empty;
}
