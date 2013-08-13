using System.IO;
using NUnit.Framework;
using System.Linq;
using System;
using System.Xml;
using ProjectProber.SerializationExtensions;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class AssemblyVersionInfoCheckTests
    {
        public static readonly string TEST_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "AssemblyProber.sln" );
        public static readonly string TEST_WITHSHAREDASSEMBLYINFO_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithSharedAssemblyInfo", "CK-Core.sln" );
        public static readonly string TEST_WITHDIFFERENTVERSION_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithDifferentVersion", "CK-Core.sln" );
        public static readonly string TEST_WITHDIFFERENTVERSIONINASSEMBLYINFO_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithDifferentVersionInAssemblyInfo", "CK-Desktop.sln" );
        public static readonly string TEST_WITHSHAREDASSEMBLYINFOANDASSEMBLYINFO_SLN_FILE_PATH = Path.Combine("..", "..", "..", "..", "TestSolutions", "WithSharedAssemblyInfoAndAssemblyInfo", "CK-Core.sln");
        public static readonly string TEST_WITHSHAREDASSEMBLYINFONOTSEMANTICVERSIONCOMPLIANTE_SLN_FILE_PATH = Path.Combine("..", "..", "..", "..", "TestSolutions", "WithAssemblyInfoNotSemanticVersionCompliant", "CK-Core.sln");

        [Test]
        public void CheckAssemblyVersionFileTest1()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(TEST_WITHSHAREDASSEMBLYINFO_SLN_FILE_PATH);
            Assert.That(result.HasNotSharedAssemblyInfo, Is.False);
            Assert.That(result.HasOneVersionNotSemanticVersionCompliant, Is.False);
            Assert.That(result.HasMultipleAssemblyFileVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyInformationVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyVersion, Is.False);
            Assert.That(result.HasMultipleRelativeLinkInCSProj, Is.False);
            Assert.That(result.HasMultipleSharedAssemblyInfo, Is.False);
            Assert.That(result.HasMultipleVersionInOneAssemblyInfo, Is.False);
            Assert.That(result.HasRelativeLinkInCSProjNotFound, Is.False);
            Assert.That(result.HasFileWithoutVersion, Is.False);
            Assert.That(result.HasAssemblyInfoWithVersion, Is.False);
        }

        [Test]
        public void CheckAssemblyVersionFileTest2()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(TEST_WITHSHAREDASSEMBLYINFOANDASSEMBLYINFO_SLN_FILE_PATH);
            Assert.That(result.HasNotSharedAssemblyInfo, Is.False);
            Assert.That(result.HasOneVersionNotSemanticVersionCompliant, Is.False);
            Assert.That(result.HasMultipleAssemblyFileVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyInformationVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyVersion, Is.False);
            Assert.That(result.HasMultipleRelativeLinkInCSProj, Is.False);
            Assert.That(result.HasMultipleSharedAssemblyInfo, Is.False);
            Assert.That(result.HasMultipleVersionInOneAssemblyInfo, Is.False);
            Assert.That(result.HasRelativeLinkInCSProjNotFound, Is.False);
            Assert.That(result.HasFileWithoutVersion, Is.False);
            Assert.That(result.HasAssemblyInfoWithVersion, Is.True);
        }

        [Test]
        public void CheckAssemblyVersionFileTest3()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(TEST_WITHDIFFERENTVERSIONINASSEMBLYINFO_SLN_FILE_PATH);
            Assert.That(result.HasNotSharedAssemblyInfo, Is.True);
            Assert.That(result.HasOneVersionNotSemanticVersionCompliant, Is.True);
            Assert.That(result.HasMultipleAssemblyFileVersion, Is.True);
            Assert.That(result.HasMultipleAssemblyInformationVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyVersion, Is.True);
            Assert.That(result.HasMultipleRelativeLinkInCSProj, Is.False);
            Assert.That(result.HasMultipleSharedAssemblyInfo, Is.False);
            Assert.That(result.HasMultipleVersionInOneAssemblyInfo, Is.True);
            Assert.That(result.HasRelativeLinkInCSProjNotFound, Is.False);
            //Assert.That(result.HasFileWithoutVersion, Is.True); need change
            Assert.That(result.HasAssemblyInfoWithVersion, Is.False);
        }

        [Test]
        public void CheckAssemblyVersionFileTest4()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(TEST_WITHDIFFERENTVERSION_SLN_FILE_PATH);
            Assert.That(result.HasNotSharedAssemblyInfo, Is.False);
            Assert.That(result.HasOneVersionNotSemanticVersionCompliant, Is.False);
            Assert.That(result.HasMultipleAssemblyFileVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyInformationVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyVersion, Is.False);
            Assert.That(result.HasMultipleRelativeLinkInCSProj, Is.False);
            Assert.That(result.HasMultipleSharedAssemblyInfo, Is.False);
            Assert.That(result.HasMultipleVersionInOneAssemblyInfo, Is.True);
            Assert.That(result.HasRelativeLinkInCSProjNotFound, Is.False);
            Assert.That( result.HasFileWithoutVersion, Is.False );
            Assert.That( result.HasAssemblyInfoWithVersion, Is.False );
        }

        [Test]
        public void CheckAssemblyVersionFileTest5()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles(TEST_WITHSHAREDASSEMBLYINFONOTSEMANTICVERSIONCOMPLIANTE_SLN_FILE_PATH);
            Assert.That(result.HasNotSharedAssemblyInfo, Is.False);
            Assert.That(result.HasOneVersionNotSemanticVersionCompliant, Is.True);
            Assert.That(result.HasMultipleAssemblyFileVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyInformationVersion, Is.False);
            Assert.That(result.HasMultipleAssemblyVersion, Is.False);
            Assert.That(result.HasMultipleRelativeLinkInCSProj, Is.False);
            Assert.That(result.HasMultipleSharedAssemblyInfo, Is.False);
            Assert.That(result.HasMultipleVersionInOneAssemblyInfo, Is.True);
            Assert.That(result.HasRelativeLinkInCSProjNotFound, Is.False);
            Assert.That(result.HasFileWithoutVersion, Is.False);
            Assert.That(result.HasAssemblyInfoWithVersion, Is.False);
        }

        [Test]
        public void TestToSerialize()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHSHAREDASSEMBLYINFONOTSEMANTICVERSIONCOMPLIANTE_SLN_FILE_PATH );
            using( XmlTextWriter xw = new XmlTextWriter( Path.GetDirectoryName( TEST_SLN_FILE_PATH ) + "test.xml", null ) )
            {
                xw.WriteStartDocument( true );
                xw.Formatting = Formatting.Indented;
                xw.WriteProcessingInstruction( "xml-stylesheet", "type='text/xsl' href='SolutionCheckResult.xslt'" );
                result.SerializeTo( xw );
                xw.WriteEndDocument();
            }
        }
    }
}