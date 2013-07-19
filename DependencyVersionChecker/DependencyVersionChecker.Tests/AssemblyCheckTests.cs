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

            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );

            AssemblyVersionChecker checker = new AssemblyVersionChecker( l );

            Environment.CurrentDirectory = assemblyDirectory;
            checker.AddDirectory( new DirectoryInfo( assemblyDirectory ), true );
            
            var r = checker.Check();

            Assert.That( r.VersionConflicts.Count() >= 1, "At least one conflict was found" );
        }

        public static IAssemblyInfo[] GetReferencesFromThisAssembly()
        {
            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );
            IAssemblyInfo assembly = null;
            ManualResetEventSlim waiter = new ManualResetEventSlim();

            FileInfo assemblyFile = new FileInfo( Assembly.GetExecutingAssembly().Location );
            Environment.CurrentDirectory = assemblyFile.DirectoryName;

            assembly = l.LoadFromFile( assemblyFile );


            Assert.That( assembly, Is.Not.Null, "Entry assembly was correctly loaded" );

            return ListReferencedAssemblies( assembly ).ToArray();
        }

        public static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly )
        {
            return ListReferencedAssemblies( assembly, new List<IAssemblyInfo>() );
        }

        public static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly, IList<IAssemblyInfo> existingAssemblies )
        {
            if( !existingAssemblies.Contains( assembly ) )
                existingAssemblies.Add( assembly );

            foreach( IAssemblyInfo dep in assembly.Dependencies )
            {
                ListReferencedAssemblies( dep, existingAssemblies );
            }

            return existingAssemblies;
        }
    }
}