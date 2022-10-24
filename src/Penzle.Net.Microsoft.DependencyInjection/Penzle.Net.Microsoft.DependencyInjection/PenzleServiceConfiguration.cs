namespace Penzle.Net.Microsoft.DependencyInjection;

public sealed class PenzleServiceConfiguration
{
    public Uri BaseUri { get; set; }
    public string ApiDeliveryKey { get; set; }
    public string ApiManagementKey { get; set; }
    public TimeSpan TimeOut { get; set; }
    public string Project { get; set; }
    public string Environment { get; set; }
}