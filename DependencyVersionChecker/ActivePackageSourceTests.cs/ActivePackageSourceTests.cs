using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.TaskHost;
using CK.TaskHost.Impl;
using CK.TaskHost.Unity;
using Microsoft.Practices.Unity;
using NuGet;
using NUnit.Framework;

namespace ActivePackageSource.Tests
{
    [TestFixture]
    public class ActivePackageSourceTests
    {
        [Test]
        public void ActivePackageConfigTest()
        {
            UnityContainer cTask = new UnityContainer();
            CKUnityHost.Start( new HostMultiFileRepository( Path.Combine( Environment.CurrentDirectory, @"task-cache" ) ), cTask );

            string packageSource = @"https://get-package.com/feed/CiviKey/feed-CiviKey";
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

            AutoResetEvent ev = new AutoResetEvent( false );

            int _ckTaskId = CKUnityHost.RegisterTask( typeof( ActivePackageSourceTask ), "Package source update task", DateTime.UtcNow, d => { d = config; } );

            CKHost.Bus.RegisterOnOneRunCompleted( _ckTaskId, t => { ev.Set(); } );

            ev.WaitOne();
        }
    }
}
