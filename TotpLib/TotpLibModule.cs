global using CryptoBase.DataFormatExtensions;
global using CryptoBase.Digests;
global using CryptoBase.Macs.Hmac;
global using JetBrains.Annotations;
global using System.Buffers;
global using System.Buffers.Binary;
global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.InteropServices;
global using System.Security.Cryptography;
global using System.Web;
global using TotpLib.TimeServices;
global using Volo.Abp.DependencyInjection;
global using Volo.Abp.Modularity;

namespace TotpLib;

public class TotpLibModule : AbpModule
{
}
