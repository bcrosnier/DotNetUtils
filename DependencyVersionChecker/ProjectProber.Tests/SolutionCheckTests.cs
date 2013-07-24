using System.Collections.Generic;
using System.Linq;
using CK.Core;
using NuGet;
using NUnit.Framework;
using ProjectProber.Interfaces;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class SolutionCheckTests
    {
        [Test]
        public void CheckSolutionFile()
        {
            IActivityLogger logger = TestUtils.CreateLogger();

            SolutionChecker d = SolutionChecker.CheckSolutionFile( SolutionParseTests.TEST_SLN_FILE_PATH );

            //SolutionChecker d = SolutionChecker.CheckSolutionFile( @"D:\Benjamin\Development\CSharp\ck-certified\CK-Certified.sln" );

            foreach ( KeyValuePair<string, List<string>> pair in d.PackageNamesWithMultipleVersions.OrderBy( x => x.Key ) )
            {
                IEnumerable<string> packageVersions = pair.Value;
                string packageName = pair.Key;

                using ( logger.OpenGroup( LogLevel.Warn, "Multiple package versions for: {0}", packageName ) )
                {
                    foreach ( string packageVersionId in packageVersions )
                    {
                        IPackage package = d.Packages[packageVersionId];

                        Assert.That( package.Id == packageName );

                        using ( logger.OpenGroup( LogLevel.Warn, "Projects referencing {0}, {1}:", package.Id, package.Version ) )
                        {
                            foreach ( ISolutionProjectItem project in d.GetProjectsReferencing( packageVersionId ).OrderBy( x => x.ProjectName ) )
                            {
                                logger.Warn( "{0}", project.ProjectName );
                            }
                        }
                    }
                }
            }
        }
    }
}