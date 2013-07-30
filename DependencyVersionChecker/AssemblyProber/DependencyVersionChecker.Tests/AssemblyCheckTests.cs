using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace AssemblyProber.Tests
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

            TestAssembliesInfo( r.Assemblies );

            Assert.That( r.VersionConflicts.Count() >= 1, "At least one conflict was found" );
        }

        public static IList<IAssemblyInfo> GetReferencesFromThisAssembly()
        {
            IAssemblyLoader l = new AssemblyLoader( AssemblyLoader.DefaultBorderChecker );
            IAssemblyInfo assembly = null;
            ManualResetEventSlim waiter = new ManualResetEventSlim();

            FileInfo assemblyFile = new FileInfo( Assembly.GetExecutingAssembly().Location );
            //Environment.CurrentDirectory = assemblyFile.DirectoryName;

            assembly = l.LoadFromFile( assemblyFile );

            Assert.That( assembly, Is.Not.Null, "Entry assembly was correctly loaded" );

            return ListReferencedAssemblies( assembly );
        }

        public static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly )
        {
            return ListReferencedAssemblies( assembly, new List<IAssemblyInfo>() );
        }

        public static IList<IAssemblyInfo> ListReferencedAssemblies( IAssemblyInfo assembly, IList<IAssemblyInfo> existingAssemblies )
        {
            if ( !existingAssemblies.Contains( assembly ) )
                existingAssemblies.Add( assembly );

            foreach ( var pair in assembly.Dependencies )
            {
                IAssemblyInfo dep = pair.Value;
                ListReferencedAssemblies( dep, existingAssemblies );
            }

            return existingAssemblies;
        }

        public static void TestAssemblyInfo( IAssemblyInfo a )
        {
            Assert.That( a, Is.Not.Null, "Tested assembly is not null" );
            Assert.That( a.FullName, Is.Not.Null.Or.Empty, "Assembly full name exists and is not empty" );

            if ( a.Error == null )
            {
                Assert.That( a.SimpleName, Is.Not.Null.Or.Empty, "SimpleName exists and is not empty" );
                Assert.That( a.Version, Is.Not.Null, "Version is not null" );

                // Optional fields must exist, even if empty.
                Assert.That( a.Culture, Is.Not.Null, "Culture can be empty, but not null" );
                Assert.That( a.Company, Is.Not.Null, "Company can be empty, but not null" );
                Assert.That( a.Description, Is.Not.Null, "Description can be empty, but not null" );
                Assert.That( a.Product, Is.Not.Null, "Product can be empty, but not null" );
                Assert.That( a.Copyright, Is.Not.Null, "Copyright can be empty, but not null" );
                Assert.That( a.Trademark, Is.Not.Null, "Trademark can be empty, but not null" );
                Assert.That( a.FileVersion, Is.Not.Null, "FileVersion can be empty, but not null" );
                Assert.That( a.InformationalVersion, Is.Not.Null, "InformationalVersion can be empty, but not null" );

                CollectionAssert.IsNotEmpty( a.Paths, "Paths aren't empty" );

                CollectionAssert.AllItemsAreUnique( a.Paths, "All assembly paths are unique" );
                CollectionAssert.AllItemsAreUnique( a.Dependencies, "All references are unique" );

                if ( a.BorderName != null )
                {
                    Assert.That( a.Dependencies.Count() == 0, "Border assembly does not recurse into its references" );
                }

                if ( a.PublicKeyToken != null && a.PublicKeyToken.Length > 0 )
                {
                    Assert.That( a.PublicKeyToken.Length == 8, "PublicKeyToken is exactly 8 bytes" );
                }
            }
        }

        public static void TestAssembliesInfo( IEnumerable<IAssemblyInfo> assemblies )
        {
            foreach ( var a in assemblies )
            {
                TestAssemblyInfo( a );
            }
        }

        public static void TestAssemblyEquivalence( IAssemblyInfo a, IAssemblyInfo b )
        {
            TestAssemblyInfo( a );
            TestAssemblyInfo( b );

            Assert.That( a.FullName == b.FullName );

            if ( a.Error == null )
            {
                Assert.That( a.SimpleName == b.SimpleName );
                Assert.That( a.Version == b.Version );
                Assert.That( a.Culture == b.Culture );
                Assert.That( a.Description == b.Description );
                Assert.That( a.Company == b.Company );
                Assert.That( a.FileVersion == b.FileVersion );
                Assert.That( a.Copyright == b.Copyright );
                Assert.That( a.InformationalVersion == b.InformationalVersion );

                CollectionAssert.AreEquivalent( a.Paths, b.Paths );
                CollectionAssert.AreEqual( a.PublicKeyToken, b.PublicKeyToken );

                Assert.That( a.Dependencies.Count() == b.Dependencies.Count() );

                foreach ( var pair in a.Dependencies )
                {
                    string asReference = pair.Key;
                    IAssemblyInfo d = pair.Value;

                    Assert.That( b.Dependencies
                        .Where( d2 => d2.Key == pair.Key )
                        .Count() == 1 );

                    IAssemblyInfo e = b.Dependencies
                        .Where( d2 => d2.Key == pair.Key )
                        .Select( x => x.Value )
                        .First();

                    TestAssemblyEquivalence( d, e );
                }
            }
        }

        public static void TestAssembliesEquivalence( IEnumerable<IAssemblyInfo> aGroup, IEnumerable<IAssemblyInfo> bGroup )
        {
            Assert.That( aGroup.Count() == bGroup.Count() );

            foreach ( IAssemblyInfo a in aGroup )
            {
                Assert.That( bGroup
                    .Where( a2 => a2.FullName == a.FullName )
                    .Count() == 1 );

                IAssemblyInfo b = bGroup
                    .Where( a2 => a2.FullName == a.FullName )
                    .First();

                TestAssemblyEquivalence( a, b );
            }
        }
    }
}