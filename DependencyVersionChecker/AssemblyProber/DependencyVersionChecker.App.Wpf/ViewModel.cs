using System.ComponentModel;

namespace DependencyVersionCheckerApp.Wpf
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Auto-filled with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string caller = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities
    }
}