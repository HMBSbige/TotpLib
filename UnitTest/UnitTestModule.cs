global using CryptoBase.DataFormatExtensions;
global using CryptoBase.Digests;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using System.Text;
global using TotpLib;
global using TotpLib.TimeServices;
global using TotpLib.TotpServices;
global using Volo.Abp.DependencyInjection;
global using Volo.Abp.Modularity;
global using Volo.Abp.Testing;

namespace UnitTest;

[DependsOn(typeof(TotpLibModule))]
[UsedImplicitly]
public class UnitTestModule : AbpModule
{
}
