namespace ProjectProber.Interfaces
{
    /// <summary>
    /// Reference to an assembly, which is part of a NuGet package, from a project.
    /// </summary>
    /// <remarks>
    /// Can be parsed from a project reference path using ProjectUtils.ParseReferenceFromPath.
    /// </remarks>
    public interface IPackageLibraryReference
    {
        /// <summary>
        /// The full identifier of the NuGet package, in its format PackageId.PackageVersion, as seen on the file system.
        /// </summary>
        /// <example>
        /// CK.Core.2.8.14
        /// </example>
        string PackageIdVersion { get; }

        /// <summary>
        /// The target framework of the assembly in the package package, as seen on the filesystem. Can be null.
        /// </summary>
        /// <example>
        /// net45
        /// </example>
        string TargetFramework { get; }

        /// <summary>
        /// The file name of the assembly, inside its respective folder.
        /// </summary>
        /// <example>
        /// CK.Core.dll
        /// </example>
        string AssemblyFileName { get; }

        /// <summary>
        /// The path, normally relative to the project file, which was used for parsing.
        /// </summary>
        /// <example>
        /// ..\packages\CK.Core.2.8.14\lib\net45\CK.Core.dll
        /// </example>
        string FullPath { get; }
    }
}