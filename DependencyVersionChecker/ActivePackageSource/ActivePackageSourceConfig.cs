using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace ActivePackageSource
{
    /// <summary>
    /// Configuration object for an ActivePackageSource.
    /// </summary>
    [Serializable()]
    public class ActivePackageSourceConfig : DynamicObject
    {
        /// <summary>
        /// NuGet Package repository
        /// </summary>
        public string PackageSource;

        /// <summary>
        /// Package cache directory
        /// </summary>
        public string PackageCachePath;

        /// <summary>
        /// Delay to wait after the previous update completes before running a new one.
        /// </summary>
        public TimeSpan UpdateDelay;

        /// <summary>
        /// If true, the next update will be skipped.
        /// </summary>
        public bool IsPaused;

        /// <summary>
        /// Maximum package age to consider (relative to the package publication date).
        /// Can be null: Consider all packages, regardless of date.
        /// </summary>
        public TimeSpan MaximumPackageAge;

        /// <summary>
        /// Last time when the update was started.
        /// </summary>
        public DateTimeOffset LastUpdateTime;

    }
}
