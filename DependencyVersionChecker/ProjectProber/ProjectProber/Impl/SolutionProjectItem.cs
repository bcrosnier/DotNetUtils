using ProjectProber.Interfaces;
using System;
using System.Diagnostics;

namespace ProjectProber.Impl
{
    [DebuggerDisplay("{ProjectName}")]
    internal class SolutionProjectItem : ISolutionProjectItem
    {
        #region ISolutionProjectItem Members

        public Guid ProjectTypeGuid { get; private set; }

        public Guid ProjectGuid { get; private set; }

        public string ProjectName { get; private set; }

        public string ProjectPath { get; private set; }

        #endregion ISolutionProjectItem Members

        internal SolutionProjectItem(Guid projectTypeGuid, Guid projectGuid, string projectName, string projectPath)
        {
            ProjectGuid = projectGuid;
            ProjectName = projectName;
            ProjectTypeGuid = projectTypeGuid;
            ProjectPath = projectPath;
        }
    }
}