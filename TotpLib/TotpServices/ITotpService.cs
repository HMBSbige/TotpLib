namespace TotpLib.TotpServices;

public interface ITotpService : ITransientDependency
{
	string? Secret { get; set; }

	string? Label { get; set; }

	string? Issuer { get; set; }

	uint Period { get; set; }

	DigestType Algorithm { get; set; }

	uint Digits { get; set; }

	TotpOutputType OutputType { get; set; }

	uint ExtraGap { get; set; }

	void GenerateNewSecret(uint minimumBits = 80);

	ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default);

	string GetToken(DateTimeOffset utcTime);

	string GetToken(long timestamp);

	string GetUri();

	bool TryParse(string? uri);

	ValueTask<bool> ValidateTokenAsync(string? token, CancellationToken cancellationToken = default);

	bool ValidateToken(string? token, DateTimeOffset utcTime);

	bool ValidateToken(string? token, long timestamp);
}
