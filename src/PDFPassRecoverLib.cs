using System;
using System.IO;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace PDFPassRecovery
{
    public static class PDFPassRecoverLib
    {
        /// <summary>
        /// Method generates a padded password per PDF 1.2~ PDF 1.6 specs based on the parameters provided- password length, alphabet and start string
        /// </summary>
        /// <param name="passwordLength">Length of the password to generate</param>
        /// <param name="alphabet">Alphabet (symbols) to use for password generation</param>
        /// <param name="startPassword">A string to start with</param>
        /// <returns>IEnumerable collection for a byte array</returns>
        /// <exception cref="ArgumentNullException">Exception is thrown, if the string to strat with is null</exception>
        /// <exception cref="ArgumentException">Exception is thrown, if the string to start with is longer, then the password length</exception>
        public static IEnumerable<byte[]> GeneratePaddedPassword(int passwordLength, string alphabet, string startPassword)
        {
            // Padding string for security handlers of revision 2~ 4
            byte[] PADDING_STRING = { 0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A };

            const int PADDED_PASSWORD_LENGTH = 32;
            byte[] paddedPassword = new byte[PADDED_PASSWORD_LENGTH];

            // Length of the password and the input alphabet
            int k = passwordLength - startPassword.Length;
            int n = alphabet.Length;

            byte[] alphabetAsBytes = Encoding.ASCII.GetBytes(alphabet);
            byte[] startPasswordAsBytes = Encoding.ASCII.GetBytes(startPassword);

            // Creating a string to start with
            string passwordPadding = new string(alphabet[0], k);
            byte[] permutations = Encoding.ASCII.GetBytes(passwordPadding);

            int[] indexArray = new int[k];

            // Copying to the output array constant data
            Buffer.BlockCopy(startPasswordAsBytes, 0, paddedPassword, 0, startPasswordAsBytes.Length);
            Buffer.BlockCopy(PADDING_STRING, 0, paddedPassword, startPasswordAsBytes.Length + permutations.Length, PADDED_PASSWORD_LENGTH - startPasswordAsBytes.Length - permutations.Length);

            // Main cycle for passwords generation
            while (true)
            {
                Buffer.BlockCopy(permutations, 0, paddedPassword, startPasswordAsBytes.Length, permutations.Length);

                yield return paddedPassword;

                int i = k - 1;
                for (; i >= 0; i--)
                {
                    if (indexArray[i] < n - 1) break;
                }
                if (i < 0) break;

                indexArray[i] += 1;
                i++;
                for (; i < k;)
                {
                    indexArray[i] = 0;
                    i++;
                }

                // Selecting a next byte from the alphabet
                for (int j = 0; j < k; j++)
                {
                    int alphaIndex = indexArray[j];
                    permutations[j] = alphabetAsBytes[alphaIndex];
                }
            }
        }

        /// <summary>
        /// Method generates a padded password per PDF 1.7 specs based on the parameters provided- password length, alphabet and start string
        /// </summary>
        /// <param name="passwordLength">Length of the password to generate</param>
        /// <param name="alphabet">Alphabet (symbols) to use for password generation</param>
        /// <param name="startPassword">A string to start with</param>
        /// <returns>IEnumerable collection for a byte array</returns>
        public static IEnumerable<byte[]> GeneratePasswordAsBytes(int passwordLength, string alphabet, string startPassword)
        {
            // Length of the password and the input alphabet
            int k = passwordLength - startPassword.Length;
            int n = alphabet.Length;

            byte[] fullPassword = new byte[passwordLength];

            byte[] alphabetAsBytes = Encoding.UTF8.GetBytes(alphabet);
            byte[] startPasswordAsBytes = Encoding.UTF8.GetBytes(startPassword);

            // Creating a string to start with
            string passwordString = new string(alphabet[0], k);
            byte[] permutations = Encoding.UTF8.GetBytes(passwordString);

            // Array to keep indexes of every symbol in the alhpabet
            int[] indexArray = new int[k];

            Buffer.BlockCopy(startPasswordAsBytes, 0, fullPassword, 0, startPasswordAsBytes.Length);

            // Main cycle for passwords generation
            while (true)
            {
                Buffer.BlockCopy(permutations, 0, fullPassword, startPasswordAsBytes.Length, permutations.Length);

                yield return fullPassword;

                int i = k - 1;
                for (; i >= 0; i--)
                {
                    if (indexArray[i] < n - 1) break;
                }
                if (i < 0) break;

                indexArray[i] += 1;
                i++;
                for (; i < k;)
                {
                    indexArray[i] = 0;
                    i++;
                }

                // Selecting a next byte from the alphabet
                for (int j = 0; j < k; j++)
                {
                    int alphaIndex = indexArray[j];
                    permutations[j] = alphabetAsBytes[alphaIndex];
                }
            }
        }

        /// <summary>
        /// Method measures the password restore function execution time
        /// </summary>
        /// <typeparam name="T">Generic parameter for a password data class and its childs</typeparam>
        /// <param name="bruteForceFunction">Delegate for the brute force function</param>
        /// <param name="passwordData">BasePasswordData object</param>
        /// <param name="passwordSettings">Password settings read from the configuration file</param>
        /// <exception cref="InvalidDataException">The exception is raised, if the deleagate for the brute force function is null</exception>
        public static void RestoreEncryptedPassword<T>(Func<T, PDFInitPassSettings, (string, long)> bruteForceFunction, T passwordData, PDFInitPassSettings passwordSettings) where T : SecHandlerR2PasswordData
        {
            Console.WriteLine($"{Environment.NewLine}Starting to restore the password...");

            int passwordLength = passwordSettings.PasswordLength;
            string alphabet = passwordSettings.Alphabet;
            string startPassword = passwordSettings.StartPassword;

            Console.WriteLine($"Password length:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{passwordLength}");
            Console.WriteLine($"Starting password:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"\"{startPassword}\"");
            Console.WriteLine($"Starting password length:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{startPassword.Length}");

            long totalPasswords = CalculateTotalPasswordsNumber(passwordLength - startPassword.Length, alphabet.Length);
            Console.WriteLine($"Total number of passwords to check:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{totalPasswords:N0}");

            Console.WriteLine();
            DateTime startTime = DateTime.Now;
            Console.WriteLine($"Brute force started at:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{startTime}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            string password;
            long passwordsChecked;

            if (!(bruteForceFunction is null))
            {
                (password, passwordsChecked) = bruteForceFunction.Invoke(passwordData, passwordSettings);
                if (password == string.Empty)
                {
                    PrintColoredText($"The password has not been found :(", ConsoleColor.Red);
                }
                else
                {
                    PrintColoredText($"The password is:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{password}{Environment.NewLine}", ConsoleColor.Green, false);
                }
            }
            else
            {
                throw new InvalidDataException($"The brute force function is not provided");
            }

            sw.Stop();
            TimeSpan elapsed = sw.Elapsed;

            DateTime endTime = DateTime.Now;
            Console.WriteLine($"Brute force ended at:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{endTime}");

            Console.WriteLine($"{Environment.NewLine}*************** STATISTICS ***************");
            Console.WriteLine($"Total elapsed time:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{elapsed.TotalSeconds:N1} seconds");

            Console.WriteLine($"Passwords checked:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{passwordsChecked:N0}");
            double passwordsCheckPerSec = passwordsChecked / elapsed.TotalSeconds;
            Console.WriteLine($"Performance:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR) + $"{passwordsCheckPerSec:N0} password checks per second");

            Console.WriteLine($"{Environment.NewLine}Have a nice day!");
        }

        #region ***** PASSWORDS BRUTE FORCE FUNCTIONS SECTION *****
        public static (string, long) BruteForceR2SecHandler(SecHandlerR2PasswordData passwordData, PDFInitPassSettings passwordSettings)
        {
            byte[] PADDING_STRING = { 0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A };
            const int RC4_KEY_SIZE = 5;
            const int PADDED_PASSWORD_LENGTH = 32;

            // Variables for the RC4 encryption
            byte[] rc4encryptionKey = new byte[RC4_KEY_SIZE];
            byte[] rc4outBuffer = new byte[PADDING_STRING.Length];

            // Pre-filling the constant data into the array used to calculate the MD5 hash
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.Id.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.Id, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.Id.Length);

            long passwordsCounter = 0;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
                passwordsCounter++;

                MD5 md5 = MD5.Create();

                // Copy the password as a byte array into the full array for MD5 calculation
                Buffer.BlockCopy(paddedPassword, 0, fullInputForMd5Hash, 0, paddedPassword.Length);
                md5.ComputeHash(fullInputForMd5Hash);

                // Extract 5 bytes for the RC4 encryption key
                Buffer.BlockCopy(md5.Hash, 0, rc4encryptionKey, 0, RC4_KEY_SIZE);

                OptimizedRC4 rc4 = new OptimizedRC4();
                rc4.Initialize(rc4encryptionKey);
                rc4.Encrypt(PADDING_STRING, rc4outBuffer);

                if (SimpleByteArraysCompare(rc4outBuffer, passwordData.UEntry))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }
            }

            // No password found
            return (string.Empty, passwordsCounter);
        }

        public static (string, long) BruteForceR3SecHandler(SecHandlerR3PasswordData passwordData, PDFInitPassSettings passwordSettings)
        {
            byte[] PADDING_STRING = { 0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A };
            const int MD5_HASH_SIZE = 16;
            const int PADDED_PASSWORD_LENGTH = 32;
            const int U_ENTRY_LENGTH_FOR_CHECK = 16;

            // Extracting the first 16 bytes of the U-entry
            byte[] uEntry16 = new byte[U_ENTRY_LENGTH_FOR_CHECK];
            Buffer.BlockCopy(passwordData.UEntry, 0, uEntry16, 0, U_ENTRY_LENGTH_FOR_CHECK);

            // Variables for the RC4 encryption
            byte[] rc4encryptionKey;
            byte[] rc4newKey = new byte[MD5_HASH_SIZE];
            byte[] rc4outBuffer = new byte[MD5_HASH_SIZE];

            // Variables for the MD5 encryption
            byte[] newMD5Hash;

            // Pre-filling the constant data into the array used to calculate the MD5 hash
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.Id.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.Id, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.Id.Length);

            // Step 2 from Algorigthm 3.5
            MD5 md35 = MD5.Create();
            md35.TransformBlock(PADDING_STRING, 0, PADDING_STRING.Length, null, 0);
            md35.TransformFinalBlock(passwordData.Id, 0, passwordData.Id.Length);

            // Passwords counter
            long passwordsCounter = 0;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
                passwordsCounter++;

                // Copy the password as a byte array into the full array for MD5 calculation
                MD5 newMD5 = MD5.Create();
                Buffer.BlockCopy(paddedPassword, 0, fullInputForMd5Hash, 0, paddedPassword.Length);
                newMD5.ComputeHash(fullInputForMd5Hash);
                newMD5Hash = newMD5.Hash;

                // New MD5 instance for calculation of the MD5 hash 50 times
                MD5 newMD50 = MD5.Create();
                for (int i = 0; i < 50; i++)
                {
                    newMD5Hash = newMD50.ComputeHash(newMD5Hash);
                }
                rc4encryptionKey = newMD5Hash;

                OptimizedRC4 naiveRC4 = new OptimizedRC4();
                naiveRC4.Initialize(rc4encryptionKey);
                naiveRC4.Encrypt(md35.Hash, rc4outBuffer);

                for (int i = 1; i < 20; i++)
                {
                    Buffer.BlockCopy(rc4encryptionKey, 0, rc4newKey, 0, rc4encryptionKey.Length);
                    for (int j = 0; j < rc4encryptionKey.Length; j++)
                    {
                        rc4newKey[j] = (byte)(rc4encryptionKey[j] ^ i);
                    }
                    naiveRC4.Initialize(rc4newKey);
                    naiveRC4.Encrypt(rc4outBuffer, rc4outBuffer);
                }

                if (SimpleByteArraysCompare(rc4outBuffer, uEntry16))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }
            }

            // No password found
            return (string.Empty, passwordsCounter);
        }

        public static (string, long) BruteForceR4SecHandler(SecHandlerR4PasswordData passwordData, PDFInitPassSettings passwordSettings)
        {
            byte[] PADDING_STRING = { 0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A };
            const int MD5_HASH_SIZE = 16;
            const int PADDED_PASSWORD_LENGTH = 32;
            const int U_ENTRY_LENGTH_FOR_CHECK = 16;

            // Extracting the first 16 bytes of the U-entry
            byte[] uEntry16 = new byte[U_ENTRY_LENGTH_FOR_CHECK];
            Buffer.BlockCopy(passwordData.UEntry, 0, uEntry16, 0, U_ENTRY_LENGTH_FOR_CHECK);

            // Variables for the RC4 encryption
            byte[] rc4encryptionKey;
            byte[] rc4newKey = new byte[MD5_HASH_SIZE];
            byte[] rc4outBuffer = new byte[MD5_HASH_SIZE];

            // Variables for the MD5 encryption
            byte[] newMD5Hash;

            // Pre-filling the constant data into the array used to calculate the MD5 hash
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.Id.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.Id, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.Id.Length);

            if (passwordData.EncryptMetadata == false)
            {
                Array.Resize(ref fullInputForMd5Hash, fullInputForMd5Hash.Length + 4);
                Buffer.BlockCopy(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.Id.Length, 4);
            }

            // Step 2 from Algorigthm 3.5
            MD5 md35 = MD5.Create();
            md35.TransformBlock(PADDING_STRING, 0, PADDING_STRING.Length, null, 0);
            md35.TransformFinalBlock(passwordData.Id, 0, passwordData.Id.Length);

            // Passwords counter
            long passwordsCounter = 0;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
                passwordsCounter++;

                // Copy the password as a byte array into the full array for MD5 calculation
                MD5 newMD5 = MD5.Create();
                Buffer.BlockCopy(paddedPassword, 0, fullInputForMd5Hash, 0, paddedPassword.Length);
                newMD5.ComputeHash(fullInputForMd5Hash);
                newMD5Hash = newMD5.Hash;

                // New MD5 instance for calculation of the MD5 hash 50 times
                MD5 newMD50 = MD5.Create();
                for (int i = 0; i < 50; i++)
                {
                    newMD5Hash = newMD50.ComputeHash(newMD5Hash);
                }
                rc4encryptionKey = newMD5Hash;

                OptimizedRC4 naiveRC4 = new OptimizedRC4();
                naiveRC4.Initialize(rc4encryptionKey);
                naiveRC4.Encrypt(md35.Hash, rc4outBuffer);

                for (int i = 1; i < 20; i++)
                {
                    Buffer.BlockCopy(rc4encryptionKey, 0, rc4newKey, 0, rc4encryptionKey.Length);
                    for (int j = 0; j < rc4encryptionKey.Length; j++)
                    {
                        rc4newKey[j] = (byte)(rc4encryptionKey[j] ^ i);
                    }
                    naiveRC4.Initialize(rc4newKey);
                    naiveRC4.Encrypt(rc4outBuffer, rc4outBuffer);
                }

                if (SimpleByteArraysCompare(rc4outBuffer, uEntry16))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }
            }

            // No password found
            return (string.Empty, passwordsCounter);
        }

        public static (string, long) BruteForceR5SecHandler(SecHandlerR6PasswordData passwordData, PDFInitPassSettings passwordSettings)
        {
            // Save the first 32 bytes of the U entry for comparison
            byte[] uEntryToCompare = new byte[32];
            Buffer.BlockCopy(passwordData.UEntry, 0, uEntryToCompare, 0, uEntryToCompare.Length);

            // Passwords counter
            long passwordsCounter = 0;

            byte[] paddedPassword = new byte[passwordSettings.PasswordLength + SecHandlerR6PasswordData.SaltSize];
            Buffer.BlockCopy(passwordData.ValidationSalt, 0, paddedPassword, passwordSettings.PasswordLength, SecHandlerR6PasswordData.SaltSize);

            IEnumerable<byte[]> passwords = GeneratePasswordAsBytes(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            foreach (byte[] password in passwords)
            {
                passwordsCounter++;

                // Padding password with the validation salt
                Buffer.BlockCopy(password, 0, paddedPassword, 0, password.Length);

                SHA256 sha256 = SHA256.Create();
                sha256.TransformFinalBlock(paddedPassword, 0, paddedPassword.Length);

                if (SimpleByteArraysCompare(sha256.Hash, uEntryToCompare))
                {
                    string passwordAsString = Encoding.UTF8.GetString(password);

                    return (passwordAsString, passwordsCounter);
                }
            }

            // No password found
            return (string.Empty, passwordsCounter);
        }

        public static (string, long) BruteForceR6SecHandler(SecHandlerR6PasswordData passwordData, PDFInitPassSettings passwordSettings)
        {
            // Save the first 32 bytes of the U entry for comparison
            byte[] uEntryToCompare = new byte[32];
            Buffer.BlockCopy(passwordData.UEntry, 0, uEntryToCompare, 0, uEntryToCompare.Length);

            // Filling in the data that will not be modified
            byte[] paddedPassword = new byte[passwordSettings.PasswordLength + SecHandlerR6PasswordData.SaltSize];
            Buffer.BlockCopy(passwordData.ValidationSalt, 0, paddedPassword, passwordSettings.PasswordLength, SecHandlerR6PasswordData.SaltSize);

            byte[] E = null;

            // Passwords counter
            long passwordsCounter = 0;

            IEnumerable<byte[]> passwords = GeneratePasswordAsBytes(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);
            foreach (byte[] password in passwords)
            {
                passwordsCounter++;

                // Padding password with the validations salt
                Buffer.BlockCopy(password, 0, paddedPassword, 0, password.Length);

                SHA256 sha256 = SHA256.Create();
                byte[] K = sha256.ComputeHash(paddedPassword);

                // Interations counter for E checks
                int round = 0;
                do
                {
                    // Step "a"
                    byte[] shortK1 = new byte[(password.Length + K.Length)];
                    Buffer.BlockCopy(password, 0, shortK1, 0, password.Length);
                    Buffer.BlockCopy(K, 0, shortK1, password.Length, K.Length);

                    byte[] K1 = new byte[64 * shortK1.Length];
                    for (int i = 0; i < 64; i++)
                    {
                        Buffer.BlockCopy(shortK1, 0, K1, i * shortK1.Length, shortK1.Length);
                    }

                    // Step "b"
                    Aes aes = Aes.Create();
                    aes.Mode = CipherMode.CBC;

                    byte[] aesKey = new byte[16];
                    Buffer.BlockCopy(K, 0, aesKey, 0, 16);
                    aes.Key = aesKey;

                    byte[] aesIV = new byte[16];
                    Buffer.BlockCopy(K, 16, aesIV, 0, 16);
                    aes.IV = aesIV;

                    aes.Padding = PaddingMode.None;

                    ICryptoTransform aesEncryptor = aes.CreateEncryptor();
                    E = aesEncryptor.TransformFinalBlock(K1, 0, K1.Length);

                    // Step "c"
                    byte[] uBigEndInt = new byte[16];
                    Buffer.BlockCopy(E, 0, uBigEndInt, 0, 16);

                    // The BigInteger constructor is expecting the data in the little-endian format
                    Array.Reverse(uBigEndInt);

                    // Interpreting the byte array as a positive integer
                    if (uBigEndInt[15] > 0x7f)
                    {
                        byte[] expandArray = new byte[17];
                        Buffer.BlockCopy(uBigEndInt, 0, expandArray, 0, 16);
                        uBigEndInt = expandArray;
                    }
                    BigInteger newBigInt = new BigInteger(uBigEndInt);

                    // Check the remainder, modulo 3
                    int remainder3 = (int)(newBigInt % 3);
                    switch (remainder3)
                    {
                        case 0:
                            SHA256 newSha256 = SHA256.Create();
                            K = newSha256.ComputeHash(E);
                            break;
                        case 1:
                            SHA384 newSha384 = SHA384.Create();
                            K = newSha384.ComputeHash(E);
                            break;
                        case 2:
                            SHA512 newSha512 = SHA512.Create();
                            K = newSha512.ComputeHash(E);
                            break;
                    }

                    round++;
                } while (round < 64 || (uint)E[E.Length - 1] > round - 32);

                byte[] finalKToCompare = new byte[32];
                Buffer.BlockCopy(K, 0, finalKToCompare, 0, 32);

                if (SimpleByteArraysCompare(finalKToCompare, uEntryToCompare))
                {
                    string passwordAsString = Encoding.UTF8.GetString(password);

                    return (passwordAsString, passwordsCounter);
                }
            }

            // No password found
            return (string.Empty, passwordsCounter);
        }
        #endregion

        /// <summary>
        /// Function claculates a number of passwords to check based on the password length and alphabet
        /// </summary>
        /// <param name="passwordLength">Length of the password</param>
        /// <param name="alphabet">Password alphabet</param>
        /// <param name="startString">Password to start with</param>
        /// <returns>A number of passwords to go through</returns>
        public static long CalculateTotalPasswordsNumber(int passwordLength, int alphabetLength)
        {
            long totalPasswords = (long)Math.Pow(alphabetLength, passwordLength);

            return totalPasswords;
        }

        /// <summary>
        /// Simple (no input validity checks for the purpose of speed) comparision of 2 arrays of bytes
        /// </summary>
        /// <param name="firstArray">First byte array</param>
        /// <param name="secondArray">Second byte array</param>
        /// <returns>True if the data in the arrays is the same</returns>
        private static bool SimpleByteArraysCompare(byte[] firstArray, byte[] secondArray)
        {
            for (int i = 0; i < firstArray.Length; i++)
            {
                if (firstArray[i] != secondArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        #region ***** CONVERTION FUNCTIONS *****
        /// <summary>
        /// Method converts a hexadecimal string into a byte array. The size of the byte array is 2 times smaller than the length of the input string
        /// </summary>
        /// <param name="hexString">Hexadecimal string</param>
        /// <returns>Byte array of converted values</returns>
        /// <exception cref="ArgumentException">This exception is thrown, if the input string is null or empty</exception>
        /// <exception cref="FormatException">This exception is thrown, if the input string contains characters that are not possible to convert into a byte</exception>
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                throw new ArgumentException($"The input hex string can't be null or empty");
            }

            // Per PDF specification we need to add a "0" to the end of the hex string, if the length is odd
            string innerString = hexString;
            if (hexString.Length % 2 != 0) innerString += '0';

            byte[] outArray = new byte[innerString.Length / 2];
            try
            {
                int idCnt = 0;
                string tempString;

                // Converting a hex string into a byte array by converting every 2 sequential symbols into a byte
                for (int i = 0; i < innerString.Length; i += 2)
                {
                    tempString = innerString.Substring(i, 2);
                    outArray[idCnt] = byte.Parse(tempString, NumberStyles.AllowHexSpecifier);
                    idCnt++;
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException($"The hex string provided contains non-hex characters", ex);
            }
            return outArray;
        }
        #endregion

        #region ***** PRINT FUNCTIONS *****
        /// <summary>
        /// Function prints the usage of the program, if no command line arguments were provided
        /// </summary>
        public static void PrintHelp()
        {
            Console.WriteLine($"Usage:{Environment.NewLine} PDFPassRecover.exe file_name.pdf{Environment.NewLine}");
        }

        /// <summary>
        /// Prints a fatal message in red color
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintFatal(string message, bool writeLine = true)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Prints a fatal message in yellow color
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintWarning(string message, bool writeLine = true)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Prints a fatal message in green color
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintInfo(string message, bool writeLine = true)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Function prints a text to the console in the selected color
        /// </summary>
        /// <param name="message">Message to be printed</param>
        /// <param name="textColor">Color specified as ConsoleColor enum</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintColoredText(string message, ConsoleColor textColor, bool writeLine = true)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = textColor;

            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = originalColor;
        }
        #endregion
    }
}
