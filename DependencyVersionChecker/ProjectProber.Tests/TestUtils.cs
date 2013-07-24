using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace ProjectProber.Tests
{
    public static class TestUtils
    {
        public static IActivityLogger CreateLogger()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger(false);

            logger.Tap.Register( new ActivityLoggerConsoleSink() );

            return logger;
        }
    }
}
