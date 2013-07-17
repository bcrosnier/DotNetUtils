using System;
using DependencyAssembly;
using HostAssembly1;

namespace HostAssembly2
{
    public static class MyHostClass2
    {
        public static Guid GuidFromHost1()
        {
            return MyHostClass.GuidFromDependencyClass();
        }

        public static Guid GuidFromDependency()
        {
            return MyDependencyClass.Choucroute();
        }
    }
}