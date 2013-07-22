using System;
using System.IO;
using System.Net;

namespace NuGetPackageProber
{
    public class RemotePackageManager
    {
        public RemotePackageManager()
        {
        }

        public static Stream GetUrlStream( Uri url )
        {
            HttpWebRequest  request  = (HttpWebRequest)WebRequest.Create( url );
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }
    }
}