using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TotpLib.TimeServices
{
	public interface ITimeService : ITransientDependency
	{
		ValueTask<DateTime> GetUtcNowAsync(CancellationToken cancellationToken = default);
	}
}
