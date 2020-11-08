using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Claunia.PropertyList;

namespace Ios.Backup.Decrypter.Library
{
    public class KeyBag
    {
        private string[] _tags = new[] { nameof(ClassKey.WRAP), nameof(ClassKey.CLAS), nameof(ClassKey.KTYP), nameof(ClassKey.WPKY) };
        private int? Type { get; set; }
        private int? Wrap { get; set; }
        private byte[] UUID { get; set; }
        private Dictionary<string, byte[]> Attr { get; set; } = new Dictionary<string, byte[]>();
        private Dictionary<int, ClassKey> ClassKeys { get; set; } = new Dictionary<int, ClassKey>();

        private int _WRAP_PASSPHRASE = 2;

        public KeyBag(NSData data)
        {
            Parsekeybag(data);
        }

        public bool UnlockWithPassphrase(string passPhrase)
        {
            byte[] bytes;

            using (var deriveBytes = new Rfc2898DeriveBytes(passPhrase, Attr["DPSL"], GetInt(Attr["DPIC"]), HashAlgorithmName.SHA256))
            {
                bytes = deriveBytes.GetBytes(32);
            }

            var passphrase_round1 = bytes;

            using (var deriveBytes = new Rfc2898DeriveBytes(passphrase_round1, Attr["SALT"], GetInt(Attr["ITER"]), HashAlgorithmName.SHA1))
            {
                bytes = deriveBytes.GetBytes(32);
            }

            var passphrase_key = bytes;

            foreach (var classKey in ClassKeys)
            {
                if (classKey.Value.WPKY == null)
                {
                    continue;
                }

                if (classKey.Value.WRAP == _WRAP_PASSPHRASE)
                {
                    var k = AESUnwrap(passphrase_key, classKey.Value.WPKY);

                    if (k == null)
                    {
                        return false;
                    }

                    classKey.Value.Key = k;
                }
            }

            return true;
        }

        private byte[] AESUnwrap(byte[] kek, byte[] wrapped)
        {
            var C = new List<ulong>();
            var test = Enumerable.Range(0, wrapped.Length / 8);

            foreach (var i in test)
            {
                C.Add(Unpack64Bit(wrapped[new Range(i * 8, i * 8 + 8)]));
            }

            var n = C.Count - 1;
            var r = new ulong[n + 1];
            var a = C[0];

            for (int i = 1; i < n + 1; i++)
            {
                r[i] = C[i];
            }

            foreach (int j in Enumerable.Range(0, 6).Reverse())
            {
                foreach (int i in Enumerable.Range(1, n).Reverse())
                {
                    var first = Pack64Bit(a ^ (ulong)(n * j + i)).Reverse();
                    var second = Pack64Bit(r[i]).Reverse();

                    var todec = first.Concat(second).ToArray();

                    var b = EncryptionHelper.DecryptAES(todec, kek, CipherMode.ECB);

                    a = Unpack64Bit(b.Take(8).ToArray());
                    r[i] = Unpack64Bit(b.Skip(8).ToArray());
                }
            }

            if ((ulong)a != 0xa6a6a6a6a6a6a6a6)
            {
                return null;
            }

            var res = r.Skip(1).Select(Pack64Bit).SelectMany(m => m.Reverse()).ToArray();
            return res;
        }

        private byte[] Pack64Bit(ulong l)
        {
            return StructConverter.Pack(new object[] { l });
        }

        private ulong Unpack64Bit(byte[] bytes)
        {
            return (ulong)StructConverter.Unpack(">Q", bytes.Reverse().ToArray())[0];
        }

        private void Parsekeybag(NSData nsData)
        {

            ClassKey currentClassKey = null;

            foreach (var (tag, data) in LoopTLVBlocks(nsData))
            {
                var dataAsInt = 0;

                if (data.Length == 4)
                {
                    dataAsInt = GetInt(data);
                }

                if (tag == "TYPE")
                {
                    Type = dataAsInt;
                    if (Type > 3)
                    {
                        throw new Exception($"FAIL: keybag type > 3 : {Type}");
                    }

                }
                else if (tag == "UUID" && UUID == null)
                {
                    UUID = data;
                }
                else if (tag == "WRAP" && Wrap == null)
                {
                    Wrap = dataAsInt;
                }
                else if (tag == "UUID")
                {
                    if (currentClassKey != null)
                    {
                        ClassKeys.Add(currentClassKey.CLAS, currentClassKey);
                    }

                    currentClassKey = new ClassKey { UUID = data };
                }
                else if (_tags.Contains(tag) && currentClassKey != null)
                {
                    if (tag == nameof(ClassKey.CLAS))
                    {
                        currentClassKey.CLAS = dataAsInt;
                    }
                    else if (tag == nameof(ClassKey.KTYP))
                    {
                        currentClassKey.KTYP = dataAsInt;
                    }
                    else if (tag == nameof(ClassKey.WPKY))
                    {
                        currentClassKey.WPKY = data;
                    }
                    else if (tag == nameof(ClassKey.WRAP))
                    {
                        currentClassKey.WRAP = dataAsInt;
                    }

                }
                else
                {
                    Attr.Add(tag, data);
                }
            }

            if (currentClassKey != null)
            {
                ClassKeys.Add(currentClassKey.CLAS, currentClassKey);
            }
        }

        private class ClassKey
        {
            public int CLAS { get; set; }
            public byte[] UUID { get; set; }
            public int? WRAP { get; set; }
            public int? KTYP { get; set; }
            public byte[] WPKY { get; set; }
            public byte[] Key { get; set; }
        }

        private IEnumerable<(string, byte[])> LoopTLVBlocks(NSData nsData)
        {
            var blob = nsData.Bytes;

            var i = 0;
            while (i + 8 <= blob.Length)
            {
                var tag = blob[new Range(i, i + 4)];
                var length = GetInt(blob[new Range(i + 4, i + 8)]);
                var data = blob[new Range(i + 8, i + 8 + length)];
                yield return (Encoding.ASCII.GetString(tag), data);
                i += 8 + length;
            }
        }

        private static int GetInt(byte[] bytes)
        {
            return (int)(uint)StructConverter.Unpack(">L", bytes.Reverse().ToArray())[0];
        }

        public byte[] UnwrapKeyForClass(int manifestClass, byte[] manifestKey)
        {
            var ck = ClassKeys[manifestClass].Key;

            if (ck == null)
            {
                throw new Exception("Key not found, did you provide the correct pass phrase?");
            }

            if (manifestKey.Length != 0x28)
            {
                throw new Exception("Invalid key length");
            }

            return AESUnwrap(ck, manifestKey);
        }
    }
}
