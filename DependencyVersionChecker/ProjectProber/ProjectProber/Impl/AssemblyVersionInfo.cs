using CK.Package;
using ProjectProber.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Impl
{
	public class AssemblyVersionInfo
	{
		public Version AssemblyVersion { get { return _assemblyVersion; } }
		public Version AssemblyFileVersion { get { return _assemblyFileVersion; } }
		public SemanticVersion AssemblyInformationVersion { get { return _assemblyInformationVersion; } }
		public bool IsSharedAssemblyInformation { get { return _isSharedAssemblyInformation; } }
		public string AssemblyInfoFilePath { get { return _assemblyInfoFilePath; } }
		public ISolutionProjectItem SolutionProjectItem { get { return _solutionProjectItem; } }

		Version _assemblyVersion;
		Version _assemblyFileVersion;
		SemanticVersion _assemblyInformationVersion;
		bool _isSharedAssemblyInformation;
		string _assemblyInfoFilePath;
		ISolutionProjectItem _solutionProjectItem;

		public AssemblyVersionInfo( string assemblyInfoFilePath, ISolutionProjectItem solutionProjectItem, Version assemblyVersion, Version assemblyFileVersion, SemanticVersion assemblyInformationVersion )
		{
			_assemblyInfoFilePath = assemblyInfoFilePath;
			_solutionProjectItem = solutionProjectItem;
			_assemblyVersion = assemblyFileVersion;
			_assemblyFileVersion = assemblyFileVersion;
			_assemblyInformationVersion = assemblyInformationVersion;

			_isSharedAssemblyInformation = solutionProjectItem == null;
		}
	}
}
