using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }
}