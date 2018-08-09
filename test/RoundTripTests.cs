using System.IO;
using NSpanny;
using NUnit.Framework;

namespace test
{
	[TestFixture]
	public class RoundTripTests : BaseTest
	{
		[TestCase("alice29.txt")]
		[TestCase("asyoulik.txt")]
		[TestCase("cp.html")]
		[TestCase("fields.c")]
		[TestCase("geo.protodata")]
		[TestCase("grammar.lsp")]
		[TestCase("house.jpg")]
		[TestCase("html")]
		[TestCase("html_x_4")]
		[TestCase("kennedy.xls")]
		[TestCase("kppkn.gtb")]
		[TestCase("lcet10.txt")]
		[TestCase("mapreduce-osdi-1.pdf")]
		[TestCase("plrabn12.txt")]
		[TestCase("ptt5")]
		[TestCase("sum")]
		[TestCase("urls.10k")]
		[TestCase("xargs.1")]
		public void Test(string filename)
		{
			var inputFile = GetTestFile(@filename);

			var compressedStream = new MemoryStream();
			using (var fs = File.OpenRead(inputFile))
			{
				var compressor = new Compressor();
				compressor.Compress(fs, compressedStream);
			}

			compressedStream.Seek(0, SeekOrigin.Begin);

			var outputFile = GetOutputFile(filename);
			using (var fs = File.Create(outputFile))
			{
				Decompressor.Decompress(compressedStream, fs);
			}

			FileAssert.AreEqual(inputFile, outputFile);
		}
	}
}