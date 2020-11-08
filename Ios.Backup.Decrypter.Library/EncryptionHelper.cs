using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Ios.Backup.Decrypter.Library
{
    public static class EncryptionHelper
    {
        public static byte[] DecryptAES(byte[] cipher, byte[] key, CipherMode mode)
        {
            // Check arguments.
            if (cipher == null || cipher.Length <= 0)
                throw new ArgumentNullException(nameof(cipher));

            if (mode == CipherMode.CBC)
            {
                if (cipher.Length % 16 != 0)
                {
                    Debug.Write("WARN: AESdecryptCBC: data length not /16, truncating");
                    var truncatedEnd = (cipher.Length / 16) * 16;
                    cipher = cipher[new Range(0, truncatedEnd)];
                }
            }

            byte[] decrypted;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                var m_IV = new byte[16];

                rijAlg.Mode = mode;
                rijAlg.KeySize = key.Length * 8;
                rijAlg.Key = key;
                rijAlg.BlockSize = m_IV.Length * 8;
                rijAlg.IV = m_IV;
                rijAlg.Padding = PaddingMode.Zeros;

                using (ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                {
                    using (MemoryStream ms = new MemoryStream(cipher))
                    {
                        using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            decrypted = new byte[cipher.Length];
                            var bytesRead = cryptoStream.Read(decrypted, 0, cipher.Length);

                            decrypted = decrypted.Take(bytesRead).ToArray();
                        }
                    }
                }
            }

            return decrypted;
        }

    }
}
