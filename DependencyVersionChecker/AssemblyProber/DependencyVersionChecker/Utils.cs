using System;
using System.IO;

namespace AssemblyProber
{
    /// <summary>
    /// Various static string- and path- related utilities.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Converts an absolute path to a relative path, relative to a given folder.
        /// </summary>
        /// <param name="absoluteTargetPath">Absolute path to convert</param>
        /// <param name="relativeTo">Folder the path should be relative to</param>
        /// <returns></returns>
        public static string MakeRelativePath( string absoluteTargetPath, string relativeTo )
        {
            Uri source = new Uri( absoluteTargetPath );
            Uri folder = new Uri( relativeTo + Path.DirectorySeparatorChar );

            return folder.MakeRelativeUri( source ).ToString();
        }

        /// <summary>
        /// Converts a relative path, relative to a given folder, to an absolute path.
        /// </summary>
        /// <param name="relativeTargetPath">Relative path to convert</param>
        /// <param name="relativeTo">Folder the path is relative to</param>
        /// <returns></returns>
        public static string MakeAbsolutePath( string relativeTargetPath, string relativeTo )
        {
            Uri uri = new Uri( Path.Combine( relativeTo, relativeTargetPath ) );
            return Path.GetFullPath( uri.AbsolutePath );
        }

        /// <summary>
        /// Converts a byte array to its hexadecimal lowercase string representation.
        /// </summary>
        /// <param name="bytes">Bytes array to convert</param>
        /// <returns>Lowercase hexadecimal representation, or an empty string if bytes was null.</returns>
        public static string ByteArrayToHexString( byte[] bytes )
        {
            if( bytes == null )
                return String.Empty;

            return BitConverter.ToString( bytes ).Replace( "-", string.Empty ).ToLowerInvariant();
        }

        /// <summary>
        /// Converts an hexadecimal string representation to a byte array.
        /// From http://stackoverflow.com/questions/854012/how-to-convert-hex-to-a-byte-array
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Converted Byte array, or empty byte array if str was empty.</returns>
        public static byte[] HexStringToByteArray( string str )
        {
            if( String.IsNullOrEmpty( str ) )
                return new byte[0];

            int offset = str.StartsWith( "0x" ) ? 2 : 0;
            if( (str.Length % 2) != 0 )
            {
                throw new ArgumentException( "Invalid length: " + str.Length );
            }
            byte[] ret = new byte[(str.Length - offset) / 2];

            for( int i = 0; i < ret.Length; i++ )
            {
                ret[i] = (byte)((ParseNybble( str[offset] ) << 4)
                                 | ParseNybble( str[offset + 1] ));
                offset += 2;
            }
            return ret;
        }

        /// <summary>
        /// Converts an hexadecimal character to its integer representation.
        /// From http://stackoverflow.com/questions/854012/how-to-convert-hex-to-a-byte-array
        /// </summary>
        /// <param name="c">Character to convert (0-9/A-F)</param>
        /// <returns>Integer representation of the hexadecimal character.</returns>
        private static int ParseNybble( char c )
        {
            if( c >= '0' && c <= '9' )
            {
                return c - '0';
            }
            if( c >= 'A' && c <= 'F' )
            {
                return c - 'A' + 10;
            }
            if( c >= 'a' && c <= 'f' )
            {
                return c - 'a' + 10;
            }
            throw new ArgumentException( "Invalid hex digit: " + c );
        }
    }
}