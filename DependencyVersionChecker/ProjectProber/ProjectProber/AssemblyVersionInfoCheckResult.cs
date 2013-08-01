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
		public bool MultipleAssemblyVersion
		{
			get { return _multipleAssemblyVersion; }
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
		bool _multipleAssemblyVersion = false;
		bool _multipleRelativeLinkInCSProj = false;
		bool _multipleVersionInPropretiesAssemblyInfo = false;

		//Todo
		bool _haveDifferenteVersionInFile = false;
		bool _assemblyInformationVersionNotSemanticVersionCompliant = false;
		bool _assemblyVersionNotSemanticVersionCompliant = false;


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
					if( !temp )
					{
						_versions.Add( version );
						_multipleAssemblyVersion = true;
					}
					if( _versions.Count == 0 )
					{
						_versions.Add( version );
					}
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
					foreach( CSProjCompileLinkInfo csProjCompileLinkInfoCompare in csProjCompileLinkInfoToCompare )
					{
						if( csProjCompileLinkInfoCompare != csProjCompileLinkInfo )
						{
							csProjCompileLinkInfoToCompare.Add( csProjCompileLinkInfo );
							_multipleRelativeLinkInCSProj = true;
							break;
						}
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
					if( !temp )
					{
						_versions.Add( version );
						_multipleAssemblyVersion = true;
					}
				}
			}
			
		}
	}
}
