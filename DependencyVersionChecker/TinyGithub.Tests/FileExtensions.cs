using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotNetUtilitiesApp.GithubDownloader
{
    public static class FileExtensions
    {
        public static string GetSha( this FileInfo file )
        {
            if( !file.Exists )
                return null;

            using( FileStream fs = file.OpenRead() )
            using( BufferedStream bs = new BufferedStream( fs ) )
            {
                using( SHA1Managed sha1 = new SHA1Managed() )
                {
                    byte[] hash = sha1.ComputeHash( bs );
                    StringBuilder output = new StringBuilder( 2 * hash.Length );
                    foreach( byte b in hash )
                    {
                        output.AppendFormat( "{0:x2}", b );
                    }

                    return output.ToString();
                }
            }
        }
    }
}