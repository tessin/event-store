using System;

namespace SqlEventStoreStress
{
    static class Randomness
    {
        public static Random Create()
        {
            var seed = new byte[4];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(seed);
            }
            return new Random(BitConverter.ToInt32(seed, 0));
        }
    }
}
