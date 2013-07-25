using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Extension utilities for SolutionCheckResult
    /// </summary>
    public static class SolutionCheckResultExtensions
    {
        /// <summary>
        /// Serializes a SolutionCheckResult to a XmlWriter
        /// </summary>
        /// <param name="result">Object to serialize</param>
        /// <param name="w">XmlWriter to use</param>
        public static void SerializeTo( this SolutionCheckResult result, XmlWriter w )
        {

            w.WriteStartElement( "SolutionCheckResult" );

            w.WriteStartAttribute( "Path" );
            w.WriteValue( result.SolutionPath );
            w.WriteEndAttribute();

            w.WriteStartElement( "PackageVersionMismatches" );
            foreach( var pair in result.PackagesWithMultipleVersions )
            {
                WritePackageVersionMismatches( pair, result, w );
            }
            w.WriteEndElement();

            w.WriteStartElement( "Projects" );
            foreach( ISolutionProjectItem projectItem in result.Projects.OrderBy( x => x.ProjectName ) )
            {
                WriteProjectItem( projectItem, result.ProjectAssemblyReferences[projectItem], result.ProjectNugetReferences[projectItem], w );
            }
            w.WriteEndElement();

            w.WriteStartElement( "NuGetPackages" );
            foreach( IPackage package in result.NuGetPackages.OrderBy( x => x.Id ).ThenBy( x => x.Version ) )
            {
                WriteNuGetPackage( package, w );
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WritePackageVersionMismatches( KeyValuePair<string, IEnumerable<IPackage>> pair, SolutionCheckResult result, XmlWriter w )
        {
            w.WriteStartElement( "PackageVersionMismatch" );

            w.WriteStartAttribute( "PackageName" );
            w.WriteValue( pair.Key );
            w.WriteEndAttribute();

            foreach( IPackage p in pair.Value.OrderBy( x => x.Id ) )
            {
                string packageIdentifier = p.Id + '.' + p.Version.ToString();

                w.WriteStartElement( "ProjectsReferencing" );

                w.WriteStartAttribute( "PackageVersion" );
                w.WriteValue( p.Version.ToString() );
                w.WriteEndAttribute();

                foreach( ISolutionProjectItem i in result.GetProjectsReferencing( packageIdentifier ).OrderBy( x => x.ProjectName ) )
                {
                    w.WriteStartElement( "Project" );

                    w.WriteStartAttribute( "Name" );
                    w.WriteValue( i.ProjectName );
                    w.WriteEndAttribute();

                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        private static void WriteNuGetPackage( IPackage package, XmlWriter w )
        {
            w.WriteStartElement( "NuGetPackage" );

            w.WriteStartAttribute( "Id" );
            w.WriteValue( package.Id );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "Version" );
            w.WriteValue( package.Version.ToString() );
            w.WriteEndAttribute();

            w.WriteStartElement( "Title" );
            w.WriteValue( package.Title );
            w.WriteEndElement();

            w.WriteStartElement( "Description" );
            w.WriteValue( package.Description );
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WriteProjectItem( ISolutionProjectItem projectItem, IEnumerable<INuGetPackageAssemblyReference> assemblyRefs,
            IEnumerable<INuGetPackageReference> packageRefs, XmlWriter w )
        {
            w.WriteStartElement( "Project" );

            w.WriteStartAttribute( "Name" );
            w.WriteValue( projectItem.ProjectName );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "Path" );
            w.WriteValue( projectItem.ProjectPath );
            w.WriteEndAttribute();

            w.WriteStartElement( "NuGetPackageReferences" );
            if( packageRefs != null )
            {
                foreach( INuGetPackageReference packageRef in packageRefs.OrderBy( x => x.Id ) )
                {
                    WriteNugetReference( packageRef, w );
                }
            }
            w.WriteEndElement();

            w.WriteStartElement( "NuGetAssemblyReferences" );
            foreach( INuGetPackageAssemblyReference assemblyRef in assemblyRefs.OrderBy( x => x.PackageIdVersion ) )
            {
                WriteNugetAssemblyReference( assemblyRef, w );
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WriteNugetAssemblyReference( INuGetPackageAssemblyReference assemblyRef, XmlWriter w )
        {
            w.WriteStartElement( "NuGetAssemblyRef" );

            w.WriteStartAttribute( "Name" );
            w.WriteValue( assemblyRef.AssemblyFileName );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "PackageId" );
            w.WriteValue( assemblyRef.PackageIdVersion );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "Path" );
            w.WriteValue( assemblyRef.FullPath );
            w.WriteEndAttribute();

            w.WriteEndElement();
        }

        private static void WriteNugetReference( INuGetPackageReference packageRef, XmlWriter w )
        {
            w.WriteStartElement( "NuGetPackageRef" );

            w.WriteStartAttribute( "Id" );
            w.WriteValue( packageRef.Id );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "Version" );
            w.WriteValue( packageRef.Version );
            w.WriteEndAttribute();

            if( packageRef.TargetFramework != null )
            {
                w.WriteStartAttribute( "TargetFramework" );
                w.WriteValue( VersionUtility.GetShortFrameworkName( packageRef.TargetFramework ) );
                w.WriteEndAttribute();
            }

            w.WriteEndElement();
        }
    }
}
