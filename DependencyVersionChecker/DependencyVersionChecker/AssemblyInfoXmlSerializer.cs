using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyVersionChecker
{
    public static class AssemblyInfoXmlSerializer
    {
        public static XmlDocument Serialize( IEnumerable<IAssemblyInfo> assemblies )
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration( "1.0", "UTF-8", null );
            doc.AppendChild( docNode );

            XmlNode assembliesNode = doc.CreateElement( "Assemblies" );

            foreach ( IAssemblyInfo a in assemblies )
            {
                assembliesNode.AppendChild( GetAssemblyNode( a, doc ) );
            }
            doc.AppendChild( assembliesNode );
            return doc;
        }

        private static XmlNode GetAssemblyNode( IAssemblyInfo a, XmlDocument doc )
        {
            XmlNode assemblyNode = doc.CreateElement( "AssemblyInfo" );

            XmlElement assemblyFullName = doc.CreateElement( "AssemblyFullName" );
            assemblyFullName.AppendChild( doc.CreateTextNode( a.AssemblyFullName ) );
            assemblyNode.AppendChild( assemblyFullName );

            XmlElement simpleName = doc.CreateElement( "SimpleName" );
            simpleName.AppendChild( doc.CreateTextNode( a.SimpleName ) );
            assemblyNode.AppendChild( simpleName );

            XmlElement version = doc.CreateElement( "Version" );
            version.AppendChild( doc.CreateTextNode( a.Version.ToString() ) );
            assemblyNode.AppendChild( version );

            XmlElement culture = doc.CreateElement( "Culture" );
            culture.AppendChild( doc.CreateTextNode( a.Culture ) );
            assemblyNode.AppendChild( culture );

            XmlElement fileVersion = doc.CreateElement( "FileVersion" );
            fileVersion.AppendChild( doc.CreateTextNode( a.FileVersion ) );
            assemblyNode.AppendChild( fileVersion );

            XmlElement informationalVersion = doc.CreateElement( "InformationalVersion" );
            informationalVersion.AppendChild( doc.CreateTextNode( a.InformationalVersion ) );
            assemblyNode.AppendChild( informationalVersion );

            XmlElement description = doc.CreateElement( "Description" );
            description.AppendChild( doc.CreateTextNode( a.Description ) );
            assemblyNode.AppendChild( description );

            XmlElement borderName = doc.CreateElement( "BorderName" );
            borderName.AppendChild( doc.CreateTextNode( a.BorderName ) );
            assemblyNode.AppendChild( borderName );

            XmlElement paths = doc.CreateElement( "Paths" );
            foreach ( string p in a.Paths )
            {
                XmlElement path = doc.CreateElement( "Path" );
                path.AppendChild( doc.CreateTextNode( p ) );
                paths.AppendChild( path );
            }
            assemblyNode.AppendChild( paths );

            XmlElement dependencies = doc.CreateElement( "Dependencies" );
            foreach ( IAssemblyInfo d in a.Dependencies )
            {
                XmlElement reference = doc.CreateElement( "Reference" );
                reference.AppendChild( doc.CreateTextNode( d.AssemblyFullName ) );
                dependencies.AppendChild( reference );
            }
            assemblyNode.AppendChild( dependencies );

            return assemblyNode;
        }

        public static IEnumerable<IAssemblyInfo> Deserialize( XmlDocument doc )
        {
            Dictionary<string, AssemblyInfo> assemblies = new Dictionary<string, AssemblyInfo>();

            foreach ( XmlNode n in doc.DocumentElement )
            {
                AssemblyInfo a = GetInfoFromNode( n );
                assemblies.Add( a.AssemblyFullName, a );
            }

            foreach ( XmlNode n in doc.DocumentElement )
            {
                AssemblyInfo a = assemblies[n["AssemblyFullName"].FirstChild.Value];
                foreach ( XmlNode r in n["Dependencies"] )
                {
                    AssemblyInfo d = assemblies[r.FirstChild.Value];
                    a.InternalDependencies.Add( d );
                }
            }

            return assemblies.Values;
        }

        public static AssemblyInfo GetInfoFromNode( XmlNode n )
        {
            AssemblyInfo a = new AssemblyInfo();

            a.AssemblyFullName = n["AssemblyFullName"].FirstChild.Value;
            a.SimpleName = n["SimpleName"].FirstChild.Value;
            a.Version = Version.Parse( n["Version"].FirstChild.Value );
            a.Culture = n["Culture"].FirstChild.Value;
            a.FileVersion = n["FileVersion"].FirstChild.Value;
            a.InformationalVersion = n["InformationalVersion"].FirstChild.Value;
            a.Description = n["Description"].FirstChild.Value;
            a.BorderName = String.IsNullOrEmpty( n["BorderName"].FirstChild.Value ) ? null : n["BorderName"].FirstChild.Value;

            foreach ( XmlNode p in n["Paths"] )
            {
                a.Paths.Add( p.FirstChild.Value );
            }
            return a;
        }
    }
}