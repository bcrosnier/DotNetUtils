using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DependencyVersionChecker.Tests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void TestRelativePath()
        {
            string targetAbsolutePath = Path.GetFullPath( Path.Combine( Environment.CurrentDirectory, "..", "..", "bin", "Release" ) );
            string relativeFolder = Environment.CurrentDirectory;

            string relativePath = DependencyUtils.MakeRelativePath( targetAbsolutePath, relativeFolder );
            string reconvertedAbsolutePath = DependencyUtils.MakeAbsolutePath( relativePath, relativeFolder );

            Assert.That( reconvertedAbsolutePath == targetAbsolutePath, "New absolute path is correct old one" );
        }

        [Test]
        public void TestHexToString()
        {
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes( bytes );

            string hexstr = DependencyUtils.ByteArrayToHexString( bytes );
            byte[] hexarr = DependencyUtils.HexStringToByteArray( hexstr );

            CollectionAssert.AreEqual( bytes, hexarr, "Could convert hex string back" );
        }
    }
}