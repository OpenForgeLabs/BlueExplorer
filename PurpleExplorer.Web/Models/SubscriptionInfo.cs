namespace PurpleExplorer.Web.Models;

public class SubscriptionInfo
{
    public string Name { get; set; } = string.Empty;
    public long MessageCount { get; set; }
    public long DlqCount { get; set; }
}
