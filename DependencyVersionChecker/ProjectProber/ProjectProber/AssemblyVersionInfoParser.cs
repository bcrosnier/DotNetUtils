using CK.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectProber
{
	public class AssemblyVersionInfoParser
	{
		/// <summary>
		///
		/// </summary>
		/// <example>
		/// [assembly: AssemblyVersion( "2.8.14-develop" )]
		/// </example>
		public static readonly Regex VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyVersion\(\s*\""(?<Version>\d+(\.\d+){0,3})\""\s*\)\]", RegexOptions.Compiled );
		/// <summary>
		///
		/// </summary>
		/// <example>
		/// [assembly: AssemblyVersion( "2.8.14-develop" )]
		/// </example>
		public static readonly Regex FILE_VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyFileVersion\(\s*\""(?<Version>\d+(\.\d+){0,3})\""\s*\)\]", RegexOptions.Compiled );
		/// <summary>
		///
		/// </summary>
		/// <example>
		/// [assembly: AssemblyVersion( "2.8.14-develop" )]
		/// </example>
		public static readonly Regex INFO_VERSION_ASSEMBLY_PATTERN = new Regex( @"\[assembly: AssemblyInformationalVersion\(\s*\""s*(?<Version>\d+(\.\d+){2})-(?<Release>[0-9a-z-.]*)?\s*\""\s*\)\]", RegexOptions.Compiled );


		/// <summary>
		/// Read a SharedAssemblyInfo.cs file, and return AssemblyVersion.
		/// </summary>
		/// <param name="filePath">Path of the SharedAssemblyInfo.cs file. Must exist.</param>
		/// <returns><see cref="System.Version"/></returns>
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
				return ( !string.IsNullOrEmpty( m.Groups["version"].Value ) ) ? new Version( m.Groups["version"].Value ) : null;
			}
			else
			{
				//throws error ?
				return null;
			}
		}

		public static SemanticVersion GetSemanticAssemblyVersionFromAssemblyInfoFile( string filePath, Regex regex )
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
				return ( !string.IsNullOrEmpty( m.Groups["version"].Value ) ) ? new SemanticVersion( new Version( m.Groups["version"].Value ), m.Groups["Release"].Value ) : null;
			}
			else
			{
				//throws error ?
				return null;
			}
		}

	}
}
