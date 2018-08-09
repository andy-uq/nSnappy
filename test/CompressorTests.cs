using System;
using System.IO;
using System.Linq;
using NSpanny;
using NUnit.Framework;

namespace test
{
	[TestFixture]
	public class CompressorTests : BaseTest
	{
		private int CompressData(byte[] data, string filename)
		{
			var actual = GetOutputFile(filename);
			using ( var fs = File.Create(actual) )
			{
				var compressor = new Compressor();
				return compressor.Compress(new MemoryStream(data), fs);
			}
		}

		[Test]
		public void ZeroSizedFile()
		{
			var ms = new MemoryStream();
			var compressor = new Compressor();
			int compressedLength = compressor.Compress(Stream.Null, ms);

			Assert.That(compressedLength, Is.EqualTo(1));
			Assert.That(ms.ToArray(), Is.EqualTo(new byte[] { 0 }));
		}

		[Test]
		public void ByteSizedFile()
		{
			var compressor = new Compressor();

			var ms1 = new MemoryStream();
			int compressedLength = compressor.Compress(new MemoryStream(new byte[] { 1 }), ms1);
			Assert.That(compressedLength, Is.EqualTo(3));
			Assert.That(ms1.ToArray(), Is.EqualTo(new byte[] { 1, 0, 1 }));

			var ms2 = new MemoryStream();
			compressedLength = compressor.Compress(new MemoryStream(new byte[] { 1, 2, 3, 4, }), ms2);
			Assert.That(compressedLength, Is.EqualTo(6));
			Assert.That(ms2.ToArray(), Is.EqualTo(new byte[] { 4, 12, 1, 2, 3, 4 }));
		}

		[Test]
		public void TwoBlockUncompressibleFile()
		{
			var data = Enumerable.Range(1, 31).Select(x => (byte) x).ToArray();

			var compressedLength = CompressData(data, "1..31.bin");
			Assert.That(compressedLength, Is.EqualTo(33));

			FileAssert.AreEqual(GetExpectedFile("1..31.bin.comp"), GetOutputFile("1..31.bin"));
		}

		[Test]
		public void TwoBlockCompressibleFile()
		{
			var data = Enumerable.Repeat(1, 31).Select(x => (byte) x).ToArray();

			var compressedLength = CompressData(data, "1x31.bin");
			Assert.That(compressedLength, Is.EqualTo(6));

			FileAssert.AreEqual(GetExpectedFile("1x31.bin.comp"), GetOutputFile("1x31.bin"));
		}

		[Test]
		public void TenBlockCompressibleFile()
		{
			var data = Enumerable.Repeat(new byte[] { 1, 2, 3, 4, }, 1 << 10).SelectMany(x => x).ToArray();
			Assert.That(data.Length, Is.EqualTo(4 << 10));

			var compressedLength = CompressData(data, "1234x10.bin");
			Assert.That(compressedLength, Is.EqualTo(199));

			FileAssert.AreEqual(GetExpectedFile("1234x10.bin.comp"), GetOutputFile("1234x10.bin"));
		}

		[Test]
		public void CompressibleFile()
		{
			var compressor = new Compressor();

			var ms = new MemoryStream();
			var data = File.ReadAllBytes(GetTestFile(@"alice29.txt"));
			int compressedLength = compressor.Compress(new MemoryStream(data), ms);
			Assert.That(data.Length, Is.EqualTo(152089));
			Assert.That(compressedLength, Is.EqualTo(90965));

			var actual = GetOutputFile("alice29.bin");
			File.WriteAllBytes(actual, ms.ToArray());

			FileAssert.AreEqual(GetExpectedFile("alice29.txt.comp"), actual);
		}

		[Test]
		public void CompressibleFile2()
		{
			var compressor = new Compressor();

			var ms = new MemoryStream();
			var data = File.ReadAllBytes(GetTestFile(@"ptt5"));
			compressor.Compress(new MemoryStream(data), ms);

			var actual = GetOutputFile("ptt5.bin");
			File.WriteAllBytes(actual, ms.ToArray());

			FileAssert.AreEqual(GetExpectedFile("ptt5.comp"), actual);
		}
		
		[Test]
		public void CompressText()
		{
			var compressor = new Compressor();

			var ms = new MemoryStream();
			var data = System.Text.Encoding.ASCII.GetBytes("Wikipedia is a free, web-based, collaborative, multilingual encyclopedia project.");
			int compressedLength = compressor.Compress(new MemoryStream(data), ms);
			Assert.That(data.Length, Is.EqualTo(81));
			Assert.That(compressedLength, Is.EqualTo(84));

			var compressed = ms.ToArray();
			
			Assert.That(compressed[0], Is.EqualTo(0x51));
			Assert.That(compressed[1], Is.EqualTo(0xF0));
			Assert.That(compressed[2], Is.EqualTo(0x50));

			Console.WriteLine(Convert.ToBase64String(compressed));
			Assert.That(Convert.ToBase64String(compressed), Is.EqualTo("UfBQV2lraXBlZGlhIGlzIGEgZnJlZSwgd2ViLWJhc2VkLCBjb2xsYWJvcmF0aXZlLCBtdWx0aWxpbmd1YWwgZW5jeWNsb3BlZGlhIHByb2plY3Qu"));
		}

		[Test]
		public void GetHashTable()
		{
			var hashtable = new HashTable(0);
			Assert.That(hashtable.Size, Is.EqualTo(256));

			hashtable = new HashTable(CompressorConstants.MaxHashTableSize + 1);
			Assert.That(hashtable.Size, Is.EqualTo(CompressorConstants.MaxHashTableSize));
		}
	}
}