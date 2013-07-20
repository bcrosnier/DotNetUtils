using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace DependencyVersionChecker.Tests
{
    [TestFixture]
    public class XmlSerialization
    {
        [Test]
        public void XmlSerializeDeserialize()
        {
            List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>( AssemblyCheckTests.GetReferencesFromThisAssembly() );
            List<IAssemblyInfo> assemblies2;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count, Is.GreaterThan( 1 ), "Test assembly references at least 1 other assembly" );

            AssemblyCheckTests.TestAssembliesInfo( assemblies );
            AssemblyCheckTests.TestAssembliesInfo( assemblies );
            AssemblyCheckTests.TestAssembliesInfo( assemblies );

            XmlDocument serialized = AssemblyInfoXmlSerializer.Serialize( assemblies );

            assemblies2 = AssemblyInfoXmlSerializer.Deserialize( serialized ).ToList();

            CollectionAssert.IsNotEmpty( assemblies2, "Deserialized collection is not empty" );

            AssemblyCheckTests.TestAssembliesEquivalence( assemblies, assemblies2 );
        }
    }
}