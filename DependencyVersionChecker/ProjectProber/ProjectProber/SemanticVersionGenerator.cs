using System;
using CK.Package;

namespace ProjectProber
{
    /// <summary>
    /// Utilities to generate a semantic version
    /// </summary>
    public static class SemanticVersionGenerator
    {
        /// <summary>
        /// Utilities to generate a semantic version
        /// </summary>
        /// <param name="version">Current version</param>
        /// <param name="publicBreakingChange">True, if the new project's version has a breaking change or a lot of new functionality.</param>
        /// <param name="deprecatedOrNewFunction">True, if the new project's version has deprecated or new function(s).</param>
        /// <param name="bugFixe">True, if the new project's version has a bug fix.</param>
        /// <param name="preRelease">True, if the new project's version is a pre-release.</param>
        /// <returns>New semantic version</returns>
        /// <remarks>don't support metadata in semantic versioning</remarks>
        public static SemanticVersion GenerateSemanticVersion( SemanticVersion version,
            bool publicBreakingChange,
            bool deprecatedOrNewFunction,
            bool bugFixe,
            string preRelease )
        {
            if( publicBreakingChange ) return new SemanticVersion( version.Version.Major + 1, 0, 0, preRelease );
            if( deprecatedOrNewFunction ) return new SemanticVersion( version.Version.Major, version.Version.Minor + 1, 0, preRelease );
            if (bugFixe) return new SemanticVersion(version.Version.Major, version.Version.Minor, version.Version.Build + 1, preRelease);
            SemanticVersion temp = new SemanticVersion(version.Version.Major, version.Version.Minor, version.Version.Build, preRelease);
            if (version < temp ) return temp;
            return new SemanticVersion( new Version( 0, 0, 0 ) );
        }
    }
}