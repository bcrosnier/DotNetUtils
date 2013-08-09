//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.Versioning;
//using CK.Core;
//using NuGet;
//using NUnit.Framework;
//using ProjectProber.Interfaces;

//namespace ProjectProber.Tests
//{
//    [TestFixture]
//    public class ProjectReferenceTests
//    {
//        private IActivityLogger _logger = TestUtils.CreateLogger();
//        private IPackageRepository _localRepository;

//        [Test]
//        public void TestReferences()
//        {
//            var projectItem = GetAssemblyProberProjectItem();
//            string filename = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, projectItem.ProjectPath );

//            Assert.That( File.Exists( filename ), "Found project file" );

//            //IEnumerable<ProjectItem> items = ProjectUtils.LoadProjectReferencesFromFile( filename );
            IEnumerable<IProjectAssemblyReference> refs = ProjectUtils.GetReferencesFromProjectFile( filename );

//            CollectionAssert.IsNotEmpty( refs );
//            CollectionAssert.AllItemsAreNotNull( refs );
//            CollectionAssert.AllItemsAreUnique( refs );
//        }

//        [Test]
//        public void GetAllPackageReferencesInSolution()
//        {
//            List<NuGet.IPackage> packages = new List<NuGet.IPackage>();

//            ISolution s = SolutionParseTests.GetTestSolution();

//            string packageRoot = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, @"packages" );

//            LocalPackageRepository repository = new LocalPackageRepository( packageRoot );

//            Assert.That( Directory.Exists( packageRoot ), "NuGet package root exists" );

//            foreach( ISolutionProjectItem projectItem in s.Projects.Where( pi => pi.GetItemType() == SolutionProjectType.VISUAL_C_SHARP ) )
//            {
//                string filename = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, projectItem.ProjectPath );
//                Assert.That( File.Exists( filename ), "Found project file at {0}", filename );

//                string path = Path.Combine( Path.GetDirectoryName( filename ), "packages.config" );

//                if( File.Exists( path ) )
//                {
//                    IEnumerable<INuGetPackageReference> packageRefs = ProjectUtils.GetReferencesFromPackageConfig( path );

//                    CollectionAssert.IsNotEmpty( packageRefs );
//                    CollectionAssert.AllItemsAreNotNull( packageRefs );
//                    CollectionAssert.AllItemsAreUnique( packageRefs );

//                    foreach( var packageRef in packageRefs )
//                    {
//                        var package = repository.FindPackage( packageRef.Id, SemanticVersion.Parse( packageRef.Version ), true, true );
//                        packages.Add( package );
//                    }
//                }
//            }

//            CollectionAssert.IsNotEmpty( packages );
//            CollectionAssert.AllItemsAreNotNull( packages );
//        }

//        [Test]
//        public void TestPackageConfig()
//        {
//            string path = Path.Combine( GetTestProjectDirectoryPath(), "packages.config" );

//            IEnumerable<INuGetPackageReference> references = ProjectUtils.GetReferencesFromPackageConfig( path );

//            CollectionAssert.IsNotEmpty( references );
//            CollectionAssert.AllItemsAreNotNull( references );
//            CollectionAssert.AllItemsAreUnique( references );
//        }

//        [Test]
//        public void TestManualProjectReferenceParsing()
//        {
//            string path = GetTestProjectFilePath();

            IEnumerable<IProjectAssemblyReference> references = ProjectUtils.GetReferencesFromProjectFile( path );

//            CollectionAssert.IsNotEmpty( references );
//            CollectionAssert.AllItemsAreNotNull( references );
//            CollectionAssert.AllItemsAreUnique( references );
//        }

//        [Test]
//        public void AllReferencesAreValid()
//        {
//            ISolution s = SolutionParseTests.GetTestSolution();

//            string packageRoot = Path.Combine( s.DirectoryPath, "packages" );

//            _localRepository = new LocalPackageRepository( packageRoot );

//            _logger.Info( "Solution directory: {0}", s.DirectoryPath );

//            foreach( ISolutionProjectItem item in s.Projects.Where( pi => pi.GetItemType() == SolutionProjectType.VISUAL_C_SHARP ) )
//            {
//                using( _logger.OpenGroup( LogLevel.Info, item.ProjectName ) )
//                {
//                    string fullProjectFilePath = Path.GetFullPath( Path.Combine( s.DirectoryPath, item.ProjectPath ) );
//                    string projectDirectory = Path.GetFullPath( Path.GetDirectoryName( fullProjectFilePath ) );

//                    Assert.That( File.Exists( fullProjectFilePath ), "Project file {0} exists", fullProjectFilePath );

                    IEnumerable<IProjectAssemblyReference> projectReferences = ProjectUtils.GetReferencesFromProjectFile( fullProjectFilePath );

//                    CollectionAssert.IsNotEmpty( projectReferences );
//                    CollectionAssert.AllItemsAreNotNull( projectReferences );
//                    CollectionAssert.AllItemsAreUnique( projectReferences );

//                    IEnumerable<string> hintPaths = projectReferences
//                        .Where( i => i.HintPath != null )
//                        .Select( i => i.HintPath );

//                    CollectionAssert.AllItemsAreNotNull( hintPaths );
//                    CollectionAssert.AllItemsAreUnique( hintPaths );

//                    foreach( string hintPath in hintPaths )
//                    {
//                        string assemblyPath = Path.GetFullPath( Path.Combine( projectDirectory, hintPath ) );
//                        _logger.Info( "Project ref: {0}", assemblyPath );

//                        Assert.That( File.Exists( assemblyPath ), "Reference exists: {0}", assemblyPath );
//                    }

//                    string packagesConfigFilePath = Path.Combine( projectDirectory, "packages.config" );
//                    if( File.Exists( packagesConfigFilePath ) )
//                    {
//                        IEnumerable<INuGetPackageReference> packageRefs = ProjectUtils.GetReferencesFromPackageConfig( packagesConfigFilePath );

//                        CollectionAssert.AllItemsAreNotNull( packageRefs );
//                        CollectionAssert.AllItemsAreUnique( packageRefs );

//                        foreach( INuGetPackageReference packageRef in packageRefs )
//                        {
//                            IPackage package = ProjectUtils.GetPackageFromReference( packageRef, packageRoot );

//                            Assert.That( package, Is.Not.Null, "Package exists" );

//                            FrameworkName framework = packageRef.TargetFramework;

//                            PrintPackageInfo( package, framework );
//                        }
//                    }
//                }
//            }
//        }

//        public void PrintPackageInfo( IPackage package, FrameworkName framework )
//        {
//            var dependencies = package.GetCompatiblePackageDependencies( framework );

//            if( dependencies.Count() > 0 )
//            {
//                using( _logger.OpenGroup( LogLevel.Info, "NuGet package: {0} {1} - dependencies", package.Id, package.Version ) )
//                {
//                    foreach( var dep in dependencies )
//                    {
//                        _logger.Info( "{0} {1} depends on: {2} {3}", package.Id, package.Version, dep.Id, dep.VersionSpec );

//                        IPackage depPackage = _localRepository.FindPackage( dep.Id, dep.VersionSpec, true, true );
//                        if( depPackage == null )
//                        {
//                            _logger.Error( "Couldn'depPackage resolve dependency: {0} {1}", dep.Id, dep.VersionSpec );
//                        }
//                        else
//                        {
//                            PrintPackageInfo( depPackage, framework );
//                        }
//                    }
//                }
//            }
//            else
//            {
//                _logger.Info( "NuGet package: {0} {1}", package.Id, package.Version );
//            }
//        }

//        public static ISolutionProjectItem GetTestProjectItem()
//        {
//            ISolution s = SolutionParseTests.GetTestSolution();

//            ISolutionProjectItem i = s.Projects.Where( pi => pi.ProjectName == @"ProjectProber.Tests" ).FirstOrDefault();

//            Assert.That( i != null, "Found test project item" );

//            return i;
//        }

//        public static ISolutionProjectItem GetAssemblyProberProjectItem()
//        {
//            ISolution s = SolutionParseTests.GetTestSolution();

//            ISolutionProjectItem i = s.Projects.Where( pi => pi.ProjectName == @"AssemblyProber" ).FirstOrDefault();

//            Assert.That( i != null, "Found AssemblyProber project item" );

//            return i;
//        }

//        public static string GetTestProjectDirectoryPath()
//        {
//            string path = Path.GetDirectoryName( GetTestProjectFilePath() );
//            Assert.That( Directory.Exists( path ), "Test project directory was found" );

//            return path;
//        }

//        public static string GetTestProjectFilePath()
//        {
//            string path = Path.Combine( SolutionParseTests.TEST_SLN_DIRECTORY_PATH, GetTestProjectItem().ProjectPath );
//            Assert.That( File.Exists( path ), "Test project file was found" );

//            return path;
//        }
//    }
//}