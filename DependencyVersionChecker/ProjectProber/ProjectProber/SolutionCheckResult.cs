using System.Collections.Generic;
using System.Linq;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Result of a solution-checking process. Contains conclusions about a certain solution.
    /// </summary>
    public class SolutionCheckResult
    {
        /// <summary>
        /// Path of the solution file (.sln)
        /// </summary>
        public string SolutionPath { get; private set; }

        /// <summary>
        /// Evaluated NuGet packages from all projects of the solution.
        /// </summary>
        public IReadOnlyDictionary<INuGetPackageReference, IPackage> NuGetPackages { get; private set; }

        /// <summary>
        /// Evaluated project items in the solution.
        /// </summary>
        public IEnumerable<ISolutionProjectItem> Projects { get; private set; }

        /// <summary>
        /// Dictionary associating each project of the solution to all of its assembly references originating from NuGet packages.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public IReadOnlyDictionary<ISolutionProjectItem, IEnumerable<IProjectAssemblyReference>> ProjectAssemblyReferences
        {
            get;
            private set;
        }

        /// <summary>
        /// Dictionary associating each project of the solution to all of its references to NuGet packages.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public IReadOnlyDictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>> ProjectNugetReferences
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a dictionary associating a simple package id (eg. CK.Core) to all matching full package identifiers detected during the check, when there are multiple.
        /// </summary>
        /// <example>
        /// There are two CK.Core with two distinct versions referenced during the check.
        /// Entry will be:
        /// Key: CK.Core
        /// Value: { "CK.Core.2.1.0", "CK.Core.2.8.5" }
        /// </example>
        public IReadOnlyDictionary<string, IEnumerable<INuGetPackageReference>> PackagesWithMultipleVersions
        {
            get;
            private set;
        }

        internal SolutionCheckResult( string solutionPath, IReadOnlyDictionary<INuGetPackageReference, IPackage> scannedPackages, IEnumerable<ISolutionProjectItem> projects,
            IReadOnlyDictionary<ISolutionProjectItem, IEnumerable<IProjectAssemblyReference>> assemblyReferences,
            IReadOnlyDictionary<ISolutionProjectItem, IEnumerable<INuGetPackageReference>> packageReferences )
        {
            SolutionPath = solutionPath;
            NuGetPackages = scannedPackages;
            Projects = projects;
            ProjectAssemblyReferences = assemblyReferences;
            ProjectNugetReferences = packageReferences;

            // Looks for multiple versions of each NuGet package.
            Dictionary<string, List<INuGetPackageReference>> packagesPerId = new Dictionary<string, List<INuGetPackageReference>>();

            foreach( INuGetPackageReference packageRef in NuGetPackages.Keys )
            {
                List<INuGetPackageReference> packageVersions;
                if( !packagesPerId.TryGetValue( packageRef.Id, out packageVersions ) )
                {
                    // Package Id is not in the list
                    packageVersions = new List<INuGetPackageReference>();
                    packagesPerId.Add( packageRef.Id, packageVersions );
                }

                packageVersions.Add( packageRef );
            }

            PackagesWithMultipleVersions = packagesPerId
                .Where( x => x.Value.Count > 1 )
                .ToDictionary( x => x.Key, x => (IEnumerable<INuGetPackageReference>)x.Value );
        }
    }
}