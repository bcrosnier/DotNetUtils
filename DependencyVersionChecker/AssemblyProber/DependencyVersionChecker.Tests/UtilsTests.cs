using System;
using System.IO;
using NUnit.Framework;

namespace AssemblyProber.Tests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void TestRelativePath()
        {
            string targetAbsolutePath = Path.GetFullPath( Path.Combine( Environment.CurrentDirectory, "..", "..", "bin", "Release" ) );
            string relativeFolder = Environment.CurrentDirectory;

            string relativePath = StringUtils.MakeRelativePath( targetAbsolutePath, relativeFolder );
            string reconvertedAbsolutePath = StringUtils.MakeAbsolutePath( relativePath, relativeFolder );

            Assert.That( reconvertedAbsolutePath == targetAbsolutePath, "New absolute path is correct old one" );
        }

        [Test]
        public void TestHexToString()
        {
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes( bytes );

            string hexstr = StringUtils.ByteArrayToHexString( bytes );
            byte[] hexarr = StringUtils.HexStringToByteArray( hexstr );

            CollectionAssert.AreEqual( bytes, hexarr, "Could convert hex string back" );
        }
    }
}