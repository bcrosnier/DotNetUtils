using System;
using System.Collections.Generic;
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

            XmlElement assemblyFullName = doc.CreateElement( "FullName" );
            assemblyFullName.AppendChild( doc.CreateTextNode( a.FullName ) );
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

            XmlElement company = doc.CreateElement( "Company" );
            company.AppendChild( doc.CreateTextNode( a.Company ) );
            assemblyNode.AppendChild( company );

            XmlElement product = doc.CreateElement( "Product" );
            product.AppendChild( doc.CreateTextNode( a.Product ) );
            assemblyNode.AppendChild( product );

            XmlElement trademark = doc.CreateElement( "Trademark" );
            trademark.AppendChild( doc.CreateTextNode( a.Trademark ) );
            assemblyNode.AppendChild( trademark );

            XmlElement copyright = doc.CreateElement( "Copyright" );
            copyright.AppendChild( doc.CreateTextNode( a.Copyright ) );
            assemblyNode.AppendChild( copyright );

            XmlElement borderName = doc.CreateElement( "BorderName" );
            borderName.AppendChild( doc.CreateTextNode( a.BorderName ) );
            assemblyNode.AppendChild( borderName );

            XmlElement publicKeyToken = doc.CreateElement( "PublicKeyToken" );
            publicKeyToken.AppendChild( doc.CreateTextNode( DependencyUtils.ByteArrayToHexString( a.PublicKeyToken ) ) );
            assemblyNode.AppendChild( publicKeyToken );

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
                reference.AppendChild( doc.CreateTextNode( d.FullName ) );
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
                assemblies.Add( a.FullName, a );
            }

            foreach ( XmlNode n in doc.DocumentElement )
            {
                AssemblyInfo a = assemblies[n["FullName"].FirstChild.Value];
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

            a.FullName = n["FullName"].FirstChild.Value;
            a.SimpleName = n["SimpleName"].FirstChild.Value;
            a.Version = Version.Parse( n["Version"].FirstChild.Value );

            a.Culture = n["Culture"].FirstChild == null ? String.Empty : n["Culture"].FirstChild.Value;

            a.FileVersion = n["FileVersion"].FirstChild == null ? String.Empty : n["FileVersion"].FirstChild.Value;
            a.InformationalVersion = n["InformationalVersion"].FirstChild == null ? String.Empty : n["InformationalVersion"].FirstChild.Value;
            a.Description = n["Description"].FirstChild == null ? String.Empty : n["Description"].FirstChild.Value;

            a.Company = n["Company"].FirstChild == null ? String.Empty : n["Company"].FirstChild.Value;
            a.Product = n["Product"].FirstChild == null ? String.Empty : n["Product"].FirstChild.Value;
            a.Trademark = n["Trademark"].FirstChild == null ? String.Empty : n["Trademark"].FirstChild.Value;
            a.Copyright = n["Copyright"].FirstChild == null ? String.Empty : n["Copyright"].FirstChild.Value;

            a.PublicKeyToken = n["PublicKeyToken"].FirstChild == null ? null : DependencyUtils.HexStringToByteArray( n["PublicKeyToken"].FirstChild.Value );

            if ( n["BorderName"].FirstChild == null || String.IsNullOrEmpty( n["BorderName"].FirstChild.Value ) )
            {
                a.BorderName = null;
            }
            else
            {
                a.BorderName = n["BorderName"].FirstChild.Value;
            }

            foreach ( XmlNode p in n["Paths"] )
            {
                a.Paths.Add( p.FirstChild.Value );
            }
            return a;
        }
    }
}