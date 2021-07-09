using JetBrains.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TotpLib.TimeServices
{
	[UsedImplicitly]
	public class DefaultTimeService : ITimeService
	{
		public ValueTask<DateTime> GetUtcNowAsync(CancellationToken cancellationToken = default)
		{
			return ValueTask.FromResult(DateTime.UtcNow);
		}
	}
}
