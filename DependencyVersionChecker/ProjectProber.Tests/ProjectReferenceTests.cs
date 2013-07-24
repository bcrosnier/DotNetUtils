using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectProber.Interfaces;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class ProjectReferenceTests
    {
        [Test]
        public void TestReferences()
        {
            var projectItem = GetAssemblyProberProjectItem();
            string filename = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, projectItem.ProjectPath );

            Assert.That( File.Exists( filename ), "Found project file" );

            //IEnumerable<ProjectItem> items = ProjectUtils.LoadProjectReferencesFromFile( filename );

            IEnumerable<string> hintPaths = ProjectUtils.GetPackageLibraryReferences( filename );

            List<IPackageLibraryReference> refs = new List<IPackageLibraryReference>();
            foreach ( string hintPath in hintPaths )
            {
                IPackageLibraryReference libRef = ProjectUtils.ParseReferenceFromPath( hintPath );
                refs.Add( libRef );
            }

            CollectionAssert.IsNotEmpty( refs );
            CollectionAssert.AllItemsAreNotNull( refs );
            CollectionAssert.AllItemsAreUnique( refs );
        }

        [Test]
        public void GetAllPackageReferencesInSolution()
        {
            List<IPackageLibraryReference> refs = new List<IPackageLibraryReference>();
            List<NuGet.IPackage> packages = new List<NuGet.IPackage>();

            ISolution s = SolutionParseTests.GetTestSolution();

            string packageRoot = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, @"packages" );

            Assert.That( Directory.Exists( packageRoot ), "NuGet package root exists" );

            foreach ( ISolutionProjectItem projectItem in s.Projects.Where( pi => SolutionUtils.GetProjectType( pi ) == SolutionProjectType.VISUAL_C_SHARP ) )
            {
                string filename = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, projectItem.ProjectPath );

                Assert.That( File.Exists( filename ), "Found project file at {0}", filename );

                IEnumerable<string> hintPaths = ProjectUtils.GetPackageLibraryReferences( filename );

                foreach ( string hintPath in hintPaths )
                {
                    IPackageLibraryReference libRef = ProjectUtils.ParseReferenceFromPath( hintPath );
                    refs.Add( libRef );
                    var package = ProjectUtils.GetPackageFromReference( libRef, packageRoot );
                    packages.Add( package );
                }
            }

            CollectionAssert.IsNotEmpty( refs );
            CollectionAssert.AllItemsAreNotNull( refs );
            CollectionAssert.IsNotEmpty( packages );
            CollectionAssert.AllItemsAreNotNull( packages );
        }

        public static ISolutionProjectItem GetTestProjectItem()
        {
            ISolution s = SolutionParseTests.GetTestSolution();

            ISolutionProjectItem i = s.Projects.Where( pi => pi.ProjectName == @"ProjectProber.Tests" ).FirstOrDefault();

            Assert.That( i != null, "Found test project item" );

            return i;
        }

        public static ISolutionProjectItem GetAssemblyProberProjectItem()
        {
            ISolution s = SolutionParseTests.GetTestSolution();

            ISolutionProjectItem i = s.Projects.Where( pi => pi.ProjectName == @"AssemblyProber" ).FirstOrDefault();

            Assert.That( i != null, "Found AssemblyProber project item" );

            return i;
        }
    }
}