using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AssemblyProber.Tests
{
    [TestFixture]
    public class AssemblyLoaderTests
    {
        [Test]
        public void CanLoadTestAssembly()
        {
            DirectoryInfo testDir = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );
            Assert.That( testDir.Exists );

            IEnumerable<FileInfo> fileList = AssemblyVersionChecker.ListAssembliesFromDirectory( testDir, false );

            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );

            List<IAssemblyInfo> Files = new List<IAssemblyInfo>();

            foreach ( FileInfo f in fileList )
            {
                IAssemblyInfo a = l.LoadFromFile( f );
                IAssemblyInfo b = l.LoadFromFile( f );
                IAssemblyInfo c = l.LoadFromFile( f );

                Assert.That( a == b );
                Assert.That( b == c );

                Assert.That( a.Error, Is.Null );

                Files.Add( a );
            }

            AssemblyCheckTests.TestAssembliesInfo( Files );

            CollectionAssert.AllItemsAreUnique( Files );
            CollectionAssert.AllItemsAreNotNull( Files );

            IAssemblyInfo executingAssembly;
            IAssemblyInfo dependencyAssembly;

            executingAssembly =
                Files
                .Where( x => x.SimpleName == Assembly.GetExecutingAssembly().GetName().Name )
                .Select( x => x )
                .FirstOrDefault();

            dependencyAssembly =
                Files
                .Where( x => x.SimpleName == l.GetType().Assembly.GetName().Name )
                .Select( x => x )
                .FirstOrDefault();

            Assert.That( executingAssembly, Is.Not.Null, "Executing assembly was loaded" );

            Assert.That( dependencyAssembly, Is.Not.Null, "Dependency assembly was loaded" );

            CollectionAssert.Contains(
                executingAssembly.Dependencies.Values,
                dependencyAssembly,
                "Dependency is in executing assembly dependency list"
                );
        }

        [Test]
        public void CanLoadExternalAssembly()
        {
            string pathToExternalAssembly = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "..", "..", "ExternalAssemblies", "HostAssembly2.dll" );

            Assert.That( File.Exists( pathToExternalAssembly ), "External assembly file exists" );

            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );
            IAssemblyInfo info = l.LoadFromFile( new FileInfo( pathToExternalAssembly ) );

            Assert.That( IsAssemblyLoaded( info.FullName ), Is.False, "External assembly wasn't loaded when reflecting" );

            Assert.That( info.Error, Is.Null, "No error during assembly loading" );

            AssemblyCheckTests.TestAssemblyInfo( info );
        }

        [Test]
        public void CheckLoadByName()
        {
            DirectoryInfo testDir = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );
            Assert.That( testDir.Exists );

            IEnumerable<FileInfo> fileList = AssemblyVersionChecker.ListAssembliesFromDirectory( testDir, false );

            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );

            foreach( FileInfo f in fileList )
            {
                IAssemblyInfo a = l.LoadFromFile( f );
                IAssemblyInfo b = AssemblyLoader.ParseAssemblyInfoFromString( a.FullName );

                Assert.That( a.SimpleName == b.SimpleName , "Name is equivalent" );
                Assert.That( a.Version == b.Version, "Version is equivalent" );
                Assert.That( a.Culture == b.Culture, "Culture is equivalent" );
                Assert.That( a.FullName == b.FullName, "FullName is equivalent" );
                CollectionAssert.AreEqual( a.PublicKeyToken, b.PublicKeyToken );
            }

        }

        private bool IsAssemblyLoaded( string assemblyFullName )
        {
            foreach ( Assembly a in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( a.GetName().FullName == assemblyFullName )
                    return true;
            }
            return false;
        }
    }
}