using System.Collections.Generic;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    internal class Solution : ISolution
    {
        internal List<ISolutionProjectItem> ProjectItems;

        public string DirectoryPath { get; internal set; }

        public IEnumerable<ISolutionProjectItem> Projects
        {
            get
            {
                return ProjectItems;
            }
        }
    }
}