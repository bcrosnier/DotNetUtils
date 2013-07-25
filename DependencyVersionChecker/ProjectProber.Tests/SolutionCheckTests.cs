using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
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
        public void CheckSolutionFile()
        {
            SolutionCheckResult r = SolutionChecker.CheckSolutionFile( SolutionParseTests.TEST_SLN_FILE_PATH );
            Assert.That( r, Is.Not.Null );

            CollectionAssert.IsNotEmpty( r.NuGetPackages );
            CollectionAssert.AllItemsAreNotNull( r.NuGetPackages );
            CollectionAssert.AllItemsAreUnique( r.NuGetPackages );

            CollectionAssert.IsNotEmpty( r.ProjectAssemblyReferences );
            CollectionAssert.AllItemsAreNotNull( r.ProjectAssemblyReferences );
            CollectionAssert.AllItemsAreUnique( r.ProjectAssemblyReferences );

            CollectionAssert.IsNotEmpty( r.Projects );
            CollectionAssert.AllItemsAreNotNull( r.Projects );
            CollectionAssert.AllItemsAreUnique( r.Projects );
        }

        [Test]
        public void CheckSolutionPackageVersions()
        {
            SolutionCheckResult d = SolutionChecker.CheckSolutionFile( SolutionParseTests.TEST_SLN_FILE_PATH );

            //d = SolutionChecker.CheckSolutionFile( @"D:\Benjamin\Development\CSharp\ck-certified\CK-Certified.sln" );
            //d = SolutionChecker.CheckSolutionFile( @"D:\Benjamin\Development\CSharp\INVENIETIS-PRIVATE\papv\PAPV.sln" );
            //d = SolutionChecker.CheckSolutionFile( @"D:\Benjamin\Development\CSharp\INVENIETIS-PRIVATE\cofely-bo\Cofely.sln" );
            //d = SolutionChecker.CheckSolutionFile( @"D:\Benjamin\Development\CSharp\INVENIETIS-PRIVATE\ck-database\CK-Database.sln" );

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

            // Serialization test
            string s;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            using( XmlWriter xw = XmlWriter.Create( "test-result.xml", settings ) )
            {
                xw.WriteStartDocument( true );
                xw.WriteProcessingInstruction( "xml-stylesheet", "type='text/xsl' href='SolutionCheckResult.xslt'" );
                d.SerializeTo( xw );
                xw.WriteEndDocument();
            }

            Process.Start( "test-result.xml" );
        }
    }
}