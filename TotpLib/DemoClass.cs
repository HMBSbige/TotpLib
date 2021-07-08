using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using TotpLib.Options;
using Volo.Abp.DependencyInjection;

namespace TotpLib
{
	[UsedImplicitly]
	public class DemoClass : ITransientDependency
	{
		private readonly DemoClassOptions _option;

		public DemoClass(IOptions<DemoClassOptions> options)
		{
			_option = options.Value;
		}

		public bool IsSuccess()
		{
			return _option.IsSuccess;
		}
	}
}
