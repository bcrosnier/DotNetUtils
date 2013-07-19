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
            IAssemblyInfo[] assemblies = AssemblyCheckTests.GetReferencesFromThisAssembly();
            IAssemblyInfo[] assemblies1;

            XmlSerializer serializer = new XmlSerializer( typeof( SerializableAssemblyInfoSet ) );

            SerializableAssemblyInfoSet set = new SerializableAssemblyInfoSet();

            set.Assemblies = (AssemblyInfo[])assemblies;

            using ( MemoryStream ms = new MemoryStream() )
            {
                serializer.Serialize( ms, assemblies );
                ms.Position = 0;

                SerializableAssemblyInfoSet set1 = serializer.Deserialize( ms ) as SerializableAssemblyInfoSet;
                assemblies1 = (IAssemblyInfo[])set1.Assemblies;
            }

            Assert.That( assemblies1, Is.Not.Null );
            Assert.That( assemblies1.Count() == assemblies.Count() );

            for ( int i = 0; i < assemblies.Count(); i++ )
            {
                Assert.That( assemblies
                    .Where( x => x.AssemblyFullName == assemblies1[i].AssemblyFullName )
                    .Count() == 1
                    );
            }
        }
    }
}