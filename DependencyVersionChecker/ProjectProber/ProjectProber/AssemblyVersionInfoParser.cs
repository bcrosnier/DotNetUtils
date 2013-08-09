using System;
using System.IO;
using System.Text.RegularExpressions;
using CK.Package;

namespace ProjectProber
{
    public static class AssemblyVersionInfoParser
    {
        /// <summary>
        ///Regex pattern for version discovery. Match : AssemblyVersion in SharedAssemblyInfo.cs or AssemblyInfo.cs.
        /// </summary>
        /// <example>
        /// [assembly: AssemblyVersion( "2.8.14" )]
        /// </example>
        public static readonly Regex VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyVersion\(\s*\""(?<Version>\d+(\.\d+){0,3})\""\s*\)\]", RegexOptions.Compiled );

        /// <summary>
        ///Regex pattern for version discovery. Match : AssemblyFileVersion in SharedAssemblyInfo.cs or AssemblyInfo.cs.
        /// </summary>
        /// <example>
        /// [assembly: AssemblyFileVersion( "2.8.14" )]
        /// </example>
        public static readonly Regex FILE_VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyFileVersion\(\s*\""(?<Version>\d+(\.\d+){0,3})\""\s*\)\]", RegexOptions.Compiled );

        /// <summary>
        ///Regex pattern for version discovery. Match : AssemblyInformationVersion in SharedAssemblyInfo.cs or AssemblyInfo.cs.
        /// </summary>
        /// <example>
        /// [assembly: AssemblyInformationalVersion( "2.8.14-develop" )]
        /// </example>
        public static readonly Regex INFO_VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyInformationalVersion\(\s*\""s*(?<Version>\d+(\.\d+){0,3})-?(?<Release>[0-9a-z-.]*)?\s*\""\s*\)\]", RegexOptions.Compiled );

        /// <summary>
        /// Read a version in SharedAssemblyInfo.cs or AssemblyInfo.cs file.
        /// </summary>
        /// <param name="filePath">Path of File. Must exist.</param>
        /// <returns><see cref="System.Version"/> or null if not found</returns>
        public static Version GetAssemblyVersionFromAssemblyInfoFile( string filePath, Regex regex )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new ArgumentNullException( "filePath" );
            if( !File.Exists( filePath ) )
                return null;

            StreamReader reader = File.OpenText( filePath );

            string text = reader.ReadToEnd();
            Match m = regex.Match( text );
            if( m.Success )
            {
                return (!string.IsNullOrEmpty( m.Groups["Version"].Value )) ? new Version( m.Groups["Version"].Value ) : null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Read a version in SharedAssemblyInfo.cs or AssemblyInfo.cs file.
        /// </summary>
        /// <param name="filePath">Path of File. Must exist.</param>
        /// <returns><see cref="CK.Package.SemanticVersion"/> or null if not found</returns>
        public static string GetSemanticAssemblyVersionFromAssemblyInfoFile( string filePath, Regex regex )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new ArgumentNullException( "filePath" );
            if( !File.Exists( filePath ) )
                return null;

            StreamReader reader = File.OpenText( filePath );

            string text = reader.ReadToEnd();
            Match m = regex.Match( text );
            if( m.Success )
            {
                return (!string.IsNullOrEmpty( m.Groups["Release"].Value )) ? m.Groups["Version"].ToString() + "-" + m.Groups["Release"].Value.ToString() : m.Groups["Version"].ToString();
            }
            else
            {
                return null;
            }
        }
    }
}