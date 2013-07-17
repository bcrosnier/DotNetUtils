using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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


            IAssemblyLoader loader = new AssemblyLoader();

            List<IAssemblyInfo> Files = new List<IAssemblyInfo>();


            CountdownEvent countdown = new CountdownEvent( fileList.Count() );

            EventHandler<AssemblyLoadingCompleteEventArgs> OnAssemblyComplete =
                delegate( object s, AssemblyLoadingCompleteEventArgs e )
                {
                    if( e.ResultingAssembly != null )
                        Files.Add( e.ResultingAssembly );
                    countdown.Signal();
                };

            loader.AsyncAssemblyLoaded += OnAssemblyComplete;

            foreach( var f in fileList )
            {
                loader.LoadFromFileAsync( f );
            }

            countdown.Wait();

            loader.AsyncAssemblyLoaded -= OnAssemblyComplete;

            CollectionAssert.AllItemsAreUnique( Files );
            CollectionAssert.AllItemsAreNotNull( Files );

            IAssemblyInfo executingAssembly;
            IAssemblyInfo dependencyAssembly;

            executingAssembly =
                Files
                .Where( x => x.AssemblyName == Assembly.GetExecutingAssembly().GetName().Name )
                .Select( x => x )
                .FirstOrDefault();

            dependencyAssembly =
                Files
                .Where( x => x.AssemblyName == loader.GetType().Assembly.GetName().Name )
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

            IAssemblyLoader loader = new AssemblyLoader();
            IAssemblyInfo info = loader.LoadFromFile( new FileInfo( pathToExternalAssembly ) );

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

        [Test]
        public void AsyncAssemblyLoading()
        {
        }
    }
}