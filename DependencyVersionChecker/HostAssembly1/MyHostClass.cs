using System;
using DependencyAssembly;

namespace HostAssembly1
{
    public class MyHostClass
    {
        public MyHostClass()
        {
            Console.WriteLine( MyDependencyClass.Choucroute() );
        }

        public static Guid GuidFromDependencyClass()
        {
            return MyDependencyClass.Choucroute();
        }
    }
}