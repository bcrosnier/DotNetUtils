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
        IActivityLogger _logger = TestUtils.CreateLogger();

        [Test]
        public void NewCheckSolutionFile()
        {
            SolutionCheckResult r = SolutionChecker.NewCheckSolutionFile( SolutionParseTests.TEST_SLN_FILE_PATH );
            Assert.That( r, Is.Not.Null );

            CollectionAssert.IsNotEmpty( r.NuGetPackages );
            CollectionAssert.AllItemsAreNotNull( r.NuGetPackages );
            CollectionAssert.AllItemsAreUnique( r.NuGetPackages );

            CollectionAssert.IsNotEmpty( r.ProjectReferences );
            CollectionAssert.AllItemsAreNotNull( r.ProjectReferences );
            CollectionAssert.AllItemsAreUnique( r.ProjectReferences );

            CollectionAssert.IsNotEmpty( r.Projects );
            CollectionAssert.AllItemsAreNotNull( r.Projects );
            CollectionAssert.AllItemsAreUnique( r.Projects );
        }

        [Test]
        public void CheckSolutionFile()
        {
            SolutionCheckResult d = SolutionChecker.NewCheckSolutionFile( SolutionParseTests.TEST_SLN_FILE_PATH );

            d = SolutionChecker.NewCheckSolutionFile( @"D:\Benjamin\Development\CSharp\ck-certified\CK-Certified.sln" );

            foreach ( KeyValuePair<string, IEnumerable<IPackage>> pair in d.PackagesWithMultipleVersions.OrderBy( x => x.Key ) )
            {
                string packageName = pair.Key;

                using( _logger.OpenGroup( LogLevel.Warn, "Multiple package versions for: {0}", packageName ) )
                {
                    foreach( IPackage package in pair.Value )
                    {
                        string packageIdentifier = package.Id + '.' + package.Version;

                        using( _logger.OpenGroup( LogLevel.Warn, "Projects referencing {0}, {1}:", package.Id, package.Version ) )
                        {
                            foreach( ISolutionProjectItem project in d.GetProjectsReferencing( packageIdentifier ).OrderBy( x => x.ProjectName ) )
                            {
                                _logger.Warn( "{0}", project.ProjectName );
                            }
                        }
                    }
                }
            }
        }
    }
}