using System;

namespace PDFPassRecovery
{
    /// <summary>
    /// Customized implementation of the RC4 algo to speed up the password recovery process
    /// Optimization 1: A shadow state array is initialized in the constructor and then copied for every initialization
    /// Optimization 2: As the state array length is 256 bytes, the modulo operation is replaced with &0xFF operation
    /// </summary>
    class OptimizedRC4
    {
        // Size of the state array
        const int STATE_ARRAY_SIZE = 256;

        // State array
        private readonly byte[] shadowS = new byte[STATE_ARRAY_SIZE];
        private byte[] S = new byte[STATE_ARRAY_SIZE];

        public OptimizedRC4()
        {
            for (int i = 0; i < STATE_ARRAY_SIZE; i++)
            {
                shadowS[i] = (byte)i;
            }
        }

        // Key scheduling algorithm (KSA)
        public void Initialize(byte[] key)
        {
            Buffer.BlockCopy(shadowS, 0, S, 0, STATE_ARRAY_SIZE);

            byte temp;
            int j = 0;
            for (int i = 0; i < STATE_ARRAY_SIZE; i++)
            {
                j = (j + S[i] + key[i % key.Length]) & 0xFF;

                // Swap S[i] and S[j]
                temp = S[i];
                S[i] = S[j];
                S[j] = temp;
            }
        }

        public void Encrypt(byte[] input, byte[] output)
        {
            byte temp;
            int i = 0;
            int j = 0;

            for (int cnt = 0; cnt < input.Length; cnt++)
            {
                i = (i + 1) & 0xFF;
                j = (j + S[i]) & 0xFF;

                // Swap S[i] and S[j]
                temp = S[i];
                S[i] = S[j];
                S[j] = temp;

                output[cnt] = (byte)(input[cnt] ^ S[(S[i] + S[j]) & 0xFF]);
            }
        }
    }
}
