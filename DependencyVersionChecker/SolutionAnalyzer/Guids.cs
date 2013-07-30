// Guids.cs
// MUST match guids.h
using System;

namespace BCrosnier.SolutionAnalyzer
{
    static class GuidList
    {
        public const string guidSolutionAnalyzerPkgString = "b757fef3-6dbc-4722-b990-94424ca533f6";
        public const string guidSolutionAnalyzerCmdSetString = "e7914ec6-29d4-4ab2-9d43-88e873b97e56";

        public static readonly Guid guidSolutionAnalyzerCmdSet = new Guid(guidSolutionAnalyzerCmdSetString);
    };
}