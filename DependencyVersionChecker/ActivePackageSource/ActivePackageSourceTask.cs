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

        public ActivePackageSourceTask( CKTaskBuilder builder )
            : base( builder )
        {
        }

        protected override void Execute()
        {
            ActivePackageSourceConfig c = (ActivePackageSourceConfig)this.DataBag;

            _repository = PackageRepositoryFactory.Default.CreateRepository( this.DataBag.PackageSource );

            Log( "ActivePackageSourceTask started" );

            IEnumerable<IPackage> packages = DownloadPackageInfo();
            List<string> packagePaths = new List<string>();

            foreach( var p in packages )
            {
                packagePaths.Add( DownloadPackage( p ) );
            }

            SetNextRunDate( DateTime.UtcNow.Add( this.DataBag.UpdateDelay ) );
        }

        private IEnumerable<IPackage> DownloadPackageInfo()
        {
            IQueryable<IPackage> packages = _repository.GetPackages();

            DateTime? minimumPackageDate = null;

            if( this.DataBag.LastUpdateTime != null )
            {
                minimumPackageDate = this.DataBag.LastUpdateTime;
            }
            if( minimumPackageDate == null || (DateTime.UtcNow - minimumPackageDate) > this.DataBag.MaximumPackageAge )
            {
                minimumPackageDate = DateTime.UtcNow.Subtract( this.DataBag.MaximumPackageAge );
            }

            this.DataBag.LastUpdateTime = DateTime.UtcNow;
            List<IPackage> newPackages = packages.Where( x => x.Published > minimumPackageDate ).ToList();

            return newPackages;
        }


        private string DownloadPackage( IPackage package )
        {
            string packagePath = Path.Combine( this.DataBag.PackageCachePath, String.Format( "{0}.{1}.nupkg", package.Id, package.Version.ToString() ) );

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
