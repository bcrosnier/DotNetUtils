namespace ProjectProber
{
    public class CSProjCompileLinkInfo
    {
        public string SharedAssemblyInfoRelativePath { get; private set; }

        public string AssociateLink { get; private set; }

        public string Project { get; private set; }

        public CSProjCompileLinkInfo( string sharedAssemblyInfoRelativePath, string link, string project )
        {
            SharedAssemblyInfoRelativePath = sharedAssemblyInfoRelativePath;
            AssociateLink = link;
            Project = project;
        }

        public override bool Equals( object obj )
        {
            CSProjCompileLinkInfo temp = obj as CSProjCompileLinkInfo;
            return temp != null && this.SharedAssemblyInfoRelativePath == temp.SharedAssemblyInfoRelativePath && this.AssociateLink == temp.AssociateLink;
        }

        public override int GetHashCode()
        {
            return SharedAssemblyInfoRelativePath.GetHashCode() + AssociateLink.GetHashCode() + Project.GetHashCode() ;
        }

        public static bool operator ==( CSProjCompileLinkInfo obj1, CSProjCompileLinkInfo obj2 )
        {
            if( ReferenceEquals( obj1, null ) ) return ReferenceEquals( obj2, null );
            return obj1.Equals( obj2 );
        }

        public static bool operator !=( CSProjCompileLinkInfo obj1, CSProjCompileLinkInfo obj2 )
        {
            return !(obj1 == obj2);
        }
    }
}