using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    class Solution : ISolution
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
