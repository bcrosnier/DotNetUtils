using System.IO;
using NUnit.Framework;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class AssemblyVersionInfoCheckTests
    {
        public static readonly string TEST_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "AssemblyProber.sln" );
        public static readonly string TEST_WITHSHAREDASSEMBLYINFO_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithSharedAssemblyInfo", "CK-Core.sln" );
        public static readonly string TEST_WITHDIFFERENTVERSION_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithDifferentVersion", "CK-Core.sln" );
        public static readonly string TEST_WITHDIFFERENTVERSIONINASSEMBLYINFO_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithDifferentVersionInAssemblyInfo", "CK-Desktop.sln" );
        public static readonly string TEST_WITHSHAREDASSEMBLYINFOANDASSEMBLYINFO_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "..", "TestSolutions", "WithSharedAssemblyInfoAndAssemblyInfo", "CK-Core.sln" );

        [Test]
        public void CheckAssemblyVersionFileTest()
        {
            AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHSHAREDASSEMBLYINFO_SLN_FILE_PATH );
            Assert.That( result.HasSharedAssemblyInfo, Is.True );
            Assert.That( result.HasOneVersionNotSemanticVersionCompliant, Is.False );
            Assert.That( result.HasMultipleAssemblyFileVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyInformationVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyVersion, Is.False );
            Assert.That( result.HasMultipleRelativeLinkInCSProj, Is.False );
            Assert.That( result.HasMultipleSharedAssemblyInfo, Is.False );
            Assert.That( result.HasMultipleVersionInOneAssemblyInfoFile, Is.False );
            Assert.That( result.HasRelativeLinkInCSProjNotFound, Is.False );
            Assert.That( result.HaveFileWithoutVersion, Is.False );

            result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_SLN_FILE_PATH );
            Assert.That( result.HasSharedAssemblyInfo, Is.True );
            Assert.That( result.HasOneVersionNotSemanticVersionCompliant, Is.False );
            Assert.That( result.HasMultipleAssemblyFileVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyInformationVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyVersion, Is.False );
            Assert.That( result.HasMultipleRelativeLinkInCSProj, Is.False );
            Assert.That( result.HasMultipleSharedAssemblyInfo, Is.False );
            Assert.That( result.HasMultipleVersionInOneAssemblyInfoFile, Is.False );
            Assert.That( result.HasRelativeLinkInCSProjNotFound, Is.True );
            Assert.That( result.HaveFileWithoutVersion, Is.False );

            result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHDIFFERENTVERSION_SLN_FILE_PATH );
            Assert.That( result.HasSharedAssemblyInfo, Is.True );
            Assert.That( result.HasOneVersionNotSemanticVersionCompliant, Is.False );
            Assert.That( result.HasMultipleAssemblyFileVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyInformationVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyVersion, Is.False );
            Assert.That( result.HasMultipleRelativeLinkInCSProj, Is.False );
            Assert.That( result.HasMultipleSharedAssemblyInfo, Is.False );
            Assert.That( result.HasMultipleVersionInOneAssemblyInfoFile, Is.True );
            Assert.That( result.HasRelativeLinkInCSProjNotFound, Is.False );
            Assert.That( result.HaveFileWithoutVersion, Is.False );

            result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHDIFFERENTVERSIONINASSEMBLYINFO_SLN_FILE_PATH );
            Assert.That( result.HasSharedAssemblyInfo, Is.False );
            Assert.That( result.HasOneVersionNotSemanticVersionCompliant, Is.True );
            Assert.That( result.HasMultipleAssemblyFileVersion, Is.True );
            Assert.That( result.HasMultipleAssemblyInformationVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyVersion, Is.True );
            Assert.That( result.HasMultipleRelativeLinkInCSProj, Is.False );
            Assert.That( result.HasMultipleSharedAssemblyInfo, Is.False );
            Assert.That( result.HasMultipleVersionInOneAssemblyInfoFile, Is.True );
            Assert.That( result.HasRelativeLinkInCSProjNotFound, Is.False );
            Assert.That( result.HaveFileWithoutVersion, Is.True );

            result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHSHAREDASSEMBLYINFOANDASSEMBLYINFO_SLN_FILE_PATH );
            Assert.That( result.HasSharedAssemblyInfo, Is.True );
            Assert.That( result.HasOneVersionNotSemanticVersionCompliant, Is.False );
            Assert.That( result.HasMultipleAssemblyFileVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyInformationVersion, Is.False );
            Assert.That( result.HasMultipleAssemblyVersion, Is.False );
            Assert.That( result.HasMultipleRelativeLinkInCSProj, Is.False );
            Assert.That( result.HasMultipleSharedAssemblyInfo, Is.False );
            Assert.That( result.HasMultipleVersionInOneAssemblyInfoFile, Is.False );
            Assert.That( result.HasRelativeLinkInCSProjNotFound, Is.False );
            Assert.That( result.HaveFileWithoutVersion, Is.False );
        }
    }
}