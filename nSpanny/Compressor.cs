using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NSpanny
{
	public class Compressor
	{
		public int Compress(Stream input, Stream output)
		{
			var length = (int) input.Length;

			int bytesToRead = Math.Min(length, CompressorConstants.BlockSize);
			int maxOutput = MaxCompressedOutput(bytesToRead);

			var fragment = ArrayPool<byte>.Shared.Rent(bytesToRead);
			var block = ArrayPool<byte>.Shared.Rent(maxOutput);
			try
			{
				var varInt = new VarInt32(length).GetEncodedValue(block);
				output.Write(block, 0, varInt.Length);

				int bytesWritten = varInt.Length;

				while (length > 0)
				{
					var fragmentSize = input.Read(fragment, 0, bytesToRead);
					var hashTable    = new HashTable(fragmentSize);

					int blockSize = CompressFragment(fragment.AsSpan(0, fragmentSize), hashTable, block);
					output.Write(block, 0, blockSize);
					bytesWritten += blockSize;

					length -= bytesToRead;
				}

				return bytesWritten;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(fragment);
				ArrayPool<byte>.Shared.Return(block);
			}
		}

		private int CompressFragment(ReadOnlySpan<byte> source, HashTable hashTable, byte[] scratchOutput)
		{
			const int inputMarginBytes = 15;

			int shift = 32 - Log2Floor(hashTable.Size);
			var op = new Pointer(scratchOutput);
			var ip = new ReadOnlyPointer(source);
			var nextEmit = new ReadOnlyPointer(source);
			var baseIp = new ReadOnlyPointer(ip);

			uint hash_ptr(ReadOnlyPointer value, int offset) => ((uint) (value + offset) * 0x1e35a7bd) >> shift;

			if (source.Length >= inputMarginBytes)
			{
				var ipLimit = source.Length - inputMarginBytes;

				ip = ip + 1;
				var nextHash = hash_ptr(ip, 0);
				while (true)
				{
					uint skip = 32;

					var nextIp = new ReadOnlyPointer(ip);
					ReadOnlyPointer candidate;
					do
					{
						ip = nextIp;
						uint hash = nextHash;
						Assert(hash == hash_ptr(ip, 0));

						uint bytesBetweenHashLookups = skip++ >> 5;
						nextIp = ip + bytesBetweenHashLookups;

						if ( nextIp > ipLimit )
						{
							goto emit_remainder;
						}

						nextHash = hash_ptr(nextIp, 0);
						candidate = baseIp + hashTable[hash];
						
						Assert(candidate >= baseIp);
						Assert(candidate < ip);

						hashTable[hash] = ip - baseIp;

					} while ((uint) ip != (uint) candidate);

					Assert(nextEmit + 16 <= source.Length);

					op = EmitLiteral(op, nextEmit, ip - nextEmit, allowFastPath: true);

					ReadOnlyPointer inputBytes;
					uint candidateBytes;

					do
					{
						var b       = ip;
						int matched = 4 + FindMatchLength(candidate + 4, ip + 4, source.Length);
						ip += matched;
						var offset = b - candidate;

						op = EmitCopy(op, offset, matched);

						var insertTail = ip - 1;
						nextEmit = ip;

						if (ip >= ipLimit)
						{
							goto emit_remainder;
						}

						inputBytes = insertTail;

						var prevHash = hash_ptr(inputBytes, 0);
						hashTable[prevHash] = ip - baseIp - 1;

						var curHash = hash_ptr(inputBytes, 1);
						candidate = baseIp + hashTable[curHash];

						candidateBytes     = (uint) candidate;
						hashTable[curHash] = ip - baseIp;

					} while ((uint) (inputBytes + 1) == candidateBytes);

					nextHash = hash_ptr(inputBytes, 2);
					ip = ip + 1;
				}
			}

			emit_remainder:
			if (nextEmit < source.Length)
			{
				op = EmitLiteral(op, nextEmit, source.Length - nextEmit, false);
			}

			return op;
		}

		private int FindMatchLength(ReadOnlyPointer p1, ReadOnlyPointer p2, int length)
		{
			int matched = 0;
			while ( p2 <= length - 4 && (uint) p2 == (uint) (p1 + matched) )
			{
				p2 += 4;
				matched += 4;
			}

			if ( p2 <= length - 4 )
			{
				var x = (uint) p2 ^ (uint) (p1 + matched);
				var matchingBits = FindLSBSetNonZero(x);
				matched += matchingBits >> 3;
			}
			else
			{
				while ((p2 < length) && (p1[matched] == p2[0]))
				{
					p2 += 1;
					++matched;
				}
			}

			return matched;
		}

		private int FindLSBSetNonZero(uint number)
		{
			int bit = 31;
			
			for ( int i = 4, shift = 1 << 4; i >= 0; --i )
			{
				var x = number << shift;
				if ( x != 0 )
				{
					number = x;
					bit -= shift;
				}

				shift >>= 1;
			}

			return bit;
		}

		private Pointer EmitCopy(Pointer op, int offset, int len)
		{
			// Emit 64 byte copies but make sure to keep at least four bytes reserved
			while ( len >= 68 )
			{
				op = EmitCopyLessThan64(op, offset, 64);
				len -= 64;
			}

			// Emit an extra 60 byte copy if have too much data to fit in one copy
			if ( len > 64 )
			{
				op = EmitCopyLessThan64(op, offset, 60);
				len -= 60;
			}

			// Emit remainder
			op = EmitCopyLessThan64(op, offset, len);
			return op;
		}

		private Pointer EmitCopyLessThan64(Pointer op, int offset, int len)
		{
			Assert(len <= 64);
			Assert(len >= 4);
			Assert(offset < 65536);

			if ((len < 12) && (offset < 2048))
			{
				int lenMinus4 = len - 4;
				Assert(lenMinus4 < 8); // Must fit in 3 bits

				op[0] = (byte) (CompressorTag.Copy1ByteOffset | ((lenMinus4) << 2) | ((offset >> 8) << 5));
				op[1] = (byte) (offset & 0xff);
				op = op + 2;
			}
			else
			{
				op[0] = (byte) (CompressorTag.Copy2ByteOffset | ((len - 1) << 2));
				op += 1;

				((Span<ushort>) op)[0] = (ushort) offset;
				op += 2;
			}

			return op;
		}

		private Pointer EmitLiteral(Pointer dest, ReadOnlyPointer literal, int length, bool allowFastPath)
		{
			int n = length - 1;
			if (n<60)
			{
				var value = CompressorTag.Literal | (n << 2);
				dest[0] = (byte)value;
				dest += 1;

				if ( allowFastPath && length <= 16 )
				{
					var uint64s = (Span<ulong>) dest;
					uint64s[0] = (ulong) literal;
					uint64s[1] = (ulong) (literal + 8);

					return dest + length;
				}
			}
			else
			{
				var tmp = new Pointer(dest);
				dest += 1;
				
				int count = 0;
				while ( n > 0 )
				{
					dest[count] = (byte) (n & 0xff);
					n >>= 8;
					count++;
				}
				
				Assert(count >= 1);
				Assert(count <= 4);

				tmp[0] = (byte)(CompressorTag.Literal | (59 + count) << 2);
				dest += count;
			}

			dest.Copy(literal, length);
			return dest + length;
		}

		[Conditional("DEBUG")]
		[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
		private static void Assert(bool condition)
		{
			if (!condition)
				throw new ApplicationException("Assertion failed");
		}

		private int Log2Floor(uint n)
		{
			if ( n == 0 )
				return -1;
			
			int log = 0;
			uint value = n;

			for ( int i = 4; i >= 0; --i )
			{
				int shift = (1 << i);
				uint x = value >> shift;
				if ( x != 0 )
				{
					value = x;
					log += shift;
				}
			}
			
			Assert(value == 1);
			return log;
		}

		private int MaxCompressedOutput(int size)
		{
			return 32 + size + size / 6;
		}
	}
}