namespace TotpLib.TimeServices;

public interface ITimeService : ITransientDependency
{
	ValueTask<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken = default);
}
