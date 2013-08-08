namespace ProjectProber.Interfaces
{
    /// <summary>
    /// Project reference to an assembly, through its full or simple name.
    /// </summary>
    public interface IProjectAssemblyReference
    {
        /// <summary>
        /// Required target framework of the reference. Can be null.
        /// </summary>
        string RequiredTargetFramework { get; }

        /// <summary>
        /// Requested assembly name. Either a simple name, or an assembly full name.
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Assembly hint path, where the builder will try to look. Can be null. Relative to project file.
        /// </summary>
        string HintPath { get; }

        /// <summary>
        /// Private flag of the reference.
        /// </summary>
        bool IsPrivate { get; }

        /// <summary>
        /// EmbedInteropTypes flag of the reference.
        /// </summary>
        bool EmbedInteropTypes { get; }

        /// <summary>
        /// SpecificVersion flag of the reference.
        /// </summary>
        bool SpecificVersion { get; }
    }
}