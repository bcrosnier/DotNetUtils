using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtilitiesApp.WpfUtils
{
    public class StringEventArgs : EventArgs
    {
        public string Content { get; private set; }

        public StringEventArgs( string content )
        {
            Content = content;
        }
    }
}
