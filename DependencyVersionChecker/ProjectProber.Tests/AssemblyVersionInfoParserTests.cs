using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Tests
{
	[TestFixture]
	public class AssemblyVersionInfoParserTests
	{
		[Test]
		public void GetAssemblyVersionFromSharedAssemblyInfoFileTest()
		{
			Version version = AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( 
				Path.Combine( SolutionParseTests.TEST_CK_CORE_SLN_DIRECTORY_PATH, "SharedAssemblyInfo.cs" ), 
				AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN );
			
			Assert.That( version, Is.EqualTo( new Version( "2.8.14" ) ) );

			version = AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( 
				Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, "SharedAssemblyInfo.cs" ), 
				AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN );

			Assert.That( version, Is.Null );

		}
	}
}
