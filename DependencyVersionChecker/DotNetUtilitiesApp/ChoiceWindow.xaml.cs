using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DotNetUtilitiesApp.WpfUtils;

namespace DotNetUtilitiesApp
{
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged utils
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
        #endregion INotifyPropertyChanged utils

        internal event EventHandler<ChoiceWindowResultEventArgs> ChoiceResult;

        public IEnumerable<object> Selection { get; private set; }

        private bool _complete = false;

        private object _selected;

        public object Selected {
            get { return _selected; }
            private set
            {
                if( value != _selected )
                {
                    _selected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WindowTitle { get; private set; }

        public string Message { get; private set; }

        private ChoiceWindow( string windowTitle, string message, IEnumerable<object> selection )
        {
            Selection = selection;
            Message = message;
            WindowTitle = windowTitle;

            this.DataContext = this;

            InitializeComponent();
        }

        public static ChoiceWindowResult<T> ShowSelectWindow<T>(string windowTitle, string message, IEnumerable<object> selection)
        {
            ChoiceWindow window = new ChoiceWindow( windowTitle, message, selection );

            ChoiceWindowResult<T> choiceResult = null;
            AutoResetEvent waiter = new AutoResetEvent( false );

            window.ChoiceResult += ( s, e ) => {
                choiceResult = new ChoiceWindowResult<T>( (T)e.Selected, e.Result );
                waiter.Set();
            };

            window.ShowDialog();
            waiter.WaitOne();

            return choiceResult;
        }

        private void OK_Click( object sender, RoutedEventArgs e )
        {
            if( Selected != null && ChoiceResult != null )
            {
                _complete = true;
                var args = new ChoiceWindowResultEventArgs( Selected, MessageBoxResult.OK );
                ChoiceResult( this, args );
                this.Close();
            }
        }

        private void Double_Click( object sender, RoutedEventArgs e )
        {
            if( Selected != null && ChoiceResult != null )
            {
                _complete = true;
                var args = new ChoiceWindowResultEventArgs( Selected, MessageBoxResult.OK );
                ChoiceResult( this, args );
                this.Close();
            }
        }

        private void Cancel_Click( object sender, RoutedEventArgs e )
        {
            if( ChoiceResult != null )
            {
                _complete = true;
                var args = new ChoiceWindowResultEventArgs( null, MessageBoxResult.Cancel );
                ChoiceResult( this, args );
                this.Close();
            }
        }

        private void Window_Closing( object sender, CancelEventArgs e )
        {
            if( !_complete && ChoiceResult != null )
            {
                var args = new ChoiceWindowResultEventArgs( null, MessageBoxResult.Cancel );
                ChoiceResult( this, args );
            }
        }
    }

    internal class ChoiceWindowResultEventArgs : EventArgs
    {
        internal object Selected { get; private set; }
        internal MessageBoxResult Result { get; private set; }

        internal ChoiceWindowResultEventArgs( object selected, MessageBoxResult result )
        {
            Selected = selected;
            Result = result;
        }
    }

    public class ChoiceWindowResult<T>
    {
        public T Selected { get; private set; }
        public MessageBoxResult Result { get; private set; }

        internal ChoiceWindowResult( T selected, MessageBoxResult result )
        {
            Result = result;
            Selected = selected;
        }

        internal ChoiceWindowResult( MessageBoxResult result )
        {
            Result = result;
        }
    }
}
