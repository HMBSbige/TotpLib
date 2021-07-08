using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Testing;

namespace UnitTest
{
	public class TestBase : AbpIntegratedTest<UnitTestModule>
	{
		protected IAbpLazyServiceProvider LazyServiceProvider { get; }

		protected TestBase()
		{
			LazyServiceProvider = Application.ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>();
		}
	}
}
