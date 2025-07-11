﻿using BenchmarkDotNet.Attributes;

namespace PDFPassRecovery.Benchmark
{
    public class BruteForceFunctionsBenchmarks
    {
        const string passStart = "987654";
        const int passLength = 10;
        const string passAlphabet = "0123456789";
        readonly PDFInitPassSettings passSettings = new PDFInitPassSettings(passStart, passLength, passAlphabet);

        #region *************** SINGLE THREADED MANAGED BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        [Benchmark]
        public void TestV1R2Performance()
        {
            BasePasswordData v1r2PasswordData = new BasePasswordData
            {
                Id = new byte[] { 0x18, 0xBB, 0x2B, 0x79, 0xC8, 0x64, 0x5C, 0x2C, 0x95, 0x62, 0x9E, 0xDC, 0x65, 0xFD, 0x47, 0x71 },
                OEntry = new byte[] { 0x2A, 0xF0, 0x94, 0x18, 0x72, 0x84, 0x80, 0x07, 0x13, 0x07, 0x11, 0x41, 0x9A, 0x15, 0x79, 0xB8, 0xCB, 0x7B, 0xA2, 0x1A, 0x66, 0x30, 0x49, 0xC1, 0x5B, 0xCE, 0xDA, 0xD5, 0x51, 0xAE, 0x31, 0x22 },
                P = new byte[] { 0xFC, 0xFF, 0x00, 0x00 },
                R = 2,
                UEntry = new byte[] { 0x3, 0xF9, 0x3B, 0x2F, 0x75, 0xD5, 0xD2, 0x2E, 0x6D, 0xFE, 0x7C, 0xDE, 0xAD, 0xB6, 0x1C, 0xEE, 0xF0, 0x57, 0x90, 0xE1, 0x0C, 0xfC, 0x2C, 0xfA, 0x28, 0xE1, 0x52, 0xEE, 0x3C, 0xC1, 0xCB, 0x8D },
                V = 1
            };
            (string password, long passwordsNum) = PDFPassRecoverLib.BruteForceV1R2Password(v1r2PasswordData, passSettings);
        }

        [Benchmark]
        public void TestV2R3Perfromance()
        {
            PDF14PasswordData v2r3PasswordData = new PDF14PasswordData
            {
                Id = new byte[] { 0x18, 0xBB, 0x2B, 0x79, 0xC8, 0x64, 0x5C, 0x2C, 0x95, 0x62, 0x9E, 0xDC, 0x65, 0xFD, 0x47, 0x71 },
                OEntry = new byte[] { 0x2A, 0xF0, 0x94, 0x18, 0x72, 0x84, 0x80, 0x07, 0x13, 0x07, 0x11, 0x41, 0x9A, 0x15, 0x79, 0xB8, 0xCB, 0x7B, 0xA2, 0x1A, 0x66, 0x30, 0x49, 0xC1, 0x5B, 0xCE, 0xDA, 0xD5, 0x51, 0xAE, 0x31, 0x22 },
                P = new byte[] { 0xFC, 0xFF, 0x00, 0x00 },
                R = 3,
                UEntry = new byte[] { 0x3, 0xF9, 0x3B, 0x2F, 0x75, 0xD5, 0xD2, 0x2E, 0x6D, 0xFE, 0x7C, 0xDE, 0xAD, 0xB6, 0x1C, 0xEE, 0xF0, 0x57, 0x90, 0xE1, 0x0C, 0xfC, 0x2C, 0xfA, 0x28, 0xE1, 0x52, 0xEE, 0x3C, 0xC1, 0xCB, 0x8D },
                V = 2,
                KeyLength = 128
            };
            (string password, long passwordsNum) = PDFPassRecoverLib.BruteForceV2R3Password(v2r3PasswordData, passSettings);
        }

        [Benchmark]
        public void TestV4R4Performance()
        {
            // The "EncryptMetadata" set to false in order to cause the resize of the array
            PDF15PasswordData v4r4PasswordData = new PDF15PasswordData
            {
                Id = new byte[] { 0x18, 0xBB, 0x2B, 0x79, 0xC8, 0x64, 0x5C, 0x2C, 0x95, 0x62, 0x9E, 0xDC, 0x65, 0xFD, 0x47, 0x71 },
                OEntry = new byte[] { 0x2A, 0xF0, 0x94, 0x18, 0x72, 0x84, 0x80, 0x07, 0x13, 0x07, 0x11, 0x41, 0x9A, 0x15, 0x79, 0xB8, 0xCB, 0x7B, 0xA2, 0x1A, 0x66, 0x30, 0x49, 0xC1, 0x5B, 0xCE, 0xDA, 0xD5, 0x51, 0xAE, 0x31, 0x22 },
                P = new byte[] { 0xFC, 0xFF, 0x00, 0x00 },
                R = 4,
                UEntry = new byte[] { 0x3, 0xF9, 0x3B, 0x2F, 0x75, 0xD5, 0xD2, 0x2E, 0x6D, 0xFE, 0x7C, 0xDE, 0xAD, 0xB6, 0x1C, 0xEE, 0xF0, 0x57, 0x90, 0xE1, 0x0C, 0xfC, 0x2C, 0xfA, 0x28, 0xE1, 0x52, 0xEE, 0x3C, 0xC1, 0xCB, 0x8D },
                V = 4,
                KeyLength = 128,
                EncryptMetadata = false
            };
            (string password, long passwordsNum) = PDFPassRecoverLib.BruteForceV2R3Password(v4r4PasswordData, passSettings);
        }
        #endregion

        #region *************** PHYSICAL CORES MANAGED BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        // TODO: To implement
        #endregion

        #region *************** LOGICAL CORES MANAGED BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        // TODO: To implement
        #endregion

        #region *************** SINGLE THREADED NATIVE BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        // TODO: To implement
        #endregion

        #region *************** PHYSICAL CORES NATIVE BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        // TODO: To implement
        #endregion

        #region *************** LOGICAL CORES NATIVE BRUTE FORCE FUNCTIONS BENCHMARKS ***************
        // TODO: To implement
        #endregion
    }
}
