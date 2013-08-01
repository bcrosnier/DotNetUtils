using CK.Core;
using ProjectProber.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// <param name="solutionFilePath">Directory solution path</param>
		/// <param name="logger">Logger to use</param>
		/// <returns>AssemblyVersionCheckResult</returns>
		public static AssemblyVersionInfoCheckResult CheckAssemblyVersionFiles( string solutionFilePath, IActivityLogger logger )
		{
			if( String.IsNullOrEmpty( solutionFilePath ) )
				throw new ArgumentNullException( "solutionRootPath" );
			if( !File.Exists( solutionFilePath ) )
				throw new ArgumentException( "Directory don't exist", "solutionRootPath" );

			string solutionDirectoryPath = Path.GetDirectoryName( solutionFilePath );

			Dictionary<string, Version> sharedAssemblyInfoVersion = new Dictionary<string, Version>();
			Dictionary<ISolutionProjectItem, CSProjCompileLinkInfo> csProj = new Dictionary<ISolutionProjectItem, CSProjCompileLinkInfo>();
			Dictionary<ISolutionProjectItem, Version> projectVersion = new Dictionary<ISolutionProjectItem, Version>();

			//cherche les SharedAssemblyInfo.cs et les lie directement à leurs versions
			foreach( string path in Directory.GetFiles( solutionDirectoryPath, "SharedAssemblyInfo.cs", SearchOption.AllDirectories ) )
			{
				sharedAssemblyInfoVersion.Add( path, AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( path ));
			}

			ISolution solution = SolutionFactory.ReadFromSolutionFile( solutionFilePath );

			IEnumerable<ISolutionProjectItem> cSharpProjects = solution.Projects.Where( x => x.GetItemType() == SolutionProjectType.VISUAL_C_SHARP );

			foreach( ISolutionProjectItem project in cSharpProjects )
			{
				csProj.Add( project, ProjectUtils.GetSharedAssemblyRelativeLinkFromProjectFile( Path.Combine( solution.DirectoryPath, project.ProjectPath ) ) );
			}

			foreach( ISolutionProjectItem project in cSharpProjects )
			{
				string assemblyInfoPath = Path.Combine( solution.DirectoryPath, Path.GetDirectoryName( project.ProjectPath ), @"Properties\AssemblyInfo.cs" );
				projectVersion.Add( project, AssemblyVersionInfoParser.GetAssemblyVersionFromAssemblyInfoFile( assemblyInfoPath ) );
			}


			return new AssemblyVersionInfoCheckResult( solutionDirectoryPath, sharedAssemblyInfoVersion, csProj, projectVersion );
		}

	}

	
}
