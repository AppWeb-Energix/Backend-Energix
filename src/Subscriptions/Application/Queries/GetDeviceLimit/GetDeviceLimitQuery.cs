namespace Energix.Subscriptions.Application.Queries.GetDeviceLimit;

public class GetDeviceLimitQuery
{
    public Guid UserId { get; set; }
    public int CurrentDeviceCount { get; set; }
}
