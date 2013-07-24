using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber
{
    public static class SolutionUtils
    {
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

        public static readonly Guid CSharpProjectType = new Guid( "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" );

        public static SolutionProjectType GetProjectType( ISolutionProjectItem projectItem )
        {
            SolutionProjectType projectType = SolutionProjectType.UNKNOWN;

            ProjectTypes.TryGetValue( projectItem.ProjectTypeGuid, out projectType );

            return projectType;
        }
    }
}
