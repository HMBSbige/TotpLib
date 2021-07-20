using System;
using System.Threading;
using System.Threading.Tasks;
using TotpLib.HmacAlgorithmNames;
using Volo.Abp.DependencyInjection;

namespace TotpLib.TotpServices
{
	public interface ITotpService : ITransientDependency
	{
		string? Secret { get; set; }

		string? Label { get; set; }

		string? Issuer { get; set; }

		uint Period { get; set; }

		HmacAlgorithmName Algorithm { get; set; }

		uint Digits { get; set; }

		TotpOutputType OutputType { get; set; }

		uint ExtraGap { get; set; }

		void GenerateNewSecret(uint minimumBits = 80);

		ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default);

		string GetToken(DateTime utcTime);

		string GetToken(long timestamp);

		string GetUri();

		bool TryParse(string? uri);

		ValueTask<bool> ValidateTokenAsync(string? token, CancellationToken cancellationToken = default);

		bool ValidateToken(string? token, DateTime utcTime);

		bool ValidateToken(string? token, long timestamp);
	}
}
