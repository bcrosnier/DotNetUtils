using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CK.Core;
using NuGet;
using NUnit.Framework;

namespace NuGetPackageProber.Tests
{
    [TestFixture]
    public class PackageProberTests
    {
        public static readonly DirectoryInfo PACKAGE_CACHE_DIR = new DirectoryInfo( @"package-cache" + Path.DirectorySeparatorChar );
        private IDefaultActivityLogger _logger;

        public PackageProberTests()
        {
            _logger = new DefaultActivityLogger();
            _logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        [Test]
        [Category( "Online" )]
        public void GetRemotePackageInfo()
        {
            // test
            // https://get-package.net/civikey/feed-civikey/api/v2/package/ck.core/2.8.9-develop
            // view-source:https://get-package.net/feed/CiviKey/feed-CiviKey/Packages

            Stream s = RemotePackageManager.GetUrlStream( new Uri( @"https://get-package.net/civikey/feed-civikey/api/v2/package/ck.core/2.8.9-develop" ) );

            IPackage p = new ZipPackage( s );
            Assert.That( p.Id == "CK.Core" );


            CollectionAssert.IsSubsetOf( p.AssemblyReferences.Select( x => x.Path ), p.GetLibFiles().Select( x => x.Path ) );
        }

        [Test]
        [Category( "Online" )]
        public void Choucroute()
        {
            IPackageRepository pr = PackageRepositoryFactory.Default.CreateRepository( @"https://get-package.net/feed/CiviKey/feed-CiviKey" );

            List<IPackage> latestPackages = pr.GetPackages().Where( x => x.IsLatestVersion ).ToList();

            IPackage pCoreTest = pr.FindPackage( "CK.Core" );
            Assert.That( pCoreTest.Id == "CK.Core" );

            CollectionAssert.IsSubsetOf( pCoreTest.AssemblyReferences.Select( x => x.Path ), pCoreTest.GetLibFiles().Select( x => x.Path ) );

            var singleLatestPackages = latestPackages
                .GroupBy( a => a.Id )
                .Select( b => b.Single(
                    c => c.Version == b.Max( d => d.Version )
                    )
                );

            foreach( IPackage package in singleLatestPackages )
            {
                _logger.Info( "Latest {0}: {1}", package.Id, package.Version );

                FileInfo f = StorePackage( package, PACKAGE_CACHE_DIR );

                IPackage p = new ZipPackage( f.FullName );

                Assert.That( package.Id == p.Id );
                Assert.That( package.Version == p.Version );
            }
        }

        public FileInfo StorePackage( IPackage package, DirectoryInfo dir )
        {
            if( !dir.Exists )
                Directory.CreateDirectory( dir.FullName );

            string nupkgPath = Path.Combine( dir.FullName, String.Format( "{0}.{1}.nupkg", package.Id, package.Version.ToString() ) );

            FileInfo outFile = new FileInfo( nupkgPath );

            if( File.Exists( nupkgPath ) )
            {
                _logger.Info( "Already exists." );
                return outFile;
            }

            string tmpNupkgPath = Path.GetTempFileName();

            using( Stream inFileStream = package.GetStream() )
            {
                using( Stream outFileStream = File.OpenWrite( tmpNupkgPath ) )
                {
                    inFileStream.CopyTo( outFileStream );
                }
            }

            File.Move( tmpNupkgPath, nupkgPath );

            Assert.That( outFile.Exists );

            return outFile;
        }
    }
}