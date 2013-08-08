using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using NuGet;
using ProjectProber.Interfaces;

namespace ProjectProber.Impl
{
    /// <summary>
    /// NuGet package reference, as seen in projects' package.config
    /// </summary>
    [DebuggerDisplay( "{Id} {Version} ({TargetFramework})" )]
    public class NuGetPackageReference : INuGetPackageReference
    {
        /// <summary>
        /// Package ID
        /// </summary>
        /// <example>CK.Context</example>
        public string Id { get; private set; }

        /// <summary>
        /// Package descriptive version
        /// </summary>
        /// <example>2.9.1-develop</example>
        public string Version { get; private set; }

        /// <summary>
        /// Target framework object
        /// </summary>
        /// <example>net40-Client</example>
        public FrameworkName TargetFramework { get; private set; }

        internal NuGetPackageReference( string id, string version, string targetFramework )
            : this( id, version, VersionUtility.ParseFrameworkName( targetFramework ) )
        {
        }

        internal NuGetPackageReference( string id, string version, FrameworkName targetFramework )
        {
            Id = id;
            Version = version;
            TargetFramework = targetFramework;
        }

        /// <summary>
        /// Describes the NuGet package reference.
        /// </summary>
        /// <returns>Reference description</returns>
        public override string ToString()
        {
            if( TargetFramework != null )
                return String.Format( "{0}, version {1}, targeting {2}", Id, Version, TargetFramework.ToString() );
            else
                return String.Format( "{0}, version {1}", Id, Version );
        }

        /// <summary>
        /// Cehcks if current object is equal to given object.
        /// </summary>
        /// <param name="obj">Compared object</param>
        /// <returns>True is objects are equal. False otherwise.</returns>
        public override bool Equals( object obj )
        {
            NuGetPackageReference packageRef = obj as NuGetPackageReference;
            return packageRef != null && this.Equals( packageRef );
        }


        /// <summary>
        /// Equal operator
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if A is equal to B. False otherwise.</returns>
        public static bool operator ==( NuGetPackageReference a, NuGetPackageReference b )
        {
            if( System.Object.ReferenceEquals( a, b ) )
            {
                return true;
            }
            if( ((object)a == null) || ((object)b == null) )
            {
                return false;
            }

            return a.Equals( b );
        }
        
        /// <summary>
        /// Is not equal operator
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if A is not equal to B. False otherwise.</returns>
        public static bool operator !=( NuGetPackageReference a, NuGetPackageReference b )
        {
            return !(a == b);
        }

        /// <summary>
        /// Checks if a NuGetPackageReference is equal to this one.
        /// </summary>
        /// <param name="r">NuGetPackageReference to check</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals( NuGetPackageReference r )
        {
            return Id == r.Id && Version == r.Version;
        }

        /// <summary>
        /// Serves as a hash function for this particular type.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + Version.GetHashCode();
                //if( TargetFramework != null )
                //    hash = hash * 23 + TargetFramework.FullName.GetHashCode();
                return hash;
            }
        }
    }
}