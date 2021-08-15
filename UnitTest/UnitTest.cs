using CryptoBase.DataFormatExtensions;
using CryptoBase.Digests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;
using TotpLib.TimeServices;
using TotpLib.TotpServices;

namespace UnitTest
{
	[TestClass]
	public class UnitTest : TestBase
	{
		[TestMethod]
		public async Task TimeServiceTest()
		{
			var time = GetRequiredService<DefaultTimeService>();
			var compare = await time.GetUtcNowAsync() - DateTimeOffset.UtcNow;
			Assert.IsTrue(compare.Duration() < TimeSpan.FromSeconds(1));
			compare = await time.GetUtcNowAsync() - DateTimeOffset.Now;
			Assert.IsTrue(compare.Duration() < TimeSpan.FromSeconds(1));
		}

		[TestMethod]
		[DataRow(0U)]
		[DataRow(1U)]
		[DataRow(5U)]
		[DataRow(8U)]
		[DataRow(9U)]
		[DataRow(19U)]
		[DataRow(80U)]
		[DataRow(160U)]
		[DataRow(128U)]
		[DataRow(256U)]
		public void GenerateNewSecretTest(uint bit)
		{
			var service = GetRequiredService<ITotpService>();
			service.GenerateNewSecret(bit);

			Assert.IsNotNull(service.Secret);
			Console.WriteLine(service.Secret);

			var actualLength = service.Secret.FromBase32String().Length;
			Assert.IsTrue((uint)actualLength << 3 >= bit);
		}

		/// <summary>
		/// https://datatracker.ietf.org/doc/html/rfc6238#appendix-B
		/// </summary>
		[TestMethod]
		[DataRow(@"12345678901234567890", @"1970-01-01 00:00:59", 59, @"94287082", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"1970-01-01 00:00:59", 59, @"46119246", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"1970-01-01 00:00:59", 59, @"90693936", @"SHA512")]
		[DataRow(@"12345678901234567890", @"2005-03-18 01:58:29", 1111111109, @"07081804", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"2005-03-18 01:58:29", 1111111109, @"68084774", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"2005-03-18 01:58:29", 1111111109, @"25091201", @"SHA512")]
		[DataRow(@"12345678901234567890", @"2005-03-18 01:58:31", 1111111111, @"14050471", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"2005-03-18 01:58:31", 1111111111, @"67062674", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"2005-03-18 01:58:31", 1111111111, @"99943326", @"SHA512")]
		[DataRow(@"12345678901234567890", @"2009-02-13 23:31:30", 1234567890, @"89005924", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"2009-02-13 23:31:30", 1234567890, @"91819424", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"2009-02-13 23:31:30", 1234567890, @"93441116", @"SHA512")]
		[DataRow(@"12345678901234567890", @"2033-05-18 03:33:20", 2000000000, @"69279037", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"2033-05-18 03:33:20", 2000000000, @"90698825", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"2033-05-18 03:33:20", 2000000000, @"38618901", @"SHA512")]
		[DataRow(@"12345678901234567890", @"2603-10-11 11:33:20", 20000000000, @"65353130", @"SHA1")]
		[DataRow(@"12345678901234567890123456789012", @"2603-10-11 11:33:20", 20000000000, @"77737706", @"SHA256")]
		[DataRow(@"1234567890123456789012345678901234567890123456789012345678901234", @"2603-10-11 11:33:20", 20000000000, @"47863826", @"SHA512")]
		public void GetTokenTest(string rawSecret, string utcTimeStr, long timestamp, string expected, string mode)
		{
			var service = GetRequiredService<ITotpService>();
			service.Secret = Encoding.ASCII.GetBytes(rawSecret).AsSpan().ToBase32String();
			service.Period = 30;
			service.Digits = (uint)expected.Length;
			Assert.IsTrue(Enum.TryParse(mode, true, out DigestType algorithm));
			service.Algorithm = algorithm;
			service.OutputType = TotpOutputType.RFC;

			Assert.AreEqual(expected, service.GetToken(timestamp));
			Assert.AreEqual(expected, service.GetToken(DateTimeOffset.FromUnixTimeSeconds(timestamp)));
			Assert.AreEqual(expected, service.GetToken(new DateTimeOffset(DateTime.Parse(utcTimeStr), TimeSpan.Zero)));
		}

		[TestMethod]
		[DataRow(59, @"PV9M4")]
		[DataRow(1111111109, @"PY4YB")]
		[DataRow(1111111111, @"5PP3V")]
		[DataRow(1234567890, @"VHHQY")]
		[DataRow(2000000000, @"9N776")]
		[DataRow(20000000000, @"R5DMB")]
		public void GetSteamTokenTest(long timestamp, string expected)
		{
			var service = GetRequiredService<ITotpService>();
			service.Secret = Encoding.ASCII.GetBytes(@"12345678901234567890").AsSpan().ToBase32String();
			service.Period = 30;
			service.Algorithm = DigestType.Sha1;
			service.Digits = 5;
			service.OutputType = TotpOutputType.Steam;

			Assert.AreEqual(expected, service.GetToken(timestamp));
		}

		[TestMethod]
		public void GetUriTest()
		{
			var service = GetRequiredService<ITotpService>();
			Assert.ThrowsException<ArgumentException>(() => service.GetUri());

			service.Secret = @"JTQZIMD5U2PXT5MJ";
			Assert.AreEqual(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ", service.GetUri());

			service.Issuer = @"HMBSbige";
			Assert.AreEqual(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige", service.GetUri());

			service.Period = 114514;
			Assert.AreEqual(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514", service.GetUri());

			service.Algorithm = DigestType.Sha256;
			Assert.AreEqual(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514&algorithm=SHA256", service.GetUri());

			service.Digits = 8;
			Assert.AreEqual(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514&algorithm=SHA256&digits=8", service.GetUri());

			service.Label = @"Google 验证器";
			service.Issuer = @"Bruce Wayne";
			Assert.AreEqual(@"otpauth://totp/Google%20%E9%AA%8C%E8%AF%81%E5%99%A8?secret=JTQZIMD5U2PXT5MJ&issuer=Bruce+Wayne&period=114514&algorithm=SHA256&digits=8", service.GetUri());
		}

		[TestMethod]
		public void TryParseTest()
		{
			var service = GetRequiredService<ITotpService>();

			Assert.IsTrue(service.TryParse(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(string.Empty, service.Label);
			Assert.AreEqual(null, service.Issuer);
			Assert.AreEqual(30U, service.Period);
			Assert.AreEqual(DigestType.Sha1, service.Algorithm);
			Assert.AreEqual(6U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);

			Assert.IsTrue(service.TryParse(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(string.Empty, service.Label);
			Assert.AreEqual(@"HMBSbige", service.Issuer);
			Assert.AreEqual(30U, service.Period);
			Assert.AreEqual(DigestType.Sha1, service.Algorithm);
			Assert.AreEqual(6U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);

			Assert.IsTrue(service.TryParse(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(string.Empty, service.Label);
			Assert.AreEqual(@"HMBSbige", service.Issuer);
			Assert.AreEqual(114514U, service.Period);
			Assert.AreEqual(DigestType.Sha1, service.Algorithm);
			Assert.AreEqual(6U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);

			Assert.IsTrue(service.TryParse(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514&algorithm=SHA256"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(string.Empty, service.Label);
			Assert.AreEqual(@"HMBSbige", service.Issuer);
			Assert.AreEqual(114514U, service.Period);
			Assert.AreEqual(DigestType.Sha256, service.Algorithm);
			Assert.AreEqual(6U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);

			Assert.IsTrue(service.TryParse(@"otpauth://totp/?secret=JTQZIMD5U2PXT5MJ&issuer=HMBSbige&period=114514&algorithm=SHA256&digits=8"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(string.Empty, service.Label);
			Assert.AreEqual(@"HMBSbige", service.Issuer);
			Assert.AreEqual(114514U, service.Period);
			Assert.AreEqual(DigestType.Sha256, service.Algorithm);
			Assert.AreEqual(8U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);

			Assert.IsTrue(service.TryParse(@"otpauth://totp/Google%20%E9%AA%8C%E8%AF%81%E5%99%A8?secret=JTQZIMD5U2PXT5MJ&issuer=Bruce+Wayne&period=114514&algorithm=SHA256&digits=8"));
			Assert.AreEqual(@"JTQZIMD5U2PXT5MJ", service.Secret);
			Assert.AreEqual(@"Google 验证器", service.Label);
			Assert.AreEqual(@"Bruce Wayne", service.Issuer);
			Assert.AreEqual(114514U, service.Period);
			Assert.AreEqual(DigestType.Sha256, service.Algorithm);
			Assert.AreEqual(8U, service.Digits);
			Assert.AreEqual(TotpOutputType.RFC, service.OutputType);
		}

		[TestMethod]
		[DataRow(@"12345678901234567890", 2000000000, @"69279037", true)]
		[DataRow(@"12345678901234567890", 2000000000, @"26940678", true)]
		[DataRow(@"12345678901234567890", 2000000000, @"40196847", false)]
		[DataRow(@"12345678901234567890", 2000000000, @"91637009", false)]
		public void ValidateTokenTest(string rawSecret, long timestamp, string token, bool shouldAccept)
		{
			var service = GetRequiredService<ITotpService>();
			service.Secret = Encoding.ASCII.GetBytes(rawSecret).AsSpan().ToBase32String();
			service.Period = 30;
			service.Digits = (uint)token.Length;
			service.Algorithm = DigestType.Sha1;
			service.ExtraGap = 1;
			service.OutputType = TotpOutputType.RFC;

			Console.WriteLine(service.GetToken(timestamp));
			Console.WriteLine(service.GetToken(timestamp + service.Period));

			Assert.AreEqual(shouldAccept, service.ValidateToken(token, timestamp));
			Assert.AreEqual(shouldAccept, service.ValidateToken(token, DateTimeOffset.FromUnixTimeSeconds(timestamp)));
		}
	}
}
