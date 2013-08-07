using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyGithub.Models;

namespace TinyGithub
{
    /// <summary>
    /// Extension utilities on the Github model
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the blob bytes from this blob info, using its encoding.
        /// </summary>
        /// <param name="blobInfo">Blob info to use</param>
        /// <returns>Blob data</returns>
        public static byte[] GetBlobData( this GitBlobInfo blobInfo )
        {
            if( blobInfo.Encoding == "base64" )
            {
                return Convert.FromBase64String( blobInfo.Content );
            }

            throw new NotImplementedException();
        }
    }
}
