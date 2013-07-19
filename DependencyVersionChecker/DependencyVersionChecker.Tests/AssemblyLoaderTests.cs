using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace DependencyVersionChecker.Tests
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

            foreach( var f in fileList )
            {
                Files.Add( l.LoadFromFile( f ) );
            }

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
                executingAssembly.Dependencies,
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

            Assert.That( IsAssemblyLoaded( info.AssemblyFullName ), Is.False, "External assembly wasn't loaded when reflecting" );
        }

        private bool IsAssemblyLoaded( string assemblyFullName )
        {
            foreach( Assembly a in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if( a.GetName().FullName == assemblyFullName )
                    return true;
            }
            return false;
        }
    }
}