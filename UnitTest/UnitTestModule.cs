using JetBrains.Annotations;
using TotpLib;
using TotpLib.Options;
using Volo.Abp.Modularity;

namespace UnitTest
{
	[DependsOn(typeof(TotpLibModule))]
	[UsedImplicitly]
	public class UnitTestModule : AbpModule
	{
		public override void ConfigureServices(ServiceConfigurationContext context)
		{
			Configure<DemoClassOptions>(option => option.IsSuccess = true);
		}
	}
}
