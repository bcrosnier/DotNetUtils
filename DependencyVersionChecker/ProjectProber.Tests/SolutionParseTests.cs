using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectProber.Interfaces;

namespace ProjectProber.Tests
{
    [TestFixture]
    public class SolutionParseTests
    {
        public static readonly string TEST_SLN_FILE_PATH = Path.Combine( "..", "..", "..", "AssemblyProber.sln" );
        public static readonly string TEST_SLN_DIRECTORY_PATH = new FileInfo( TEST_SLN_FILE_PATH ).DirectoryName;

        [Test]
        public void SolutionFileHasProjects()
        {
            ISolution solution = GetTestSolution();

            CollectionAssert.IsNotEmpty( solution.Projects );
            CollectionAssert.AllItemsAreNotNull( solution.Projects );
            CollectionAssert.AllItemsAreUnique( solution.Projects );
        }

        [Test]
        public void SolutionContainsKnownTypes()
        {
            ISolution solution = GetTestSolution();

            var cSharpProjects = solution.Projects
                .Where( s => s.GetItemType() == SolutionProjectType.VISUAL_C_SHARP );

            var solutionFolders = solution.Projects
                .Where( s => s.GetItemType() == SolutionProjectType.PROJECT_FOLDER );

            CollectionAssert.IsNotEmpty( cSharpProjects, "C# projects were found" );
            CollectionAssert.IsNotEmpty( solutionFolders, "Solution folders were found" );
        }

        [Test]
        public void CSharpProjectFilesExist()
        {
            ISolution solution = GetTestSolution();

            var cSharpProjects = solution.Projects
                .Where( s => s.GetItemType() == SolutionProjectType.VISUAL_C_SHARP );

            foreach( var project in cSharpProjects )
            {
                string fullFilePath = Path.Combine( TEST_SLN_DIRECTORY_PATH, project.ProjectPath );
                Assert.That( File.Exists( fullFilePath ), "Project path {0} exists", project.ProjectPath );
            }
        }

        [Test]
        public void ReadSolutionsFromDirectoryWorks()
        {
            IEnumerable<ISolution> solutions = SolutionFactory.ReadSolutionsFromDirectory( TEST_SLN_DIRECTORY_PATH );

            CollectionAssert.IsNotEmpty( solutions );
            CollectionAssert.AllItemsAreNotNull( solutions );
            CollectionAssert.AllItemsAreUnique( solutions );
        }

        public static ISolution GetTestSolution()
        {
            string slnPath = Path.Combine( TEST_SLN_FILE_PATH );

            Assert.That( File.Exists( slnPath ), "Solution file was found" );

            ISolution solution = SolutionFactory.ReadFromSolutionFile( slnPath );

            Assert.That( solution, Is.Not.Null, "Solution is not null" );

            return solution;
        }
    }
}