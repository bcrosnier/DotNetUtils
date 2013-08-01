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
		/// Regex pattern for project discovery. In match order: Project type Guid, Name, Path, and project Guid.
		/// </summary>
		/// <example>
		/// [assembly: AssemblyVersion( "2.8.14-develop" )]
		/// </example>
		private static readonly string VERSION_ASSEMBLY_PATTERN = @"\[assembly: AssemblyVersion\(\s*\""(\d+\.\d+\.\d+(?:-\w+|\.\d+)?)\""\s*\)\]";

		/// <summary>
		/// Read a SharedAssemblyInfo.cs file, and return AssemblyVersion.
		/// </summary>
		/// <param name="filePath">Path of the SharedAssemblyInfo.cs file. Must exist.</param>
		/// <returns><see cref="System.Version"/></returns>
		public static Version GetAssemblyVersionFromAssemblyInfoFile( string filePath )
		{
			if( String.IsNullOrEmpty( filePath ) )
				throw new ArgumentNullException( "filePath" );
			if( !File.Exists( filePath ) )
				return null;

			StreamReader reader = File.OpenText( filePath );

			string text = reader.ReadToEnd();
			Match m = Regex.Match( text, VERSION_ASSEMBLY_PATTERN );
			if( m.Success )
			{
				return new Version( m.Groups[1].Value );
			}
			else
			{
				//throws error ?
				return null;
			}
		}

	}
}
