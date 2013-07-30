using System;

namespace ProjectProber.Interfaces
{
    /// <summary>
    /// Project item, as described in the solution.
    /// </summary>
    public interface ISolutionProjectItem
    {
        /// <summary>
        /// Guid of the project type (VB, C#, etc.).
        /// See also: <see cref="ProjectProber.SolutionUtils.GetItemType"/>
        /// </summary>
        /// <seealso cref="ProjectProber.SolutionProjectType"/>
        Guid ProjectTypeGuid { get; }

        /// <summary>
        /// Unique Guid of this particular project.
        /// </summary>
        Guid ProjectGuid { get; }

        /// <summary>
        /// Name of the project, as described in the solution.
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// Path to the project file, as described in the solution.
        /// </summary>
        string ProjectPath { get; }
    }
}