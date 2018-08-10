using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace benchmarks
{
    class Program
    {
        static void Main()
        {
	        var config = ManualConfig
		        .Create(DefaultConfig.Instance)
		        .With(new MemoryDiagnoser());

	        BenchmarkRunner.Run<SpanNoSpan>(config);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SpanNoSpan
    {
        private readonly MemoryStream _input;

        public SpanNoSpan()
        {
            var data = File.ReadAllBytes(@"C:\git\nSnappy\test\Test Data\alice29.txt");
            _input = new MemoryStream(data);
        }

        [Benchmark]
        public long nSnappy()
        {
            var compressor = new NSnappy.Compressor();

            var output = new MemoryStream();

            _input.Seek(0, SeekOrigin.Begin);
            return compressor.Compress(_input, output);
        }

        [Benchmark]
        public int nSpanny()
        {
            var compressor = new NSpanny.Compressor();

            var output = new MemoryStream();

            _input.Seek(0, SeekOrigin.Begin);
            return compressor.Compress(_input, output);
        }
    }
}
