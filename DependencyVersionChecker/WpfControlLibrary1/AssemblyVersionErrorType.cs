using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtilitiesApp.VersionAnalyzer
{
    internal enum AssemblyVersionErrorType
    {
        HasNotSharedAssemblyInfo,
        HasMultipleSharedAssemblyInfo,
        HasMultipleAssemblyVersion,
        HasMultipleRelativeLinkInCSProj,
        HasRelativeLinkInCSProjNotFound,
        HasMultipleAssemblyFileVersion,
        HasMultipleAssemblyInformationVersion,
        HasOneVersionNotSemanticVersionCompliant,
        HasMultipleVersionInOneAssemblyInfo,
        HasFileWithoutVersion
    }
}
