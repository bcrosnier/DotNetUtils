using ProjectProber.Impl;
using ProjectProber.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public bool HaveSharedAssemblyInfo
		{
			get { return _haveSharedAssemblyInfo; }
		}
		public bool MultipleSharedAssemblyInfo
		{
			get { return _multipleSharedAssemblyInfo; }
		}
		public bool MultipleSharedAssemblyInfoDifferenteVersion
		{
			get { return _multipleSharedAssemblyInfoDifferenteVersion; }
		}
		public bool MultipleRelativeLinkInCSProj
		{
			get { return _multipleRelativeLinkInCSProj; }
		}
		public bool MultipleVersionInPropretiesAssemblyInfo
		{
			get { return _multipleVersionInPropretiesAssemblyInfo; }
		}

		bool _haveSharedAssemblyInfo = false;
		bool _multipleSharedAssemblyInfo = false;
		bool _multipleSharedAssemblyInfoDifferenteVersion = false;
		bool _multipleRelativeLinkInCSProj = false;
		bool _multipleVersionInPropretiesAssemblyInfo = false;

		IReadOnlyList<Version> Versions { get { return _versions.AsReadOnly(); } }
		List<Version> _versions;

		internal AssemblyVersionInfoCheckResult( string solutionDirectoryPath,
			List<AssemblyVersionInfo> sharedAssemblyInfoVersions,
			List<CSProjCompileLinkInfo> csProjs,
			List<AssemblyVersionInfo> projectVersions )
		{
			SolutionDirectoryPath = solutionDirectoryPath;
			SharedAssemblyInfoVersions = sharedAssemblyInfoVersions;
			CsProjs = csProjs;
			ProjectVersions = projectVersions;
			_versions = new List<Version>();

			if( sharedAssemblyInfoVersions.Count > 1 )
			{
				_haveSharedAssemblyInfo = true;
				_multipleSharedAssemblyInfo = true;

				foreach( AssemblyVersionInfo version in sharedAssemblyInfoVersions )
				{
					foreach( Version versionCompare in _versions )
					{
						if( versionCompare != version.AssemblyVersion )
						{
							_versions.Add( version.AssemblyVersion );
							_multipleSharedAssemblyInfoDifferenteVersion = true;
							break;
						}
					}
					if( _versions.Count == 0 )
					{
						_versions.Add( version.AssemblyVersion );
					}
				}
			}
			else if( sharedAssemblyInfoVersions.Count == 1 )
			{
				//pas sûr
				_haveSharedAssemblyInfo = true;
				IList<CSProjCompileLinkInfo> csProjCompileLinkInfoToCompare = new List<CSProjCompileLinkInfo>();
				foreach( CSProjCompileLinkInfo csProjCompileLinkInfo in csProjs )
				{
					foreach( CSProjCompileLinkInfo csProjCompileLinkInfoCompare in csProjCompileLinkInfoToCompare )
					{
						if( csProjCompileLinkInfoCompare != csProjCompileLinkInfo )
						{
							csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
							_multipleRelativeLinkInCSProj = true;
							break;
						}
					}
					if( csProjCompileLinkInfoToCompare.Count == 0 )
					{
						csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
					}
				}
			}
			else
			{
				foreach( AssemblyVersionInfo version in projectVersions )
				{
					foreach( Version versionCompare in _versions )
					{
						if( versionCompare != version.AssemblyVersion )
						{
							_versions.Add( version.AssemblyVersion );
							_multipleVersionInPropretiesAssemblyInfo = true;
							break;
						}
					}
					if( _versions.Count == 0 )
					{
						_versions.Add( version.AssemblyVersion );
					}
				}
			}
			
		}
	}
}
