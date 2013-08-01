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

		bool _haveSharedAssemblyInfo = false;
		bool _multipleSharedAssemblyInfo = false;
		bool _multipleAssemblyVersion = false;
		bool _multipleRelativeLinkInCSProj = false;
		bool _multipleAssemblyFileInfoVersion = false;
		bool _multipleAssemblyInformationVersion = false;
		bool _haveOneVersionNotSemanticVersionCompliante = false;
		bool _multipleVersionInOneAssemblyInfoFile = false;

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
				_versions.Add( sharedAssemblyInfoVersions.First() );
				foreach( AssemblyVersionInfo version in sharedAssemblyInfoVersions )
				{
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
						&& version.AssemblyVersion != version.AssemblyInformationVersion.Version )
						_multipleVersionInOneAssemblyInfoFile = true;
				}
			}
			else if( sharedAssemblyInfoVersions.Count == 1 )
			{
				//pas sûr
				_haveSharedAssemblyInfo = true;
				if( sharedAssemblyInfoVersions.First().AssemblyInformationVersion == null )
				{
					_versions.Add( sharedAssemblyInfoVersions.First() );
				}
				else
				{
					_versions.Add( sharedAssemblyInfoVersions.First() );
				}
				IList<CSProjCompileLinkInfo> csProjCompileLinkInfoToCompare = new List<CSProjCompileLinkInfo>();
				csProjCompileLinkInfoToCompare.Add( csProjs.First() );
				foreach( CSProjCompileLinkInfo csProjCompileLinkInfo in csProjs )
				{
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
				_versions.Add( projectVersions.First() );
				foreach( AssemblyVersionInfo version in projectVersions )
				{
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
						&& version.AssemblyVersion != version.AssemblyInformationVersion.Version )
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
			if( a1.AssemblyInformationVersion != a2.AssemblyInformationVersion ) _multipleAssemblyVersion = true;
			if( a1.AssemblyFileVersion != a2.AssemblyFileVersion ) _multipleAssemblyFileInfoVersion = true;
			if( a1.AssemblyInformationVersion != a2.AssemblyInformationVersion ) _multipleAssemblyInformationVersion = true;
			SemanticVersion tempToTest;
			if( !SemanticVersion.TryParse( a1.AssemblyFileVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( !SemanticVersion.TryParse( a1.AssemblyVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( !SemanticVersion.TryParse( a1.AssemblyInformationVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( !SemanticVersion.TryParse( a2.AssemblyFileVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( !SemanticVersion.TryParse( a2.AssemblyVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
			if( !SemanticVersion.TryParse( a2.AssemblyInformationVersion.ToString(), out tempToTest ) ) _haveOneVersionNotSemanticVersionCompliante = true;
		}
	}
}
