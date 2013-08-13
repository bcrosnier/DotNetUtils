using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProjectProber.Impl;

namespace ProjectProber.SerializationExtensions
{
    public static class AssemblyVersionInfoCheckResultExtensions
    {
        /// <summary>
        /// Serializes a AssemblyVersionInfoCheckResult to a XmlWriter
        /// </summary>
        /// <param name="result">Object to serialize</param>
        /// <param name="w">XmlWriter to use</param>
        public static void SerializeTo( this AssemblyVersionInfoCheckResult result, XmlWriter w )
        {
            w.WriteStartElement( "AssemblyVersionInfoCheckResult" );
                w.WriteAttributeString( "solution", Path.GetFileName( result.SolutionFilePath ) );
                w.WriteAttributeString( "solutionpath", result.SolutionFilePath );

            WriteVersionErrors( result, w );

            WriteSharedAssemblyInfoVersions( result.SharedAssemblyInfoVersions, w );

            WriteCSProjCompileLinkInfo( result.CsProjs, w );

            WriteAssemblyInfoVersions( result.AssemblyVersions, w );

            w.WriteEndElement();
        }

        private static void WriteVersionErrors( AssemblyVersionInfoCheckResult result, XmlWriter w )
        {
            w.WriteStartElement( "VersionErrors" );

            w.WriteStartElement( "HasAssemblyInfoWithVersion" );
            w.WriteValue( result.HasAssemblyInfoWithVersion );
            w.WriteEndElement();

            w.WriteStartElement( "HasFileWithoutVersion" );
            w.WriteValue( result.HasFileWithoutVersion );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleAssemblyFileVersion" );
            w.WriteValue( result.HasMultipleAssemblyFileVersion );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleAssemblyInformationVersion" );
            w.WriteValue( result.HasMultipleAssemblyInformationVersion );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleAssemblyVersion" );
            w.WriteValue( result.HasMultipleAssemblyVersion );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleRelativeLinkInCSProj" );
            w.WriteValue( result.HasMultipleRelativeLinkInCSProj );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleSharedAssemblyInfo" );
            w.WriteValue( result.HasMultipleSharedAssemblyInfo );
            w.WriteEndElement();

            w.WriteStartElement( "HasMultipleVersionInOneAssemblyInfo" );
            w.WriteValue( result.HasMultipleVersionInOneAssemblyInfo );
            w.WriteEndElement();

            w.WriteStartElement( "HasNotSharedAssemblyInfo" );
            w.WriteValue( result.HasNotSharedAssemblyInfo );
            w.WriteEndElement();

            w.WriteStartElement( "HasOneVersionNotSemanticVersionCompliant" );
            w.WriteValue( result.HasOneVersionNotSemanticVersionCompliant );
            w.WriteEndElement();

            w.WriteStartElement( "HasRelativeLinkInCSProjNotFound" );
            w.WriteValue( result.HasRelativeLinkInCSProjNotFound );
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WriteSharedAssemblyInfoVersions( IReadOnlyList<AssemblyVersionInfo> sharedAssemblyInfo, XmlWriter w )
        {
            w.WriteStartElement( "SharedAssemblyInfo" );
                foreach( AssemblyVersionInfo assemblyVersionInfo in sharedAssemblyInfo.OrderBy( x => x.AssemblyInfoFilePath ) )
                {
                    WriteAssemblyVersions( assemblyVersionInfo, w );
                }
            w.WriteEndElement();
        }

        private static void WriteAssemblyInfoVersions( IReadOnlyList<AssemblyVersionInfo> AssemblyInfo, XmlWriter w )
        {
            w.WriteStartElement( "AssemblyInfo" );
                foreach( AssemblyVersionInfo assemblyVersionInfo in AssemblyInfo.OrderBy( x => x.AssemblyInfoFilePath ) )
                {
                    WriteAssemblyVersions( assemblyVersionInfo, w );
                }
            w.WriteEndElement();
        }

        private static void WriteCSProjCompileLinkInfo( IReadOnlyList<CSProjCompileLinkInfo> csProjCompileLinkInfo, XmlWriter w )
        {
            w.WriteStartElement( "AssemblyInfo" );
                foreach( CSProjCompileLinkInfo csProjInfo in csProjCompileLinkInfo.OrderBy( x => x.ProjectName ) )
                {
                    WriteCsProjInfo( csProjInfo, w );
                }
            w.WriteEndElement();
        }

        private static void WriteAssemblyVersions( AssemblyVersionInfo assemblyVersions, XmlWriter w )
        {
            w.WriteStartElement( "AssemblyVersions" );

            w.WriteStartAttribute( "version" );
            if( assemblyVersions.AssemblyVersion != null )
                w.WriteValue( assemblyVersions.AssemblyVersion.ToString() );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "fileversion" );
            if( assemblyVersions.AssemblyFileVersion != null )
                w.WriteValue( assemblyVersions.AssemblyFileVersion.ToString() );
            w.WriteEndAttribute();

            w.WriteStartAttribute( "informationalversion" );
            if( !string.IsNullOrEmpty( assemblyVersions.AssemblyInformationalVersion ) )
                w.WriteValue( assemblyVersions.AssemblyInformationalVersion );
            w.WriteEndAttribute();

            w.WriteStartElement( "Path" );
            if( !string.IsNullOrEmpty( assemblyVersions.AssemblyInfoFilePath ) )
                w.WriteValue( assemblyVersions.AssemblyInfoFilePath );
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void WriteCsProjInfo( CSProjCompileLinkInfo csProjCompileLinkInfo, XmlWriter w )
        {
            w.WriteStartElement( "CSProjCompileLinkInfo" );

            w.WriteStartAttribute( "projectname" );
            if( !string.IsNullOrEmpty( csProjCompileLinkInfo.ProjectName ) )
                w.WriteValue( csProjCompileLinkInfo.ProjectName );
            w.WriteEndAttribute();

            w.WriteStartElement( "RelativePath" );
            if( !string.IsNullOrEmpty( csProjCompileLinkInfo.SharedAssemblyInfoRelativePath ) )
                w.WriteValue( csProjCompileLinkInfo.SharedAssemblyInfoRelativePath );
            w.WriteEndElement();

            w.WriteStartElement( "PropertiesFilePath" );
            if( !string.IsNullOrEmpty( csProjCompileLinkInfo.AssociateLink ) )
                w.WriteValue( csProjCompileLinkInfo.AssociateLink );
            w.WriteEndElement();

            w.WriteEndElement();
        }
    }
}
