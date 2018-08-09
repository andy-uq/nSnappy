using System;
using System.Linq;
using NSpanny;
using NUnit.Framework;
using Shouldly;

namespace test
{
	[TestFixture]
	public class VarInt32Tests
	{
		[Test]
		public void SingleByteDecode()
		{
			var varints = Enumerable
				.Range(0, 128)
				.Select(x => new[] { (byte)x })
				.Select(x => new VarInt32(x))
				.ToArray();

			Assert.That(varints.Select(x => x.Value).ToArray(), Is.EquivalentTo(Enumerable.Range(0, 128).ToArray()));
		}

		[Test]
		public void NegativeEncode()
		{
			var buffer = new byte[5];

			new VarInt32(-1).GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f }));

			new VarInt32(int.MinValue).GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 }));
		}

		[Test]
		public void SingleByteEncode()
		{
			var buffer = new byte[128 * 5];
			var position = 0;

			ReadOnlySpan<byte> encode(VarInt32 i)
			{
				var encoded = i.GetEncodedValue(new Span<byte>(buffer, position, 5));
				position += encoded.Length;
				return encoded;
			}

			var varints = Enumerable
				.Range(0, 128)
				.Select(x => new VarInt32(x));

			foreach (var i in varints)
			{
				var encoded = encode(i);
				encoded.Length.ShouldBe(1);
			}
		}

		[Test]
		public void DoubleByteEncode()
		{
			byte[] buffer = new byte[2];

			var varint = new VarInt32(128);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0x80, 0x01 }));

			varint = new VarInt32(0x3fff);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0xff, 0x7f }));
		}

		[Test]
		public void TripleByteEncode()
		{
			var buffer = new byte[3];

			var varint = new VarInt32(0x4000);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0x80, 0x80, 0x01 }));

			varint = new VarInt32(0x1fffff);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0xff, 0xff, 0x7f }));
		}

		[Test]
		public void QuadByteEncode()
		{
			byte[] buffer = new byte[4];

			var varint = new VarInt32(0x200000);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x01 }));

			varint = new VarInt32(0xfffffff);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0x7f }));
		}

		[Test]
		public void FullByteEncode()
		{
			var buffer = new byte[5];

			var varint = new VarInt32(0x10000000);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x01 }));

			varint = new VarInt32(int.MaxValue);
			varint.GetEncodedValue(buffer);
			Assert.That(buffer, Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0xff, 0x7 }));
		}

		[Test]
		public void DoubleByteDecode()
		{
			var varint = new VarInt32(new byte[] { 0x80, 0x1 });
			Assert.That(varint.Value, Is.EqualTo(128));
		}
	}
}