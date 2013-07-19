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
    public class AssemblyCheckTests
    {
        [Test]
        public void CheckExternalAssemblies()
        {
            string assemblyDirectory = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "..", "..", "ExternalAssemblies" );

            Assert.That( Directory.Exists( assemblyDirectory ), "External assembly directory exists" );

            IAssemblyLoader l = new AssemblyLoader();

            AssemblyVersionChecker checker = new AssemblyVersionChecker( l );

            Environment.CurrentDirectory = assemblyDirectory;
            checker.AddDirectory( new DirectoryInfo( assemblyDirectory ), true );

            IEnumerable<DependencyAssembly> conflicts = null;

            ManualResetEventSlim waiter = new ManualResetEventSlim();
            checker.AssemblyCheckComplete += ( s, e ) => { conflicts = e.VersionConflicts; waiter.Set(); };
            checker.Check();

            waiter.Wait();

            Assert.That( conflicts.Count() >= 1, "At least one conflict was found" );
        }

        public static AssemblyInfo[] GetReferencesFromThisAssembly()
        {
            IAssemblyLoader l = new AssemblyLoader();
            AssemblyInfo assembly = null;
            ManualResetEventSlim waiter = new ManualResetEventSlim();

            l.AsyncAssemblyLoaded += ( s, e ) => { assembly = (AssemblyInfo) e.ResultingAssembly; waiter.Set(); };

            FileInfo assemblyFile = new FileInfo( Assembly.GetExecutingAssembly().Location );
            Environment.CurrentDirectory = assemblyFile.DirectoryName;

            l.LoadFromFileAsync( assemblyFile );

            waiter.Wait();

            Assert.That( assembly, Is.Not.Null, "Entry assembly was correctly loaded" );

            return ListReferencedAssemblies( assembly ).ToArray();
        }

        public static IList<AssemblyInfo> ListReferencedAssemblies( AssemblyInfo assembly )
        {
            return ListReferencedAssemblies( assembly, new List<AssemblyInfo>() );
        }

        public static IList<AssemblyInfo> ListReferencedAssemblies( AssemblyInfo assembly, IList<AssemblyInfo> existingAssemblies )
        {
            if ( !existingAssemblies.Contains( assembly ) )
                existingAssemblies.Add( assembly );

            foreach ( AssemblyInfo dep in assembly.Dependencies )
            {
                ListReferencedAssemblies( dep, existingAssemblies );
            }

            return existingAssemblies;
        }
    }
}