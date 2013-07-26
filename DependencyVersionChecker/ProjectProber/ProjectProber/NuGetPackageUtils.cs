using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using AssemblyProber;
using System.IO;
using System.Runtime.Versioning;

namespace ProjectProber
{
    static class NuGetPackageUtils
    {
        public static IEnumerable<IAssemblyInfo> GetAssemblyNames( this IPackage package, IAssemblyLoader assemblyLoader )
        {
            List<IAssemblyInfo> packageAssemblies = new List<IAssemblyInfo>();
            List<string> assemblyNames = new List<string>();

            foreach( var libFile in package.GetLibFiles() )
            {
                if( Path.GetExtension( libFile.EffectivePath ).ToLowerInvariant() == @".dll" )
                {
                    IAssemblyInfo assembly = assemblyLoader.LoadFromFile( libFile.GetStream() );
                    if( !assemblyNames.Contains( assembly.FullName ) )
                    {
                        assemblyNames.Add( assembly.FullName );
                        packageAssemblies.Add( assembly );
                    }
                }
            }

            return packageAssemblies;
        }
    }
}
