using System.IO;

namespace NSpanny
{
	[System.Diagnostics.DebuggerDisplay("Value: {" + nameof(DebugString) + "}")]
	public class Writer
	{
		private readonly Stream _output;
		private byte[] _buffer;
		private int _index;

		public Writer(Stream output)
		{
			_output = output;
		}

		private string DebugString => System.Text.Encoding.ASCII.GetString(_buffer, 0, _index);

		public void SetExpectedLength(uint len)
		{
			_buffer = new byte[len];
			_index = 0;
		}

		public bool Append(Pointer ip, int len)
		{
			int spaceLeft = _buffer.Length - _index;
			if ( spaceLeft < len )
			{
				return false;
			}

			var op = new Pointer(_buffer, _index);
			op.Copy(ip, len);
			_index += len;
			return true;
		}

		public bool TryFastAppend(Pointer ip, int available, int len)
		{
			int spaceLeft = _buffer.Length - _index;

			if (len > 16 || available < 16 || spaceLeft < 16)
			{
				return false;
			}

			var op = new Pointer(_buffer, _index).ToUInt64();
			var source = ip.ToUInt64();
			op[0] = source[0];
			op[1] = source[1];

			_index += len;
			return true;
		}

		public bool AppendFromSelf(int offset, int len)
		{
			int spaceLeft = _buffer.Length - _index;

			if ( _index <= offset - 1u )
			{  // -1u catches offset==0
				return false;
			}

			var op = new Pointer(_buffer, _index);
			if ( len <= 16 && offset >= 8 && spaceLeft >= 16 )
			{
				var src = new ReadOnlyPointer(_buffer, _index - offset).ToUInt64();
				op.WriteUInt64(src[0]);
				op.WriteUInt64(src[1]);
			}
			else
			{
				if ( spaceLeft >= len + CompressorConstants.MaxIncrementCopyOverflow )
				{
					IncrementalCopyFastPath(op - offset, op, len);
				}
				else
				{
					if ( spaceLeft < len )
					{
						return false;
					}

					IncrementalCopy(op - offset, op, len);
				}
			}

			_index += len;
			return true;
		}

		private void IncrementalCopy(Pointer src, Pointer op, int len)
		{
			do
			{
				op[0] = src[0];

				op += 1;
				src += 1;
			} while (--len > 0);
		}

		private void IncrementalCopyFastPath(ReadOnlyPointer src, Pointer op, int len)
		{
			while ( op - src < 8 )
			{
				op.WriteUInt64(src);
				len -= op - src;
				op += op - src;
			}

			while ( len > 0 )
			{
				op.WriteUInt64(src);
				src += 8;
				op += 8;
				len -= 8;
			}
		}

		public void Flush()
		{
			_output.Write(_buffer, 0, _index);
		}
	}
}