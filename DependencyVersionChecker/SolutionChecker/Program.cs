using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CK.Core;
using ProjectProber;

namespace SolutionChecker
{
    class Program
    {
        private IActivityLogger _logger;

        static void Main( string[] args )
        {
            Program program = new Program();

            program.Run( args );
        }

        private Program()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Tap.Register( new ActivityLoggerConsoleSink() );
            _logger = logger;
        }

        private void Run( string[] args )
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if( args.Length > 0 )
            {
                string path = args[0];

                string solutionName = Path.GetFileNameWithoutExtension( path );
                string outputXmlFilename = solutionName + ".SolutionCheckResult.xml";

                SolutionCheckResult result = ProjectProber.SolutionChecker.CheckSolutionFile( path );

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("\t");
                using( XmlWriter xw = XmlWriter.Create( outputXmlFilename, settings ) )
                {
                    xw.WriteStartDocument( true );
                    xw.WriteProcessingInstruction( "xml-stylesheet", "type='text/xsl' href='SolutionCheckResult.xslt'" );
                    result.SerializeTo( xw );
                    xw.WriteEndDocument();
                }

                Process.Start( outputXmlFilename );
            }
            else
            {
                _logger.Fatal( "Open with a file path argument." );
                Console.ReadLine();
            }
        }
    }
}
