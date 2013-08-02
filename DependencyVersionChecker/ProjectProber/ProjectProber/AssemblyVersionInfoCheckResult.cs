using ProjectProber.Impl;
using ProjectProber.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Package;

namespace ProjectProber
{
	public class AssemblyVersionInfoCheckResult
	{

		/// <summary>
		///
		/// </summary>
		public string SolutionDirectoryPath { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public IReadOnlyList<AssemblyVersionInfo> SharedAssemblyInfoVersions { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public IReadOnlyList<CSProjCompileLinkInfo> CsProjs { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// </remarks>
		public IReadOnlyList<AssemblyVersionInfo> ProjectVersions
		{
			get;
			private set;
		}

		public bool HaveSharedAssemblyInfo { get { return _haveSharedAssemblyInfo; } }
		public bool MultipleSharedAssemblyInfo { get { return _multipleSharedAssemblyInfo; } }
		public bool MultipleAssemblyVersion { get { return _multipleAssemblyVersion; } }
		public bool MultipleRelativeLinkInCSProj { get { return _multipleRelativeLinkInCSProj; } }
		public bool MultipleAssemblyFileInfoVersion { get { return _multipleAssemblyFileInfoVersion; } }
		public bool MultipleAssemblyInformationVersion { get { return _multipleAssemblyInformationVersion; } }
		public bool HaveOneVersionNotSemanticVersionCompliante { get { return _haveOneVersionNotSemanticVersionCompliante; } }
		public bool MultipleVersionInOneAssemblyInfoFile { get { return _multipleVersionInOneAssemblyInfoFile; } }
		public bool RelativeLinkInCSProjNotFound { get { return _relativeLinkInCSProjNotFound; } }
		public bool HaveFileWithoutVersion { get { return _haveFileWithoutVersion; } }

		bool _haveSharedAssemblyInfo = false;
		bool _multipleSharedAssemblyInfo = false;
		bool _multipleAssemblyVersion = false;
		bool _multipleRelativeLinkInCSProj = false;
		bool _relativeLinkInCSProjNotFound = false;
		bool _multipleAssemblyFileInfoVersion = false;
		bool _multipleAssemblyInformationVersion = false;
		bool _haveOneVersionNotSemanticVersionCompliante = false;
		bool _multipleVersionInOneAssemblyInfoFile = false;
		bool _haveFileWithoutVersion = false;

		public IReadOnlyList<AssemblyVersionInfo> Versions { get { return _versions.AsReadOnly(); } }
		List<AssemblyVersionInfo> _versions;

		internal AssemblyVersionInfoCheckResult( string solutionDirectoryPath,
			List<AssemblyVersionInfo> sharedAssemblyInfoVersions,
			List<CSProjCompileLinkInfo> csProjs,
			List<AssemblyVersionInfo> projectVersions )
		{
			SolutionDirectoryPath = solutionDirectoryPath;
			SharedAssemblyInfoVersions = sharedAssemblyInfoVersions;
			CsProjs = csProjs;
			ProjectVersions = projectVersions;
			_versions = new List<AssemblyVersionInfo>();

			if( sharedAssemblyInfoVersions.Count > 1 )
			{
				_haveSharedAssemblyInfo = true;
				_multipleSharedAssemblyInfo = true;
				foreach( AssemblyVersionInfo version in sharedAssemblyInfoVersions )
				{
					if( version.AssemblyVersion != null && _versions.Count == 0 )
					{
						_versions.Add( version );
					}
					bool temp = false;
					foreach( AssemblyVersionInfo versionCompare in _versions )
					{
						if( versionCompare == version )
						{
							temp = true;
							break;
						}
					}
					if( !temp ) _versions.Add( version );
					if( version.AssemblyVersion != version.AssemblyFileVersion
						|| version.AssemblyVersion != version.AssemblyInformationVersion.Version )
						_multipleVersionInOneAssemblyInfoFile = true;
				}
			}
			else if( sharedAssemblyInfoVersions.Count == 1 )
			{
				//pas sûr
				_haveSharedAssemblyInfo = true;
				_versions.Add( sharedAssemblyInfoVersions.First() );
				if( sharedAssemblyInfoVersions.First().AssemblyVersion != sharedAssemblyInfoVersions.First().AssemblyFileVersion
						|| ( sharedAssemblyInfoVersions.First().AssemblyInformationVersion != null 
						&& sharedAssemblyInfoVersions.First().AssemblyVersion != sharedAssemblyInfoVersions.First().AssemblyInformationVersion.Version ) )
				{
					_multipleVersionInOneAssemblyInfoFile = true;
				}
				IList<CSProjCompileLinkInfo> csProjCompileLinkInfoToCompare = new List<CSProjCompileLinkInfo>();
				foreach( CSProjCompileLinkInfo csProjCompileLinkInfo in csProjs )
				{
					if( csProjCompileLinkInfo == null )
					{
						_relativeLinkInCSProjNotFound = true;
						continue;
					}
					if( csProjCompileLinkInfoToCompare.Count == 0 )
					{
						csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
					}
					bool temp = false;
					foreach( CSProjCompileLinkInfo csProjCompileLinkInfoCompare in csProjCompileLinkInfoToCompare )
					{
						if( csProjCompileLinkInfoCompare == csProjCompileLinkInfo )
						{
							temp = true;
							break;
						}

					}
					if( !temp )
					{
						csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
						_multipleRelativeLinkInCSProj = true;
					}
					
				}
			}
			else
			{
				foreach( AssemblyVersionInfo version in projectVersions )
				{
					if( version.AssemblyVersion == null && version.AssemblyFileVersion == null && version.AssemblyInformationVersion == null )
					{
						_haveFileWithoutVersion = true;
						continue;
					}
					if( _versions.Count == 0 )
					{
						_versions.Add( version );
					}
					bool temp = false;
					foreach( AssemblyVersionInfo versionCompare in _versions )
					{
						if( versionCompare == version )
						{
							temp = true;
							break;
						}
					}
					if( !temp ) _versions.Add( version );
					if( version.AssemblyVersion != version.AssemblyFileVersion
						|| ( version.AssemblyInformationVersion != null 
						&& version.AssemblyVersion != version.AssemblyInformationVersion.Version ) )
						_multipleVersionInOneAssemblyInfoFile = true;
				}
			}
			if( _versions.Count > 1 )
			{
				for( int i = 1; i < _versions.Count; i++ )
					CheckAssemblyInfo( _versions.First(), _versions[i] );
			}
		}

		private void CheckAssemblyInfo( AssemblyVersionInfo a1, AssemblyVersionInfo a2 )
		{
			if( a1.AssemblyVersion != null && a2.AssemblyVersion != null && a1.AssemblyVersion != a2.AssemblyVersion ) _multipleAssemblyVersion = true;
			if( a1.AssemblyFileVersion != null && a2.AssemblyFileVersion != null && a1.AssemblyFileVersion != a2.AssemblyFileVersion ) _multipleAssemblyFileInfoVersion = true;
			if( a1.AssemblyInformationVersion != null && a2.AssemblyInformationVersion != null && a1.AssemblyInformationVersion != a2.AssemblyInformationVersion ) _multipleAssemblyInformationVersion = true;
			SemanticVersion tempToTest;
			if( a1.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyFileVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( a1.AssemblyVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( a1.AssemblyInformationVersion != null && !SemanticVersion.TryParseStrict( a1.AssemblyInformationVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( a2.AssemblyFileVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyFileVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( a2.AssemblyVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( a2.AssemblyInformationVersion != null && !SemanticVersion.TryParseStrict( a2.AssemblyInformationVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
		}
	}
}
