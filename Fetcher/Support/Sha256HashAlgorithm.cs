using System;
using System.IO;
using System.Security.Cryptography;

namespace OdjfsScraper.Fetcher.Support
{
    public class Sha256HashAlgorithm : IHashAlgorithm
    {
        public string ComputeHashToString(Stream stream)
        {
            HashAlgorithm hashAlgorithm = new SHA256Managed();
            return BitConverter.ToString(hashAlgorithm.ComputeHash(stream)).Replace("-", "");
        }
    }
}