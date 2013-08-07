using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyGithub;
using TinyGithub.Models;

namespace ProjectProber
{
    public class GithubRepositoryManager
    {
        private Github _github;

        public GithubRepositoryManager()
        {
            _github = new Github();
        }

        public void SetApiToken( string apiToken )
        {
            _github.SetApiToken( apiToken );
        }

        public void DownloadHeadRefTestFiles( string author, string repoName, string refName, string baseDirectory )
        {
            string directoryPath = Path.Combine( baseDirectory, repoName );
            string resource = String.Format( "repos/{0}/{1}/git/refs/heads/{2}", author, repoName, refName );

            GithubResponse<GithubRef> response = _github.GithubRequest<GithubRef>( resource );

            if( response.StatusCode != System.Net.HttpStatusCode.OK )
            {
                // TODO: Error management
            }
            else
            {
                GithubRef refObject = response.Content;

                Debug.Assert( refObject.Object.Type == "commit" );

                GithubCommit headCommit = refObject.Object.ResolveAs<GithubCommit>( _github );
                GithubTreeInfo treeInfo = headCommit.Tree.Resolve( _github, true );

                IEnumerable<GitCommitObject> fileObjects = treeInfo.Tree.Where( x => x.Type == "blob" );

                List<GitCommitObject> objectsToGet = new List<GitCommitObject>();

                // Get solution files from commit tree:
                // Solution files
                objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( ".sln", StringComparison.InvariantCultureIgnoreCase ) ) );
                // AssemblyInfo files (also includes SharedAssemblyInfo)
                objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( "AssemblyInfo.cs", StringComparison.InvariantCultureIgnoreCase ) ) );
                // C# project files
                objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( ".csproj", StringComparison.InvariantCultureIgnoreCase ) ) );
                // NuGet package configuration files
                objectsToGet.AddRange( fileObjects.Where( x => x.Path.EndsWith( "packages.config", StringComparison.InvariantCultureIgnoreCase ) ) );
                
                foreach( var objectToGet in objectsToGet )
                {
                    string destPath = Path.Combine( directoryPath, objectToGet.Path );

                    GitBlobInfo blobInfo = objectToGet.ResolveAs<GitBlobInfo>( _github );

                    string sourceUrl = objectToGet.Url;

                    _github.DownloadBlobInfo( blobInfo, destPath );
                }
            }
        }
    }
}
