using System;
using System.Collections.Generic;
using System.Dynamic;
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
        public void ActivePackageTaskTest()
        {
            UnityContainer cTask = new UnityContainer();

            string taskCachePath = Path.Combine( Environment.CurrentDirectory, @"task-cache" );

            CKUnityHost.Start( new HostMultiFileRepository( taskCachePath ), cTask );

            AutoResetEvent ev = new AutoResetEvent( false );

            int _ckTaskId = CKUnityHost.RegisterTask( typeof( ActivePackageSourceTask ), "Package source update task", DateTime.UtcNow );

            CKHost.Bus.RegisterOnOneRunCompleted( _ckTaskId, t => { ev.Set(); } );

            ev.WaitOne();

            Directory.Delete( taskCachePath, true );
        }
    }
}
