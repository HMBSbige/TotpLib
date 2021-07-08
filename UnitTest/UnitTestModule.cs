using JetBrains.Annotations;
using LibTemplate;
using LibTemplate.Options;
using Volo.Abp.Modularity;

namespace UnitTest
{
	[DependsOn(typeof(LibTemplateModule))]
	[UsedImplicitly]
	public class UnitTestModule : AbpModule
	{
		public override void ConfigureServices(ServiceConfigurationContext context)
		{
			Configure<DemoClassOptions>(option => option.IsSuccess = true);
		}
	}
}
