namespace TotpLib.HmacAlgorithmNames
{
	public readonly struct HmacAlgorithmName : IHmacAlgorithmName
	{
		private readonly string? _name;
		public string Name => _name ?? string.Empty;

		public HmacAlgorithmName(string? name)
		{
			_name = name;
		}

		public override string ToString()
		{
			return Name;
		}

		public bool Equals(HmacAlgorithmName other)
		{
			return _name == other._name;
		}

		public override bool Equals(object? obj)
		{
			return obj is HmacAlgorithmName other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _name is not null ? _name.GetHashCode() : default;
		}

		public static HmacAlgorithmName MD5 => new(@"HMACMD5");
		public static HmacAlgorithmName SHA1 => new(@"HMACSHA1");
		public static HmacAlgorithmName SHA256 => new(@"HMACSHA256");
		public static HmacAlgorithmName SHA384 => new(@"HMACSHA384");
		public static HmacAlgorithmName SHA512 => new(@"HMACSHA512");
	}
}
