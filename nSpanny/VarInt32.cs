using System;

namespace NSpanny
{
	public struct VarInt32
	{
		private const int MAX_ENCODED_BYTES = 5;

		public int Value { get; }

		public VarInt32(int value)
			: this()
		{
			Value = value;
		}

		public VarInt32(ReadOnlySpan<byte> data)
			: this()
		{
			Value = IntEncoder.Decode(data, MAX_ENCODED_BYTES);
		}

		public ReadOnlySpan<byte> GetEncodedValue(Span<byte> buffer)
		{
			return IntEncoder.Encode(Value, buffer);
		}
	}
}