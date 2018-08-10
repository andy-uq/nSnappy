using NSpanny;
using NUnit.Framework;

namespace test
{
	[TestFixture]
	public class PointerTests
	{
		private byte[] _buffer;

		[SetUp]
		public void BufferA()
		{
			_buffer = new byte[16];
		}

		[Test]
		public void SubtractPointers()
		{
			var a = new Pointer(_buffer);
			var b = new Pointer(_buffer, 8);

			Assert.That((int )(b - a), Is.EqualTo(8));
		}

		[Test]
		public void AddPointers()
		{
			var a = new Pointer(_buffer);
			var b = new Pointer(_buffer, 8);

			Assert.That(b == a+8, Is.True);
		}

		[Test]
		public void PointerEquality()
		{
			var a = new Pointer(_buffer);
			var b = new Pointer(_buffer, 8);

			Assert.That(b == (a + 8), Is.True);
			Assert.That((a + 8) == b, Is.True);
			Assert.That(a != b, Is.True);
		}

		[Test]
		public void TestToString()
		{
			var a = new Pointer(_buffer, name:"a");
			var b = new Pointer(_buffer, 8, name:"b");
			var anon = new Pointer(_buffer);

			Assert.That(a.ToString(), Is.EqualTo("a[16]"));
			Assert.That(b.ToString(), Is.EqualTo("b[16]+8"));
			Assert.That(anon.ToString(), Is.EqualTo("<???>[16]"));
		}
	}
}