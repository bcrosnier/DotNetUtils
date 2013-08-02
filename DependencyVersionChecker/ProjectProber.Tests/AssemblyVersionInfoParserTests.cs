using System;
using System.IO;
using NUnit.Framework;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class AssemblyVersionInfoParserTests
    {
        [Test]
        public void GetAssemblyVersionFromSharedAssemblyInfoFileTest()
        {
            Version version = AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile(
                Path.Combine( Path.GetDirectoryName( AssemblyVersionInfoCheckTests.TEST_WITHSHAREDASSEMBLYINFO_SLN_FILE_PATH ), "SharedAssemblyInfo.cs" ),
                AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN );

            Assert.That( version, Is.EqualTo( new Version( "2.8.14" ) ) );

            version = AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile(
                Path.Combine( Path.GetDirectoryName( AssemblyVersionInfoCheckTests.TEST_SLN_FILE_PATH ), "SharedAssemblyInfo.cs" ),
                AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN );

            Assert.That( version, Is.Null );
        }
    }
}