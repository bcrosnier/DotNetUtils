using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Core;

namespace DotNetUtilitiesApp.WpfUtils
{
    /// <summary>
    /// Logger client that manages strings in a string collection.
    /// </summary>
    public class ListBoxItemCollectionLoggerClient : IActivityLoggerClient
    {
        private int _maxLogEntries;
        private Collection<ListBoxItem> _outputCollection;
        private Thickness _currentPadding = new Thickness( 0, 0, 0, 0 );

        private static readonly object _paddingLock = new object();

        public static readonly int OFFSET_VALUE = 20;
        public static readonly Brush FATAL_COLOR = Brushes.DarkRed;
        public static readonly Brush ERROR_COLOR = Brushes.Red;
        public static readonly Brush WARN_COLOR = Brushes.Orange;
        public static readonly Brush INFO_COLOR = Brushes.Blue;
        public static readonly Brush TRACE_COLOR = Brushes.Gray;

        /// <summary>
        /// Create a new logger client that will add strings to a collection, and clear old entries whenever a maximum number of entries is reached.
        /// </summary>
        /// <param name="targetCollection">Collection to use</param>
        /// <param name="maxLogEntries">Maximum number of entries</param>
        public ListBoxItemCollectionLoggerClient( Collection<ListBoxItem> targetCollection, int maxLogEntries )
        {
            if( targetCollection == null ) throw new ArgumentNullException( "Output TextWriter must exist." );
            _outputCollection = targetCollection;
            _maxLogEntries = maxLogEntries;
        }

        private void AddString( string str, Brush color )
        {
            Invoke.OnAppThread( () =>
            {
                if( _outputCollection.Count >= _maxLogEntries )
                {
                    _outputCollection.RemoveAt( 0 );
                }
                var item = new ListBoxItem();
                item.Content = str;
                item.Foreground = color;
                item.Padding = this._currentPadding;
                item.HorizontalContentAlignment = HorizontalAlignment.Left;
                item.VerticalContentAlignment = VerticalAlignment.Top;

                _outputCollection.Add( item );
            } );
        }

        private static Brush GetColorFromLevel( LogLevel level )
        {
            switch( level )
            {
                case LogLevel.Fatal:
                    return FATAL_COLOR;

                case LogLevel.Error:
                    return ERROR_COLOR;

                case LogLevel.Warn:
                    return WARN_COLOR;

                case LogLevel.Info:
                    return INFO_COLOR;

                case LogLevel.Trace:
                    return TRACE_COLOR;

                default:
                    return INFO_COLOR;
            }
        }

        private void AddString( string str )
        {
            AddString( str, INFO_COLOR );
        }

        public void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            //lock( _paddingLock )
            //{
            //    AddString( "Filter level changed to: " + newValue );
            //}
        }

        public void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            //AddString( "Group closed: " + group.GroupText );
            //if( conclusions != null )
            //{
            //    foreach( var conclusion in conclusions )
            //    {
            //        AddString( "    [" + conclusion.Tag + "] " + conclusion.Text );
            //    }
            //}
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            lock( _paddingLock )
            {
                this._currentPadding.Left -= OFFSET_VALUE;
            }
            //AddString( "Group closing: " + group.GroupText );
            //if( conclusions != null )
            //{
            //    foreach( var conclusion in conclusions )
            //    {
            //        AddString( "    [" + conclusion.Tag + "] " + conclusion.Text );
            //    }
            //}
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            lock( _paddingLock )
            {
                this._currentPadding.Left += OFFSET_VALUE;
                //if( group.IsGroupTextTheExceptionMessage )
                //{
                //    AddString( "Exception group: " + group.GroupText );
                //}
                //else
                //{
                //    AddString( "Group open: " + group.GroupText );
                //}
                AddString( String.Format( "[{0}] => {1}", group.GroupLevel.ToString(), group.GroupText ) );

                if( group.Exception != null )
                {
                    AddString( group.Exception.ToString(), ERROR_COLOR );
                }
            }
        }

        public void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            lock( _paddingLock )
            {
                string message = String.Format( "[{0}] {1}: [{2}] {3}",
                    logTimeUtc.ToLocalTime().ToString( "HH:mm:ss.fff" ),
                    DescribeTraits( tags.AtomicTraits ),
                    level.ToString(),
                    text
                    );
                AddString( message, GetColorFromLevel( level ) );
            }
        }
        private static string DescribeTraits( IEnumerable<CKTrait> traits )
        {
            if( traits == null )
                return String.Empty;

            return String.Join( " ", traits );
        }
    }
}