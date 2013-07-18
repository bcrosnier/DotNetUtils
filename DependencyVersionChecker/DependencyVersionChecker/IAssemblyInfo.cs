using System;
using System.Collections.Generic;

namespace DependencyVersionChecker
{
    /// <summary>
    /// Information about a particular .NET assembly.
    /// </summary>
    public interface IAssemblyInfo
    {
        /// <summary>
        /// Assembly display name.
        /// The display name typically consists of the simple name, version number, supported culture, and public key.
        /// See: <see cref="System.Reflection.AssemblyName.FullName"/>
        /// </summary>
        string AssemblyFullName { get; }

        /// <summary>
        /// Assembly simple name, from its unique identity.
        /// Equivalent of: <see cref="System.Reflection.AssemblyName.Name"/>
        /// </summary>
        string SimpleName { get; }

        /// <summary>
        /// Assembly version, as compiled in System.Reflection.AssemblyName.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Supported culture, as compiled in System.Reflection.AssemblyName.
        /// See: <see cref="System.Reflection.AssemblyVersionAttribute"/>
        /// </summary>
        string Culture { get; }

        /**
         * Properties above can be inferred from assembly reference.
         * Properties below require assembly resolution.
         */

        /// <summary>
        /// File version string, as usually declared in Properties/AssemblyInfo.cs,
        /// or through Project Properties.
        /// See: <see cref="System.Reflection.AssemblyFileVersionAttribute"/>
        /// </summary>
        string FileVersion { get; }

        /// <summary>
        /// Assembly informational version (or product version).
        /// See: <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>
        /// </summary>
        /// <remarks>
        /// The informational version is a string that attaches additional version information to an assembly for informational purposes only;
        /// this information is not used at run time.
        /// The text-based informational version corresponds to the product's marketing literature, packaging, or product name and is not used by the runtime.
        /// For example, an informational version could be "Common Language Runtime version 1.0" or "NET Control SP 2".
        /// On the Version tab of the file properties dialog in Microsoft Windows, this information appears in the item "Product Version".
        /// </remarks>
        string InformationalVersion { get; }

        /// <summary>
        /// Assemblies this one has references to.
        /// </summary>
        IEnumerable<IAssemblyInfo> Dependencies { get; }
    }
}