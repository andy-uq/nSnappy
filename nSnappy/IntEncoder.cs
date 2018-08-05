using System;

namespace NSnappy
{
	public static class IntEncoder
	{
		public static Span<byte> Encode(int value, ref Span<byte> buffer)
		{
			const int moreData = 128;
			var uvalue = unchecked((uint) value);

			if (uvalue < 0x80)
			{
				buffer[0] = (byte) uvalue;
				return buffer.Slice(0, 1);
			}

			if (uvalue < 0x4000)
			{
				buffer[offset] = (byte) (uvalue | moreData);
				buffer[offset + 1] = (byte) (uvalue >> 7);
				return new Span<byte>(buffer, offset, 2);
			}

			if (uvalue < 0x200000)
			{
				buffer[offset] = (byte) (uvalue | moreData);
				buffer[offset + 1] = (byte) ((uvalue >> 7) | moreData);
				buffer[offset + 2] = (byte) (uvalue >> 14);

				return new Span<byte>(buffer, offset, 3);
			}

			if (uvalue < 0x10000000)
			{
				return new[] {(byte) (uvalue | moreData), (byte) ((uvalue >> 7) | moreData), (byte) ((uvalue >> 14) | moreData), (byte) (uvalue >> 21)};
			}

			return new[] {(byte) (uvalue | moreData), (byte) ((uvalue >> 7) | moreData), (byte) ((uvalue >> 14) | moreData), (byte) ((uvalue >> 21) | moreData), (byte) (uvalue >> 28)};
		}

		public static int Decode(ReadOnlySpan<byte> data, int maxEncodedBytes)
		{
			var index = 0;
			var value = 0U;

			while (index < maxEncodedBytes)
			{
				var b = data[index];
				value |= (b & 0x7fU) << index*7;

				if (b < 0x80)
					break;

				index++;
			}

			return unchecked((int) value);
		}
	}
}