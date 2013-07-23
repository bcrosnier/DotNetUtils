using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.TaskHost;
using NuGet;

namespace ActivePackageSource
{
    public class ActivePackageSourceTask : CKTask
    {
        private IPackageRepository _repository;
        dynamic Config;

        public ActivePackageSourceTask( CKTaskBuilder builder, dynamic dataBag )
            : base( builder )
        {
        }

        protected override void Execute()
        {
            string packageSource = @"https://get-package.net/feed/CiviKey/feed-CiviKey/";
            string packageCachePath = Path.Combine( Environment.CurrentDirectory, @"package-cache" );
            TimeSpan maxAge = new TimeSpan( 60, 0, 0, 0 );

            Directory.CreateDirectory( packageCachePath );

            ActivePackageSourceConfig config = new ActivePackageSourceConfig()
            {
                PackageSource = packageSource,
                UpdateDelay = new TimeSpan( 0, 10, 0 ),
                PackageCachePath = packageCachePath,
                MaximumPackageAge = maxAge
            };

            Config = config;


            _repository = PackageRepositoryFactory.Default.CreateRepository( this.Config.PackageSource );

            Log( "ActivePackageSourceTask started" );

            IEnumerable<IPackage> packages = DownloadPackageInfo();
            List<string> packagePaths = new List<string>();

            foreach( var p in packages )
            {
                packagePaths.Add( DownloadPackage( p ) );
            }

            SetNextRunDate( DateTime.UtcNow.Add( this.Config.UpdateDelay ) );
        }

        private IEnumerable<IPackage> DownloadPackageInfo()
        {
            IQueryable<IPackage> packages = _repository.GetPackages();

            DateTimeOffset? minimumPackageDate = null;
            DateTimeOffset now = new DateTimeOffset( DateTime.UtcNow );
            DateTimeOffset lastUpdate = this.Config.LastUpdateTime;
            TimeSpan maxPackageAge = this.Config.MaximumPackageAge;

            if( (now - lastUpdate) > maxPackageAge )
            {
                minimumPackageDate = now.Subtract( maxPackageAge );
            }
            else
            {
                minimumPackageDate = lastUpdate;
            }

            this.Config.LastUpdateTime = DateTimeOffset.UtcNow;

            List<IPackage> newPackages = new List<IPackage>();
            foreach( var p in packages )
            {
                if( p.Published >= minimumPackageDate )
                    newPackages.Add( p );
            }
            return newPackages;
        }


        private string DownloadPackage( IPackage package )
        {
            string packagePath = Path.Combine( this.Config.PackageCachePath, String.Format( "{0}.{1}.nupkg", package.Id, package.Version.ToString() ) );

            if( File.Exists( packagePath ) )
            {
                // Log that file exists here
            } else {
                string tmpNupkgPath = Path.GetTempFileName();

                using( Stream inFileStream = package.GetStream() )
                {
                    using( Stream outFileStream = File.OpenWrite( tmpNupkgPath ) )
                    {
                        inFileStream.CopyTo( outFileStream );
                    }
                }

                File.Move( tmpNupkgPath, packagePath );
            }

            return packagePath;
        }
    }
}
