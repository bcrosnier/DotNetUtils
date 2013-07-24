using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProber.Interfaces
{
    public interface IPackageLibraryReference
    {
        string PackageIdVersion { get; }
        string TargetFramework { get; }
        string AssemblyFileName { get; }
        string FullPath { get; }
    }
}
