using CryptoBase.DataFormatExtensions;
using CryptoBase.Digests;
using CryptoBase.Macs.Hmac;
using JetBrains.Annotations;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TotpLib.TimeServices;
using Volo.Abp.DependencyInjection;

namespace TotpLib.TotpServices
{
	/// <summary>
	/// https://datatracker.ietf.org/doc/html/rfc6238
	/// </summary>
	[UsedImplicitly]
	public class TotpService : ITotpService
	{
		#region Default

		private const uint DefaultPeriod = 30;
		private const uint DefaultDigits = 6;
		private const DigestType DefaultAlgorithm = DigestType.Sha1;

		#endregion

		public string? Secret { get; set; }

		public string? Label { get; set; }

		public string? Issuer { get; set; }

		public uint Period { get; set; } = DefaultPeriod;

		public DigestType Algorithm { get; set; } = DefaultAlgorithm;

		public uint Digits { get; set; } = 6;

		public TotpOutputType OutputType { get; set; } = TotpOutputType.RFC;

		public uint ExtraGap { get; set; } = 1;

		private readonly IAbpLazyServiceProvider _lazyServiceProvider;

		private ITimeService TimeService => _lazyServiceProvider.LazyGetRequiredService<ITimeService>();

		public TotpService(IAbpLazyServiceProvider lazyServiceProvider)
		{
			_lazyServiceProvider = lazyServiceProvider;
		}

		public void GenerateNewSecret(uint minimumBits = 80)
		{
			var length = (int)Math.Ceiling((double)minimumBits / 8);
			var buffer = ArrayPool<byte>.Shared.Rent(length);
			try
			{
				var span = buffer.AsSpan(0, length);
				RandomNumberGenerator.Fill(span);
				Secret = span.ToBase32String();
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default)
		{
			var time = await TimeService.GetUtcNowAsync(cancellationToken);
			return GetToken(time);
		}

		public string GetToken(DateTimeOffset utcTime)
		{
			return GetToken(utcTime.ToUnixTimeSeconds());
		}

		public string GetToken(long timestamp)
		{
			var key = Secret.AsSpan().FromBase32String();
			using var hmac = HmacUtils.Create(Algorithm, key);

			var ts = (ulong)(timestamp / Period);

			Span<byte> temp = stackalloc byte[sizeof(ulong)];
			BinaryPrimitives.WriteUInt64BigEndian(temp, ts);
			hmac.Update(temp);

			Span<byte> hash = stackalloc byte[hmac.Length];
			hmac.GetMac(hash);

			var offset = hash[^1] & 0xF;
			var i = BinaryPrimitives.ReadUInt32BigEndian(hash[offset..]) & 0x7FFFFFFF;

			var length = (int)Digits;

			if (length <= 0)
			{
				return GetSteamToken(i, 5);
			}

			if (OutputType is TotpOutputType.Steam)
			{
				return GetSteamToken(i, length);
			}

			return string.Create(length, i, (chars, token) =>
			{
				for (var j = chars.Length - 1; j >= 0; j--)
				{
					if (token <= 0)
					{
						chars[j] = '0';
						continue;
					}

					chars[j] = (char)(token % 10 + '0');
					token /= 10;
				}
			});
		}

		public string GetUri()
		{
			CheckSecret();

			var builder = new UriBuilder(@"otpauth", @"totp", -1, Label);

			var query = HttpUtility.ParseQueryString(builder.Query);

			query[@"secret"] = Secret;

			if (!string.IsNullOrEmpty(Issuer))
			{
				query[@"issuer"] = Issuer;
			}

			if (Period is not DefaultPeriod)
			{
				query[@"period"] = Period.ToString();
			}

			if (!Algorithm.Equals(DefaultAlgorithm))
			{
				query[@"algorithm"] = MapToString(Algorithm);
			}

			if (Digits is not DefaultDigits)
			{
				query[@"digits"] = Digits.ToString();
			}

			builder.Query = query.ToString();

			return builder.ToString();
		}

		public bool TryParse(string? uri)
		{
			if (string.IsNullOrEmpty(uri) || !uri.StartsWith(@"otpauth://totp/"))
			{
				return false;
			}

			var builder = new UriBuilder(uri);
			var query = HttpUtility.ParseQueryString(builder.Query);

			var secret = query.Get(@"secret");
			if (!IsValidSecret(secret))
			{
				return false;
			}

			var label = Uri.UnescapeDataString(builder.Path.TrimStart('/'));

			var issuer = query.Get(@"issuer");

			var periodStr = query.Get(@"period");
			var period = DefaultPeriod;
			if (!string.IsNullOrEmpty(periodStr) && !uint.TryParse(periodStr, out period))
			{
				return false;
			}

			var algorithmStr = query.Get(@"algorithm");
			var algorithm = DefaultAlgorithm;
			if (!string.IsNullOrEmpty(algorithmStr))
			{
				algorithm = MapFromString(algorithmStr);
				if (!Enum.IsDefined(algorithm))
				{
					return false;
				}
			}

			var digitsStr = query.Get(@"digits");
			var digits = DefaultDigits;
			if (!string.IsNullOrEmpty(digitsStr) && !uint.TryParse(digitsStr, out digits))
			{
				return false;
			}

			// Success
			Secret = secret;
			Label = label;
			Issuer = issuer;
			Period = period;
			Algorithm = algorithm;
			Digits = digits;

			return true;
		}

		public async ValueTask<bool> ValidateTokenAsync(string? token, CancellationToken cancellationToken = default)
		{
			var time = await TimeService.GetUtcNowAsync(cancellationToken);
			return ValidateToken(token, time);
		}

		public bool ValidateToken(string? token, DateTimeOffset utcTime)
		{
			return ValidateToken(token, utcTime.ToUnixTimeSeconds());
		}

		public bool ValidateToken(string? token, long timestamp)
		{
			CheckSecret();

			if (string.IsNullOrEmpty(token))
			{
				return false;
			}

			var result = false;

			for (var i = 0u; i <= ExtraGap; ++i)
			{
				var expectedToken = GetToken(timestamp - i * Period);
				if (IsSameToken(expectedToken, token))
				{
					result = true;
				}
			}

			return result;
		}

		private static bool IsValidSecret([NotNullWhen(true)] string? secret)
		{
			try
			{
				var b = secret.AsSpan().FromBase32String();
				if (b.Length <= 0)
				{
					return false;
				}
			}
			catch (FormatException)
			{
				return false;
			}

			return true;
		}

		[MemberNotNull(nameof(Secret))]
		private void CheckSecret()
		{
			if (!IsValidSecret(Secret))
			{
				throw new ArgumentException(@"A valid secret must be set first!");
			}
		}

		private static string GetSteamToken(uint i, int length)
		{
			const string steamAuthTable = @"23456789BCDFGHJKMNPQRTVWXY";

			return string.Create(length, i, (chars, token) =>
			{
				for (var j = 0; j < chars.Length; ++j)
				{
					chars[j] = steamAuthTable[(int)(token % steamAuthTable.Length)];
					token /= (uint)steamAuthTable.Length;
				}
			});
		}

		private static bool IsSameToken(string expected, string input)
		{
			return CryptographicOperations.FixedTimeEquals(
				MemoryMarshal.AsBytes(expected.AsSpan()),
				MemoryMarshal.AsBytes(input.AsSpan())
			);
		}

		private static string MapToString(DigestType type)
		{
			return type switch
			{
				DigestType.Sm3 => @"SM3",
				DigestType.Md5 => @"MD5",
				DigestType.Sha1 => @"SHA1",
				DigestType.Sha256 => @"SHA256",
				DigestType.Sha384 => @"SHA384",
				DigestType.Sha512 => @"SHA512",
				_ => @"SHA1"
			};
		}

		private static DigestType MapFromString(string str)
		{
			if (string.Equals(str, @"SM3", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Sm3;
			}
			if (string.Equals(str, @"MD5", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Md5;
			}
			if (string.Equals(str, @"SHA1", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Sha1;
			}
			if (string.Equals(str, @"SHA256", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Sha256;
			}
			if (string.Equals(str, @"SHA384", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Sha384;
			}
			if (string.Equals(str, @"SHA512", StringComparison.OrdinalIgnoreCase))
			{
				return DigestType.Sha512;
			}
			return (DigestType)(-1);
		}
	}
}
