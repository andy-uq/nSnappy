using System;

namespace NSpanny
{
	public static class IntEncoder
	{
		public static ReadOnlySpan<byte> Encode(int value, Span<byte> buffer)
		{
			const int moreData = 128;
			var       uvalue   = unchecked((uint) value);

			if (uvalue < 0x80)
			{
				buffer[0] = (byte) uvalue;
				return buffer.Slice(0, 1);
			}

			if (uvalue < 0x4000)
			{
				buffer[0] = (byte) (uvalue | moreData);
				buffer[1] = (byte) (uvalue >> 7);
				return buffer.Slice(0, 2);
			}

			if (uvalue < 0x200000)
			{
				buffer[0] = (byte) (uvalue | moreData);
				buffer[1] = (byte) ((uvalue >> 7) | moreData);
				buffer[2] = (byte) (uvalue >> 14);
				return buffer.Slice(0, 3);
			}

			if (uvalue < 0x10000000)
			{
				buffer[0] = (byte) (uvalue | moreData);
				buffer[1] = (byte) ((uvalue >> 7) | moreData);
				buffer[2] = (byte) ((uvalue >> 14) | moreData);
				buffer[3] = (byte) (uvalue >> 21);

				return buffer.Slice(0, 4);
			}

			buffer[0] = (byte) (uvalue | moreData);
			buffer[1] = (byte) ((uvalue >> 7) | moreData);
			buffer[2] = (byte) ((uvalue >> 14) | moreData);
			buffer[3] = (byte) ((uvalue >> 21) | moreData);
			buffer[4] = (byte) (uvalue >> 28);

			return buffer.Slice(0, 5);
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