namespace TotpLib.TimeServices;

[UsedImplicitly]
public class DefaultTimeService : ITimeService
{
	public ValueTask<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken = default)
	{
		return ValueTask.FromResult(DateTimeOffset.UtcNow);
	}
}
