using System;
using System.IO;

namespace DependencyVersionChecker
{
    public static class DependencyUtils
    {
        public static string MakeRelativePath( string absoluteTargetPath, string relativeTo )
        {
            Uri source = new Uri( absoluteTargetPath );
            Uri folder = new Uri( relativeTo + Path.DirectorySeparatorChar );

            return folder.MakeRelativeUri( source ).ToString();
        }

        public static string MakeAbsolutePath( string relativeTargetPath, string relativeTo )
        {
            Uri uri = new Uri( Path.Combine( relativeTo, relativeTargetPath ) );
            return Path.GetFullPath( uri.AbsolutePath );
        }

        public static string ByteArrayToHexString( byte[] bytes )
        {
            if( bytes == null )
                return String.Empty;

            return BitConverter.ToString( bytes ).Replace( "-", string.Empty ).ToLowerInvariant();
        }

        /// <summary>
        /// From http://stackoverflow.com/questions/854012/how-to-convert-hex-to-a-byte-array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
        /// From http://stackoverflow.com/questions/854012/how-to-convert-hex-to-a-byte-array
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
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