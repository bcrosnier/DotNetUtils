// Guids.cs
// MUST match guids.h
using System;

namespace BCrosnier.SolutionCheckerPackage
{
    static class GuidList
    {
        public const string guidSolutionCheckerPackagePkgString = "c8e60d50-8b5d-4538-a6eb-0161197032fb";
        public const string guidSolutionCheckerPackageCmdSetString = "5b45292d-edb2-402c-8754-a9dafdfa3323";
        public const string guidToolWindowPersistanceString = "b991ff18-4979-4367-b0a9-7622ac2a0348";

        public static readonly Guid guidSolutionCheckerPackageCmdSet = new Guid(guidSolutionCheckerPackageCmdSetString);
    };
}