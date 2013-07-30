using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace AssemblyProber.Tests
{
    [TestFixture]
    public class XmlSerializationTests
    {
        [Test]
        public void XmlDocumentSerializeDeserialize()
        {
            List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>( AssemblyCheckTests.GetReferencesFromThisAssembly() );
            List<IAssemblyInfo> assemblies2;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count, Is.GreaterThan( 1 ), "Test assembly references at least 1 other assembly" );

            AssemblyCheckTests.TestAssembliesInfo( assemblies );

            XmlDocument serialized = AssemblyInfoXmlSerializer.SerializeToDocument( assemblies );

            assemblies2 = AssemblyInfoXmlSerializer.DeserializeFromDocument( serialized ).ToList();

            CollectionAssert.IsNotEmpty( assemblies2, "Deserialized collection is not empty" );

            AssemblyCheckTests.TestAssembliesEquivalence( assemblies, assemblies2 );
        }

        [Test]
        public void XmlWriterSerializeDocumentDeserialize()
        {
            List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>( AssemblyCheckTests.GetReferencesFromThisAssembly() );
            List<IAssemblyInfo> assemblies2;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count, Is.GreaterThan( 1 ), "Test assembly references at least 1 other assembly" );

            AssemblyCheckTests.TestAssembliesInfo( assemblies );

            using ( MemoryStream ms = new MemoryStream() )
            {
                using ( XmlWriter w = XmlWriter.Create( ms ) )
                {
                    AssemblyInfoXmlSerializer.SerializeTo( assemblies, w );
                }

                ms.Seek( 0, System.IO.SeekOrigin.Begin );

                XmlDocument d = new XmlDocument();
                d.Load( ms );

                assemblies2 = AssemblyInfoXmlSerializer.DeserializeFromDocument( d ).ToList();
            }

            CollectionAssert.IsNotEmpty( assemblies2, "Deserialized collection is not empty" );

            AssemblyCheckTests.TestAssembliesEquivalence( assemblies, assemblies2 );
        }

        [Test]
        public void XmlDocumentSerializeReaderDeserialize()
        {
            List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>( AssemblyCheckTests.GetReferencesFromThisAssembly() );
            List<IAssemblyInfo> assemblies2;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count, Is.GreaterThan( 1 ), "Test assembly references at least 1 other assembly" );

            AssemblyCheckTests.TestAssembliesInfo( assemblies );

            XmlDocument serialized = AssemblyInfoXmlSerializer.SerializeToDocument( assemblies );

            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.None;

            XmlReaderSettings rs = new XmlReaderSettings();

            using ( MemoryStream ms = new MemoryStream() )
            {
                using ( XmlWriter xw = XmlWriter.Create( ms, ws ) )
                {
                    serialized.WriteContentTo( xw );
                }

                // Debug string
                //ms.Seek( 0, System.IO.SeekOrigin.Begin );
                //using( StreamReader sr = new StreamReader( ms ) )
                //{
                //    string s = sr.ReadToEnd();
                //}

                ms.Seek( 0, System.IO.SeekOrigin.Begin );

                using ( XmlReader r = XmlReader.Create( ms, rs ) )
                {
                    assemblies2 = AssemblyInfoXmlSerializer.DeserializeFrom( r ).ToList();
                }
            }

            CollectionAssert.IsNotEmpty( assemblies2, "Deserialized collection is not empty" );

            AssemblyCheckTests.TestAssembliesEquivalence( assemblies, assemblies2 );
        }

        [Test]
        public void XmlWriterSerializeReaderDeserialize()
        {
            List<IAssemblyInfo> assemblies = new List<IAssemblyInfo>( AssemblyCheckTests.GetReferencesFromThisAssembly() );
            List<IAssemblyInfo> assemblies2;

            Assert.That( assemblies, Is.Not.Null, "Test assembly list was returned" );
            Assert.That( assemblies.Count, Is.GreaterThan( 1 ), "Test assembly references at least 1 other assembly" );

            AssemblyCheckTests.TestAssembliesInfo( assemblies );

            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.None;

            XmlReaderSettings rs = new XmlReaderSettings();

            using ( MemoryStream ms = new MemoryStream() )
            {
                using ( XmlWriter xw = XmlWriter.Create( ms, ws ) )
                {
                    AssemblyInfoXmlSerializer.SerializeTo( assemblies, xw );
                }

                ms.Seek( 0, System.IO.SeekOrigin.Begin );

                // Debug string
                //using( StreamReader sr = new StreamReader( ms ) )
                //{
                //    string s = sr.ReadToEnd();
                //}

                using ( XmlReader r = XmlReader.Create( ms, rs ) )
                {
                    assemblies2 = AssemblyInfoXmlSerializer.DeserializeFrom( r ).ToList();
                }
            }

            CollectionAssert.IsNotEmpty( assemblies2, "Deserialized collection is not empty" );

            AssemblyCheckTests.TestAssembliesEquivalence( assemblies, assemblies2 );
        }
    }
}