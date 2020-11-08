using System;
using System.Security.Cryptography;
using Ios.Backup.Decrypter.Library;
using NUnit.Framework;

namespace Ios.Backup.Decrypter.Test
{
    public class EncryptionHelperTests
    {
        /// <summary>
        /// Examples found from debugging the original python code
        /// </summary>

        [Test]
        public void ECBTest()
        {
            var cipher = new byte[] { 100, 153, 10, 128, 199, 47, 202, 174, 108, 192, 223, 241, 145, 78, 4, 239 };
            var key = new byte[] { 25, 87, 240, 15, 147, 254, 0, 66, 60, 253, 85, 166, 220, 188, 114, 91, 138, 137, 87, 7, 91, 5, 36, 44, 177, 147, 103, 144, 174, 206, 70, 127 };

            var result = EncryptionHelper.DecryptAES(cipher, key, CipherMode.ECB);

            var expected = new byte[] { 50, 8, 74, 215, 66, 196, 55, 93, 248, 103, 236, 162, 32, 215, 145, 221 };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Ignore("Not implemented yet")]
        public void CBCTest()
        {
            var cipher = new byte[] {  };
            var key = new byte[] {  };

            var result = EncryptionHelper.DecryptAES(cipher, key, CipherMode.CBC);

            var expected = new byte[] { };

            Assert.AreEqual(expected, result);

            throw new NotImplementedException();
        }
    }
}