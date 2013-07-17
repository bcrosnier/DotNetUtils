using System;
using System.Collections.Generic;
using DependencyVersionChecker;

namespace DependencyVersionCheckerApp.Wpf
{
    public class AssemblyInfoViewModel
        : ViewModel
    {
        private IAssemblyInfo _assembly;

        private bool _isSelected;
        private bool _isExpanded;
        private string _displayName;
        private IList<AssemblyInfoViewModel> _children;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if( value != _isSelected )
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if( value != _isExpanded )
                {
                    _isExpanded = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            private set
            {
                if( value != _displayName )
                {
                    _displayName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<AssemblyInfoViewModel> Children
        {
            get
            {
                return _children;
            }
        }

        public AssemblyInfoViewModel( IAssemblyInfo assembly )
        {
            if( assembly == null )
            {
                throw new ArgumentNullException( "assembly" );
            }

            _assembly = assembly;
            _children = new List<AssemblyInfoViewModel>();

            _displayName = assembly.AssemblyFullName;

            foreach( IAssemblyInfo dep in assembly.Dependencies )
            {
                _children.Add( new AssemblyInfoViewModel( dep ) );
            }
        }
    }
}