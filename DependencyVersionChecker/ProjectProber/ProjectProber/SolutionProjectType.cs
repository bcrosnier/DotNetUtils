using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber
{
    /// <summary>
    /// Type of a solution project, eg. a C# project, a VB project, a solution folder, etc.
    /// </summary>
    /// <seealso cref="ProjectProber.SolutionUtils.GetProjectType"/>
    /// <seealso cref="ProjectProber.SolutionUtils.ProjectTypes"/>
    public enum SolutionProjectType
    {
        /// <summary>
        /// Unknown/unreferenced project type.
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// Solution folder.
        /// </summary>
        PROJECT_FOLDER,
        /// <summary>
        /// SQL Server database project.
        /// </summary>
        SQL_DATABASE_PROJECT,
        /// <summary>
        /// Visual C++ project.
        /// </summary>
        VISUAL_CPP,
        /// <summary>
        /// Visual Basic project.
        /// </summary>
        VISUAL_BASIC,
        /// <summary>
        /// Visual C# project.
        /// </summary>
        VISUAL_C_SHARP,
        /// <summary>
        /// Visual F# project.
        /// </summary>
        VISUAL_F_SHARP
    }
}
