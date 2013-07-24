using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    /// <summary>
    /// Static solution utilities.
    /// </summary>
    public static class SolutionUtils
    {
        /// <summary>
        /// Dictionary mapping known project type Guids to their Enum equivalent. Used by GetProjectType().
        /// </summary>
        /// <seealso cref="ProjectProber.SolutionUtils.GetProjectType"/>
        /// <seealso cref="ProjectProber.SolutionProjectType"/>
        public static readonly IReadOnlyDictionary<Guid, SolutionProjectType> ProjectTypes =
            new Dictionary<Guid, SolutionProjectType>()
            {
                { new Guid( "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" ), SolutionProjectType.VISUAL_C_SHARP },
                { new Guid( "2150E333-8FDC-42A3-9474-1A3956D46DE8" ), SolutionProjectType.PROJECT_FOLDER },
                { new Guid( "F184B08F-C81C-45F6-A57F-5ABD9991F28F" ), SolutionProjectType.VISUAL_BASIC },
                { new Guid( "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942" ), SolutionProjectType.VISUAL_CPP },
                { new Guid( "00D1A9C2-B5F0-4AF3-8072-F6C62B433612" ), SolutionProjectType.SQL_DATABASE_PROJECT },
                { new Guid( "F2A71F9B-5D33-465A-A702-920D77279786" ), SolutionProjectType.VISUAL_F_SHARP },
            };

        /// <summary>
        /// Gets the SolutionProjectType of a given ISolutionProjectItem, using the ProjectTypes dictionary.
        /// </summary>
        /// <param name="projectItem">Project item to get type of</param>
        /// <returns>Type of project item, or SolutionProjectType.UNKNOWN if it couldn't be guessed.</returns>
        public static SolutionProjectType GetProjectType( ISolutionProjectItem projectItem )
        {
            SolutionProjectType projectType = SolutionProjectType.UNKNOWN;

            ProjectTypes.TryGetValue( projectItem.ProjectTypeGuid, out projectType );

            return projectType;
        }
    }
}
