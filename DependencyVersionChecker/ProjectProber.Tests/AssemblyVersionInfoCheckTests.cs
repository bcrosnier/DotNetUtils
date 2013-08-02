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
			Assert.That( result.HaveSharedAssemblyInfo, Is.True );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.False );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.False );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False );
			Assert.That( result.MultipleAssemblyVersion, Is.False );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.False );
			Assert.That( result.RelativeLinkInCSProjNotFound, Is.False );
			Assert.That( result.HaveFileWithoutVersion, Is.False );

			result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_SLN_FILE_PATH );
			Assert.That( result.HaveSharedAssemblyInfo, Is.True );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.False );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.False );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False );
			Assert.That( result.MultipleAssemblyVersion, Is.False );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.False );
			Assert.That( result.RelativeLinkInCSProjNotFound, Is.True );
			Assert.That( result.HaveFileWithoutVersion, Is.False );

			result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHDIFFERENTVERSION_SLN_FILE_PATH );
			Assert.That( result.HaveSharedAssemblyInfo, Is.True );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.False );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.False );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False );
			Assert.That( result.MultipleAssemblyVersion, Is.False );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.True );
			Assert.That( result.RelativeLinkInCSProjNotFound, Is.False );
			Assert.That( result.HaveFileWithoutVersion, Is.False );

			result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHDIFFERENTVERSIONINASSEMBLYINFO_SLN_FILE_PATH );
			Assert.That( result.HaveSharedAssemblyInfo, Is.False );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.True );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.True );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False );
			Assert.That( result.MultipleAssemblyVersion, Is.True );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.True );
			Assert.That( result.RelativeLinkInCSProjNotFound, Is.False );
			Assert.That( result.HaveFileWithoutVersion, Is.True );

			result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( TEST_WITHSHAREDASSEMBLYINFOANDASSEMBLYINFO_SLN_FILE_PATH );
			Assert.That( result.HaveSharedAssemblyInfo, Is.True );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.False );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.False );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False);
			Assert.That( result.MultipleAssemblyVersion, Is.False );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.False );
			Assert.That( result.RelativeLinkInCSProjNotFound, Is.False );
			Assert.That( result.HaveFileWithoutVersion, Is.False );
		}
	}
}
