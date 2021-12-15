namespace UnitTest;

public class TestBase : AbpIntegratedTest<UnitTestModule>
{
	protected IAbpLazyServiceProvider LazyServiceProvider { get; }

	protected TestBase()
	{
		LazyServiceProvider = Application.ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>();
	}
}
