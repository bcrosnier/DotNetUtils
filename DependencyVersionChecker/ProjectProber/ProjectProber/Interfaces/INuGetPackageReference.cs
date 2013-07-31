using System.Runtime.Versioning;

namespace ProjectProber.Interfaces
{
    /// <summary>
    /// NuGet package reference, as seen in projects' package.config
    /// </summary>
    public interface INuGetPackageReference
    {
        /// <summary>
        /// Package ID
        /// </summary>
        /// <example>CK.Context</example>
        string Id { get; }

        /// <summary>
        /// Package descriptive version
        /// </summary>
        /// <example>2.9.1-develop</example>
        string Version { get; }

        /// <summary>
        /// Target framework object
        /// </summary>
        /// <example>net40-Client</example>
        FrameworkName TargetFramework { get; }
    }
}