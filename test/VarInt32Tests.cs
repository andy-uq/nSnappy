using System.Linq;
using NSnappy;
using NUnit.Framework;

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
			var varint = new VarInt32(-1);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xf }));

			varint = new VarInt32(int.MinValue);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x8 }));
		}

		[Test]
		public void SingleByteEncode()
		{
			var varints = Enumerable
				.Range(0, 128)
				.Select(x => new VarInt32(x))
				.Select(x => x.GetEncodedValue());

			Assert.That(varints, Has.All.Length.EqualTo(1));
			Assert.That(varints, Has.All.Exactly(1).LessThan(128));
		}

		[Test]
		public void DoubleByteEncode()
		{
			var varint = new VarInt32(128);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0x80, 0x01 }));

			varint = new VarInt32(0x3fff);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0xff, 0x7f }));
		}

		[Test]
		public void TrippleByteEncode()
		{
			var varint = new VarInt32(0x4000);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0x80, 0x80, 0x01 }));

			varint = new VarInt32(0x1fffff);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0xff, 0xff, 0x7f }));
		}

		[Test]
		public void QuadByteEncode()
		{
			var varint = new VarInt32(0x200000);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x01 }));

			varint = new VarInt32(0xfffffff);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0x7f }));
		}

		[Test]
		public void FullByteEncode()
		{
			var varint = new VarInt32(0x10000000);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x01 }));

			varint = new VarInt32(int.MaxValue);
			Assert.That(varint.GetEncodedValue(), Is.EqualTo(new byte[] { 0xff, 0xff, 0xff, 0xff, 0x7 }));
		}

		[Test]
		public void DoubleByteDecode()
		{
			var varint = new VarInt32(new byte[] { 0x80, 0x1 });
			Assert.That(varint.Value, Is.EqualTo(128));
		}
	}
}