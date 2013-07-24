using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Result of a solution-checking process. Contains conclusions about a certain solution.
    /// </summary>
    public class SolutionCheckResult
    {
        /// <summary>
        /// Dictionary associating each project of a solution to its NuGet Package reference.
        /// </summary>
        /// <remarks>
        /// IPackageLibraryReference.
        /// </remarks>
        public IReadOnlyDictionary<ISolutionProjectItem, List<IPackageLibraryReference>> ProjectReferences
        {
            get;
            internal set;
        }

        internal SolutionCheckResult()
        {
        }
    }
}
