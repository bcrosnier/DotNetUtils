using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectProber.Interfaces
{
    public interface ISolutionProjectItem
    {
        Guid ProjectTypeGuid { get; }
        Guid ProjectGuid { get; }
        string ProjectName { get; }
        string ProjectPath { get; }
    }
}
