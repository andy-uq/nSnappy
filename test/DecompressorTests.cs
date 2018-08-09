using System;
using System.IO;
using System.Linq;
using NSpanny;
using NUnit.Framework;

namespace test
{
	[TestFixture]
	public class DecompressorTests : BaseTest
	{
		[Test]
		public void DecompressEmptyFile()
		{
			var source = new MemoryStream(new byte[] { 0 });

			int rawLength = Decompressor.Decompress(source, Stream.Null);
			Assert.That(rawLength, Is.EqualTo(0));
		}

		[Test]
		public void DecompressOneByteFile()
		{
			var source = new MemoryStream(new byte[] { 1, 0, 1 });

			var output = new MemoryStream();
			int rawLength = Decompressor.Decompress(source, output);
			
			Assert.That(rawLength, Is.EqualTo(1));
			Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 1 }));

			output = new MemoryStream();
			source = new MemoryStream(new byte[] { 4, 12, 1, 2, 3, 4 });
			rawLength = Decompressor.Decompress(source, output);
			
			Assert.That(rawLength, Is.EqualTo(4));
			Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 1, 2, 3, 4, }));
		}

		[Test]
		public void DecompressTwoBlockFile2()
		{
			var expected = Enumerable.Repeat(1, 31).Select(x => (byte)x).ToArray();

			using (var source = File.OpenRead(GetExpectedFile("1x31.bin.comp")))
			{
				var output = new MemoryStream();
				int rawLength = Decompressor.Decompress(source, output);

				Assert.That(rawLength, Is.EqualTo(31));
				Assert.That(output.ToArray(), Is.EqualTo(expected));
			}
		}

		[Test]
		public void DecompressTwoBlockFile()
		{
			var expected = Enumerable.Range(1, 31).Select(x => (byte) x).ToArray();

			using ( var source = File.OpenRead(GetExpectedFile("1..31.bin.comp")) )
			{
				var output = new MemoryStream();
				int rawLength = Decompressor.Decompress(source, output);

				Assert.That(rawLength, Is.EqualTo(31));
				Assert.That(output.ToArray(), Is.EqualTo(expected));
			}
		}

		[Test]
		public void DecompressFile()
		{
			var expected = GetTestFile("alice29.txt");
			
			using ( var source = File.OpenRead(GetExpectedFile("alice29.txt.comp")) )
			{
				var output = new MemoryStream();
				Decompressor.Decompress(source, output);

				var actual = GetOutputFile("alice29.txt");
				File.WriteAllBytes(actual, output.ToArray());

				FileAssert.AreEqual(expected, actual);
			}
		}
	}
}