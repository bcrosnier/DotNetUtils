using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CK.Core;
using ProjectProber.Impl;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    public static class AssemblyVersionInfoChecker
    {
        /// <summary>
        /// Checks a SharedAssemblyInfo file and all AssemblyInfo files.
        /// </summary>
        /// <param name="solutionPath">SharedAssemblyInfo file path</param>
        /// <returns>AssemblyVersionCheckResult</returns>
        public static AssemblyVersionInfoCheckResult CheckAssemblyVersionFiles( string solutionPath )
        {
            return CheckAssemblyVersionFiles( solutionPath, DefaultActivityLogger.Empty );
        }

        /// <summary>
        /// Checks a SharedAssemblyInfo file and all AssemblyInfo files.
        /// </summary>
        /// <param name="solutionFilePath">Solution file path</param>
        /// <param name="logger">Logger to use</param>
        /// <returns>AssemblyVersionCheckResult</returns>
        public static AssemblyVersionInfoCheckResult CheckAssemblyVersionFiles( string solutionFilePath, IActivityLogger logger )
        {
            if( String.IsNullOrEmpty( solutionFilePath ) )
                throw new ArgumentNullException( "solutionFilePath" );
            if( !File.Exists( solutionFilePath ) )
                throw new ArgumentException( "Directory don't exist", "solutionFilePath" );

            string solutionDirectoryPath = Path.GetDirectoryName( solutionFilePath );

            List<AssemblyVersionInfo> sharedAssemblyInfoVersion = new List<AssemblyVersionInfo>();
            List<CSProjCompileLinkInfo> csProj = new List<CSProjCompileLinkInfo>();
            List<AssemblyVersionInfo> projectVersion = new List<AssemblyVersionInfo>();

            //cherche les SharedAssemblyInfo.cs et les lie directement à leurs versions
            foreach( string path in Directory.GetFiles( solutionDirectoryPath, "SharedAssemblyInfo.cs", SearchOption.AllDirectories ) )
            {
                AssemblyVersionInfo temp = new AssemblyVersionInfo( path,
                    null,
                    AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( path, AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN ),
                    AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( path, AssemblyVersionInfoParser.FILE_VERSION_ASSEMBLY_PATTERN ),
                    AssemblyVersionInfoParser.GetSemanticAssemblyVersionFromAssemblyInfoFile( path, AssemblyVersionInfoParser.INFO_VERSION_ASSEMBLY_PATTERN ) );
                sharedAssemblyInfoVersion.Add( temp );
            }

            ISolution solution = SolutionFactory.ReadFromSolutionFile( solutionFilePath );

            IEnumerable<ISolutionProjectItem> cSharpProjects = solution.Projects.Where( x => x.GetItemType() == SolutionProjectType.VISUAL_C_SHARP );

            foreach( ISolutionProjectItem project in cSharpProjects )
            {
                csProj.Add( ProjectUtils.GetSharedAssemblyRelativeLinkFromProjectFile( Path.Combine( solution.DirectoryPath, project.ProjectPath ) ) );
            }

            foreach( ISolutionProjectItem project in cSharpProjects )
            {
                string assemblyInfoPath = Path.Combine( solution.DirectoryPath, Path.GetDirectoryName( project.ProjectPath ), @"Properties\AssemblyInfo.cs" );
                AssemblyVersionInfo temp = new AssemblyVersionInfo( assemblyInfoPath,
                    project,
                    AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( assemblyInfoPath, AssemblyVersionInfoParser.VERSION_ASSEMBLY_PATTERN ),
                    AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( assemblyInfoPath, AssemblyVersionInfoParser.FILE_VERSION_ASSEMBLY_PATTERN ),
                    AssemblyVersionInfoParser.GetSemanticAssemblyVersionFromAssemblyInfoFile( assemblyInfoPath, AssemblyVersionInfoParser.INFO_VERSION_ASSEMBLY_PATTERN ) );
                projectVersion.Add( temp );
            }

            return new AssemblyVersionInfoCheckResult( solutionDirectoryPath, sharedAssemblyInfoVersion, csProj, projectVersion );
        }
    }
}