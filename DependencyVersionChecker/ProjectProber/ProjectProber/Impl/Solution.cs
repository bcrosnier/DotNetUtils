using System.Collections.Generic;
using System.IO;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    internal class Solution : ISolution
    {
        internal List<ISolutionProjectItem> ProjectItems;

        public string DirectoryPath { get { return Path.GetDirectoryName( FilePath ); } }

        public string Name { get; internal set; }

        public string FilePath { get; internal set; }

        public IEnumerable<ISolutionProjectItem> Projects
        {
            get
            {
                return ProjectItems;
            }
        }
    }
}