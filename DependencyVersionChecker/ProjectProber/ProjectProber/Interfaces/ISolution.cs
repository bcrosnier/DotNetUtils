using System.Collections.Generic;

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