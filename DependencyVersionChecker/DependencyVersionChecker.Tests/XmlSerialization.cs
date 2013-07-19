using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NUnit.Framework;

namespace DependencyVersionChecker.Tests
{
    [TestFixture]
    public class XmlSerialization
    {
        [Test]
        public void XmlSerializeDeserialize()
        {
            AssemblyInfo[] assemblies = AssemblyCheckTests.GetReferencesFromThisAssembly();
            AssemblyInfo[] assemblies1;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count(), Is.GreaterThan(1), "Test assembly references at least 1 other assembly" );

            XmlSerializer serializer = new XmlSerializer( typeof( SerializableAssemblyInfoSet ), new Type[]{typeof(AssemblyInfo)} );

            SerializableAssemblyInfoSet set = new SerializableAssemblyInfoSet();

            set.Assemblies = (AssemblyInfo[])assemblies;

            using ( MemoryStream ms = new MemoryStream() )
            {
                serializer.Serialize( ms, set );
                ms.Position = 0;

                SerializableAssemblyInfoSet set1 = serializer.Deserialize( ms ) as SerializableAssemblyInfoSet;
                assemblies1 = set1.Assemblies;
            }

            Assert.That( assemblies1, Is.Not.Null );
            Assert.That( assemblies1.Count() == assemblies.Count() );

            for ( int i = 0; i < assemblies.Count(); i++ )
            {
                AssemblyInfo deserializedAssembly = assemblies1[i];
                AssemblyInfo initialAssembly = assemblies
                    .Where( x => x.AssemblyFullName == assemblies1[i].AssemblyFullName )
                    .FirstOrDefault();

                Assert.That( initialAssembly, Is.Not.Null, "Initial assembly matches re-serialized assembly full name" );

                Assert.That( initialAssembly.Version == deserializedAssembly.Version,
                    "Initial assembly matches re-serialized assembly Version" );

                Assert.That( initialAssembly.FileVersion == deserializedAssembly.FileVersion,
                    "Initial assembly matches re-serialized assembly FileVersion" );

                Assert.That( initialAssembly.InformationalVersion == deserializedAssembly.InformationalVersion,
                    "Initial assembly matches re-serialized assembly InformationalVersion" );

                Assert.That( initialAssembly.Culture == deserializedAssembly.Culture,
                    "Initial assembly matches re-serialized assembly Culture" );

                Assert.That( initialAssembly.SimpleName == deserializedAssembly.SimpleName,
                    "Initial assembly matches re-serialized assembly SimpleName" );

                foreach( var deserializedDependency in deserializedAssembly.Dependencies )
                {
                    IAssemblyInfo initialDependency = initialAssembly.Dependencies
                        .Where( x => x.AssemblyFullName == deserializedDependency.AssemblyFullName )
                        .FirstOrDefault();


                    Assert.That( initialAssembly, Is.Not.Null, "Initial dependency matches re-serialized dependency" );
                }
            }
        }
    }
}