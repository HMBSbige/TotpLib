using JetBrains.Annotations;
using TotpLib;
using Volo.Abp.Modularity;

namespace UnitTest
{
	[DependsOn(typeof(TotpLibModule))]
	[UsedImplicitly]
	public class UnitTestModule : AbpModule
	{
	}
}
