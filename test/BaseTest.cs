using System;
using System.IO;
using NUnit.Framework;

namespace test
{
	public class BaseTest
	{
		protected string TestDirectory
		{
			get
			{
				var current = Environment.CurrentDirectory;
				while ( !Directory.Exists(Path.Combine(current, "Test Data")) )
				{
					current = Path.GetDirectoryName(current);
					if ( current == null )
						throw new InconclusiveException("Cannot determine test root (Working directory must be a child of the project directory)");
				}

				return current;
			}
		}

		protected string GetExpectedFile(string filename)
		{
			return Path.Combine(TestDirectory, Path.Combine("Expected", filename));
		}

		protected string GetOutputFile(string filename)
		{
			var outputDirectory = Path.Combine(TestDirectory, "Results");
			if ( !Directory.Exists(outputDirectory) )
				Directory.CreateDirectory(outputDirectory);

			return Path.Combine(outputDirectory, filename);
		}

		protected string GetTestFile(string filename)
		{
			return Path.Combine(TestDirectory, Path.Combine("Test Data", filename));
		}
	}
}