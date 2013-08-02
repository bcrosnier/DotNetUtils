using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Tests
{
	[TestFixture]
	public class AssemblyVersionInfoCheckTests
	{
		[Test]
		public void CheckAssemblyVersionFileTest()
		{
			AssemblyVersionInfoCheckResult result = AssemblyVersionInfoChecker.CheckAssemblyVersionFiles( SolutionParseTests.TEST_CK_CORE_SLN_FILE_PATH );
			Assert.That( result.HaveSharedAssemblyInfo, Is.True );
			Assert.That( result.HaveOneVersionNotSemanticVersionCompliante, Is.False );
			Assert.That( result.MultipleAssemblyFileInfoVersion, Is.False );
			Assert.That( result.MultipleAssemblyInformationVersion, Is.False );
			Assert.That( result.MultipleAssemblyVersion, Is.False );
			Assert.That( result.MultipleRelativeLinkInCSProj, Is.False );
			Assert.That( result.MultipleSharedAssemblyInfo, Is.False );
			Assert.That( result.MultipleVersionInOneAssemblyInfoFile, Is.False );
		}
	}
}
