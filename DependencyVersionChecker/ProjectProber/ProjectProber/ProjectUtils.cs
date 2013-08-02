using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml;
using NuGet;
using ProjectProber.Impl;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Static utilities to handle parsing of projects, and project references.
    /// </summary>
    public static class ProjectUtils
    {
        /// <summary>
        /// Gets HintPath of all references in a project file having one.
        /// </summary>
        /// <param name="projectPath">Project file to use</param>
        /// <returns>Collection of assembly paths, relative to the project directory.</returns>
        public static IEnumerable<string> GetPackageAssemblyReferencePaths( string projectPath )
        {
            IEnumerable<IProjectReference> references = GetReferencesFromProjectFile( projectPath );

            var hintPaths = references
                .Where( i => i.HintPath != null )
                .Select( i => i.HintPath );

            return hintPaths;
        }

        /// <summary>
        /// Find and open a particular NuGet package using a package reference
        /// </summary>
        /// <param name="packageReference">Package reference to use</param>
        /// <param name="packageRoot">Root of the NuGet package directory, which contains all NuGet packages</param>
        /// <returns>Opened NuGet package information</returns>
        public static NuGet.IPackage GetPackageFromReference( INuGetPackageReference packageReference, string packageRoot )
        {
            string packageIdentifier = packageReference.Id + '.' + packageReference.Version;

            return GetPackageFromReference( packageIdentifier, packageRoot );
        }

        /// <summary>
        /// Find and open a particular NuGet package using a package packageIdentifier
        /// </summary>
        /// <param name="packageIdentifier">Package identifier to use</param>
        /// <param name="packageRoot">Root of the NuGet package directory, which contains all NuGet packages</param>
        /// <returns>Opened NuGet package information</returns>
        public static NuGet.IPackage GetPackageFromReference( string packageIdentifier, string packageRoot )
        {
            string packageFile = Path.Combine( packageRoot, packageIdentifier, packageIdentifier + ".nupkg" );

            Debug.Assert( File.Exists( packageFile ), "Package file was found" );

            NuGet.IPackage package = new NuGet.ZipPackage( packageFile );

            return package;
        }

        /// <summary>
        /// Parses a NuGet package configuration file (packages.config), and gets its references.
        /// </summary>
        /// <param name="configFile">Path to the configuration file, usually packages.config</param>
        /// <returns>Collection of new INuGetPackageReferences</returns>
        public static IEnumerable<INuGetPackageReference> GetReferencesFromPackageConfig( string configFile )
        {
            if( string.IsNullOrEmpty( configFile ) )
                throw new ArgumentNullException( configFile );
            if( !File.Exists( configFile ) )
                throw new ArgumentException( "File must exist", "projectFile" );

            List<INuGetPackageReference> references = new List<INuGetPackageReference>();

            XmlDocument d = new XmlDocument();
            d.Load( configFile );
            XmlNodeList packageNodeList = d.SelectNodes( "/packages/package" );

            foreach( XmlNode packageNode in packageNodeList )
            {
                string id = packageNode.Attributes["id"].Value;
                string version = packageNode.Attributes["version"].Value;

                // Optional
                FrameworkName targetFramework = null;
                if( packageNode.Attributes["targetFramework"] != null )
                {
                    targetFramework = VersionUtility.ParseFrameworkName( packageNode.Attributes["targetFramework"].Value );
                }

                INuGetPackageReference reference = new NuGetPackageReference( id, version, targetFramework );

                references.Add( reference );
            }

            return references;
        }

        /// <summary>
        /// Tries to get this package reference from a package repository.
        /// </summary>
        /// <param name="reference">Package reference</param>
        /// <param name="repository">NuGet repository</param>
        /// <returns></returns>
        public static IPackage GetPackageFromRepository( this INuGetPackageReference reference, IPackageRepository repository )
        {
            return repository.FindPackage( reference.Id, SemanticVersion.Parse( reference.Version ), true, true );
        }

        /// <summary>
        /// Parses a project file, and gets its references.
        /// </summary>
        /// <param name="projectFile">Path to the project file (.csproj)</param>
        /// <returns>Collection of new IProjectReference</returns>
        public static IEnumerable<IProjectReference> GetReferencesFromProjectFile( string projectFile )
        {
            if( string.IsNullOrEmpty( projectFile ) )
                throw new ArgumentNullException( projectFile );
            if( !File.Exists( projectFile ) )
                throw new ArgumentException( "File must exist", "projectFile" );

            List<IProjectReference> references = new List<IProjectReference>();

            XmlDocument d = new XmlDocument();
            d.Load( projectFile );

            XmlNamespaceManager mgr = new XmlNamespaceManager( d.NameTable );
            mgr.AddNamespace( "p", d.DocumentElement.NamespaceURI );

            XmlNodeList packageNodeList = d.SelectNodes( "/p:Project/p:ItemGroup/p:Reference", mgr );

            foreach( XmlNode packageNode in packageNodeList )
            {
                string assemblyName = packageNode.Attributes["Include"].Value;
                string hintPath = null;
                string requiredTargetFramework = null;
                bool embedInteropTypes = false;
                bool specificVersion = false;
                bool isPrivate = false;

                foreach( XmlNode child in packageNode.ChildNodes )
                {
                    switch( child.Name )
                    {
                        case "HintPath":
                            hintPath = child.FirstChild.Value;
                            break;

                        case "Private":
                            isPrivate = child.FirstChild.Value.ToLowerInvariant() == "true";
                            break;

                        case "RequiredTargetFramework":
                            requiredTargetFramework = child.FirstChild.Value;
                            break;

                        case "SpecificVersion":
                            specificVersion = child.FirstChild.Value.ToLowerInvariant() == "true";
                            break;

                        case "EmbedInteropTypes":
                            embedInteropTypes = child.FirstChild.Value.ToLowerInvariant() == "true";
                            break;

                        default:
                            break;
                    }
                }

                IProjectReference reference = new ProjectReference()
                {
                    AssemblyName = assemblyName,
                    EmbedInteropTypes = embedInteropTypes,
                    HintPath = hintPath,
                    IsPrivate = isPrivate,
                    RequiredTargetFramework = requiredTargetFramework,
                    SpecificVersion = specificVersion
                };

                references.Add( reference );
            }

            return references;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="projectFile">Path to the project file (.csproj)</param>
        /// <returns>Collection of new IProjectReference</returns>
        public static CSProjCompileLinkInfo GetSharedAssemblyRelativeLinkFromProjectFile( string projectFile )
        {
            if( string.IsNullOrEmpty( projectFile ) )
                throw new ArgumentNullException( projectFile );
            if( !File.Exists( projectFile ) )
                throw new ArgumentException( "File must exist", "projectFile" );

            XmlDocument d = new XmlDocument();
            d.Load( projectFile );

            XmlNamespaceManager mgr = new XmlNamespaceManager( d.NameTable );
            mgr.AddNamespace( "p", d.DocumentElement.NamespaceURI );

            XmlNodeList packageNodeList = d.SelectNodes( "/p:Project/p:ItemGroup/p:Compile", mgr );

            foreach( XmlNode packageNode in packageNodeList )
            {
                //Get real path without "\.."
                string sharedAssemblyInfoRelativePath = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( projectFile ), packageNode.Attributes["Include"].Value ) );
                string link;
                if( sharedAssemblyInfoRelativePath.Contains( "SharedAssemblyInfo.cs" ) )
                {
                    link = packageNode.FirstChild.InnerText;
                    return new CSProjCompileLinkInfo( sharedAssemblyInfoRelativePath, link, Path.GetFileNameWithoutExtension( projectFile ) );
                }
            }
            return null;
        }
    }
}