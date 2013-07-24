using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Interfaces
{
    public interface ISolution
    {
        IEnumerable<ISolutionProjectItem> Projects { get; }
        string DirectoryPath { get; }
    }
}
