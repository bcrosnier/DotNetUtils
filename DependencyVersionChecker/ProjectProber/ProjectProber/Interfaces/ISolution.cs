using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Interfaces
{
    /// <summary>
    /// Simple build solution object.
    /// </summary>
    public interface ISolution
    {
        /// <summary>
        /// Projects which take part in the solution.
        /// </summary>
        IEnumerable<ISolutionProjectItem> Projects { get; }

        /// <summary>
        /// Solution directory.
        /// </summary>
        string DirectoryPath { get; }
    }
}
