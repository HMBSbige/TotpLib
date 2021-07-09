using System;

namespace TotpLib.HmacAlgorithmNames
{
	public interface IHmacAlgorithmName : IEquatable<HmacAlgorithmName>
	{
		string Name { get; }
	}
}
