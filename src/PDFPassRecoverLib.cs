using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace PDFPassRecovery
{
    public static class PDFPassRecoverLib
    {
        public const int PADDING_OFFSET = 55;
        public const char PADDING_CHAR = ' ';
        public delegate (string, long) BruteForceFunctionDelegate(PDF15PasswordData passwordData, PDFPasswordSettings passwordsOptions);

        /// <summary>
        /// Function generates a padded password based on the parameters provided- password length, alphabet and start string
        /// </summary>
        /// <param name="passwordLength">Length of the password to generate</param>
        /// <param name="alphabet">Alphabet (symbols) to use for password generation</param>
        /// <param name="startPassword">A string to start with</param>
        /// <returns>IEnumerable interface for a byte array</returns>
        /// <exception cref="ArgumentNullException">Exception is thrown, if the string to strat with is null</exception>
        /// <exception cref="ArgumentException">Exception is thrown, if the string to start with is longer, then the password length</exception>
        public static IEnumerable<byte[]> GeneratePaddedPassword(int passwordLength, string alphabet, string startPassword)
        {
            // Padding string from PDF1.2 ~ 1.4 specification
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

        public static void RestoreEncryptedPassword<T>(Func<T, PDFPasswordSettings, (string, long)> bruteForceFunction, T passwordData, PDFPasswordSettings passwordSettings) where T : BasePasswordData
        {
            Console.WriteLine($"{Environment.NewLine}Starting to restore the password...");

            int passwordLength = passwordSettings.PasswordLength;
            string alphabet = passwordSettings.Alphabet;
            string startPassword = passwordSettings.StartPassword;

            Console.WriteLine($"Password length:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{passwordLength}");
            Console.WriteLine($"Starting password:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"\"{startPassword}\"");
            Console.WriteLine($"Starting password length:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{startPassword.Length}");

            long totalPasswords = CalculateTotalPasswordsNumber(passwordLength - startPassword.Length, alphabet.Length);
            Console.WriteLine($"Total number of passwords to check:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{totalPasswords:N0}");

            Console.WriteLine();
            DateTime startTime = DateTime.Now;
            Console.WriteLine($"Brute force started at:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{startTime}");

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
                    PrintColoredText($"The password is:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{password}{Environment.NewLine}", ConsoleColor.Green, false);
                }
            }
            else
            {
                throw new InvalidDataException($"The brute force function is not provided");
            }

            sw.Stop();
            TimeSpan elapsed = sw.Elapsed;

            DateTime endTime = DateTime.Now;
            Console.WriteLine($"Brute force ended at:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{endTime}");

            Console.WriteLine($"{Environment.NewLine}*************** STATISTICS ***************");
            Console.WriteLine($"Total elapsed time:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{elapsed.TotalSeconds:N1} seconds");

            Console.WriteLine($"Passwords checked:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{passwordsChecked:N0}");
            double passwordsCheckPerSec = passwordsChecked / elapsed.TotalSeconds;
            Console.WriteLine($"Performance:".PadRight(PADDING_OFFSET, PADDING_CHAR) + $"{passwordsCheckPerSec:N0} password checks per second");

            Console.WriteLine($"{Environment.NewLine}Have a nice day!");
        }

        public static void RestorePDF12EncryptedPassword(BasePasswordData v1r2PasswordData, PDFPasswordSettings passwordSettings)
        {
            switch (v1r2PasswordData.R)
            {
                case 2:
                    RestoreEncryptedPassword(BruteForceV1R2Password, v1r2PasswordData, passwordSettings);
                    break;
            }
        }

        public static void RestorePDF14Password(PDF14PasswordData passwordData, PDFPasswordSettings passwordSettings)
        {
            switch (passwordData.R)
            {
                case 2:
                    RestoreEncryptedPassword(BruteForceV1R2Password, passwordData, passwordSettings);
                    break;

                case 3:
                    RestoreEncryptedPassword(BruteForceV2R3Password, passwordData, passwordSettings);
                    break;
            }
        }

        public static void RestorePDF15Password(PDF15PasswordData passwordData, PDFPasswordSettings passwordSettings)
        {
            switch (passwordData.R)
            {
                case 2:
                    RestoreEncryptedPassword(BruteForceV1R2Password, passwordData, passwordSettings);
                    break;

                case 3:
                    RestoreEncryptedPassword(BruteForceV2R3Password, passwordData, passwordSettings);
                    break;

                case 4:
                    RestoreEncryptedPassword(BruteForceV4R4Password, passwordData, passwordSettings);
                    break;
            }
        }

        #region ***** PSWDS BRUTE FORCE SECTION *****
        public static (string, long) BruteForceV1R2Password(BasePasswordData passwordData, PDFPasswordSettings passwordSettings)
        {
            byte[] PADDING_STRING = { 0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A };
            const int RC4_KEY_SIZE = 5;
            const int PADDED_PASSWORD_LENGTH = 32;

            // Variables for the RC4 encryption
            byte[] rc4encryptionKey = new byte[RC4_KEY_SIZE];
            byte[] rc4outBuffer = new byte[PADDING_STRING.Length];

            // Pre-filling the constant data into the array used to calculate the MD5 hash
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.IdValue.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.IdValue, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.IdValue.Length);

            long passwordsCounter = 1;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
                MD5 md5 = MD5.Create();

                // Copy the password as a byte array into the full array for MD5 calculation
                Buffer.BlockCopy(paddedPassword, 0, fullInputForMd5Hash, 0, paddedPassword.Length);
                md5.ComputeHash(fullInputForMd5Hash);

                // Extract 5 bytes for the RC4 encryption key
                Buffer.BlockCopy(md5.Hash, 0, rc4encryptionKey, 0, RC4_KEY_SIZE);

                OptimizedRC4 rc4 = new OptimizedRC4();
                rc4.Initialize(rc4encryptionKey);
                rc4.Encrypt(PADDING_STRING, rc4outBuffer);

                if (CompareByteArrays(rc4outBuffer, passwordData.UEntry))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }

                passwordsCounter++;
            }
            return (string.Empty, passwordsCounter);
        }

        private static (string, long) BruteForceV2R3Password(PDF14PasswordData passwordData, PDFPasswordSettings passwordSettings)
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
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.IdValue.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.IdValue, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.IdValue.Length);

            // Step 2 from Algorigthm 3.5
            MD5 md35 = MD5.Create();
            md35.TransformBlock(PADDING_STRING, 0, PADDING_STRING.Length, null, 0);
            md35.TransformFinalBlock(passwordData.IdValue, 0, passwordData.IdValue.Length);

            // Passwords counter
            long passwordsCounter = 1;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
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

                if (CompareByteArrays(rc4outBuffer, uEntry16))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }
                passwordsCounter++;
            }

            return (string.Empty, passwordsCounter);
        }

        private static (string, long) BruteForceV4R4Password(PDF15PasswordData passwordData, PDFPasswordSettings passwordSettings)
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
            byte[] fullInputForMd5Hash = new byte[PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.IdValue.Length];
            Buffer.BlockCopy(passwordData.OEntry, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH, passwordData.OEntry.Length);
            Buffer.BlockCopy(passwordData.P, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length, passwordData.P.Length);
            Buffer.BlockCopy(passwordData.IdValue, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length, passwordData.IdValue.Length);

            if (passwordData.EncryptMetadata == false)
            {
                Array.Resize(ref fullInputForMd5Hash, fullInputForMd5Hash.Length + 4);
                Buffer.BlockCopy(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, fullInputForMd5Hash, PADDED_PASSWORD_LENGTH + passwordData.OEntry.Length + passwordData.P.Length + passwordData.IdValue.Length, 4);
            }

            // Step 2 from Algorigthm 3.5
            MD5 md35 = MD5.Create();
            md35.TransformBlock(PADDING_STRING, 0, PADDING_STRING.Length, null, 0);
            md35.TransformFinalBlock(passwordData.IdValue, 0, passwordData.IdValue.Length);

            // Passwords counter
            long passwordsCounter = 1;

            // Generating passwords based on the inputs
            IEnumerable<byte[]> paddedPasswords = GeneratePaddedPassword(passwordSettings.PasswordLength, passwordSettings.Alphabet, passwordSettings.StartPassword);

            // Main cycle to check the password
            foreach (byte[] paddedPassword in paddedPasswords)
            {
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

                if (CompareByteArrays(rc4outBuffer, uEntry16))
                {
                    byte[] passwordAsBytes = new byte[passwordSettings.PasswordLength];
                    Buffer.BlockCopy(paddedPassword, 0, passwordAsBytes, 0, passwordSettings.PasswordLength);
                    string passwordAsString = Encoding.ASCII.GetString(passwordAsBytes);

                    return (passwordAsString, passwordsCounter);
                }
                passwordsCounter++;
            }
            return (string.Empty, passwordsCounter);
        }
        #endregion


        /// <summary>
        /// Function claculates a number of passwords to check based on the password length, alphabet and starting password
        /// </summary>
        /// <param name="passwordLength">Length of the password</param>
        /// <param name="alphabet">Password alphabet</param>
        /// <param name="startString">Password to start with</param>
        /// <returns></returns>
        public static long CalculateTotalPasswordsNumber(int passwordLength, int alphabetLength)
        {
            long totalPasswords = (long)Math.Pow(alphabetLength, passwordLength);

            return totalPasswords;
        }

        // Simple (no checks) comparision of 2 arrays of bytes
        private static bool CompareByteArrays(byte[] firstArray, byte[] secondArray)
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
        /// Function converts a hexadecimal string into a byte array. The size of the byte array is 2 times smaller than the length of the input string
        /// </summary>
        /// <param name="hexString">Hexadecimal string</param>
        /// <returns>Byte array of converted values</returns>
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                throw new ArgumentException($"The input hex string can't be null or empty");
            }

            // Per PDF specification we need to add a "0" to the end of the hex string, if the length is not even
            string innerString = hexString;
            if (hexString.Length % 2 != 0) innerString += '0';

            byte[] outputArray = new byte[innerString.Length / 2];
            try
            {
                int idCnt = 0;
                string tempString;

                // Converting a hex string into a byte array by converting every 2 sequential symbols into a byte
                for (int i = 0; i < innerString.Length; i += 2)
                {
                    tempString = innerString.Substring(i, 2);
                    outputArray[idCnt] = byte.Parse(tempString, NumberStyles.AllowHexSpecifier);
                    idCnt++;
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException($"The hex string provided contains non-hex characters", ex);
            }
            return outputArray;
        }
        #endregion

        #region ***** OTHER FUNCTIONS *****
        /// <summary>
        /// Function prints the usage of the program, if no command line arguments were provided
        /// </summary>
        public static void PrintHelp()
        {
            PrintColoredText($"No arguments have been provided!", ConsoleColor.Red);
            Console.WriteLine($"Usage: PDFPassRecover.exe file_name.pdf{Environment.NewLine}");
        }

        /// <summary>
        /// Function prints a text to the console in the selected color
        /// </summary>
        /// <param name="message">Message to be printed</param>
        /// <param name="textColor">Color specified as ConsoleColor enum</param>
        public static void PrintColoredText(string message, ConsoleColor textColor, bool writeLine = true)
        {
            if (writeLine)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = textColor;
                Console.WriteLine(message);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = textColor;
                Console.Write(message);
                Console.ForegroundColor = originalColor;
            }
        }
        #endregion

        public static PDFRestartConfig ParseConfig()
        {
            const string configFileName = "PDFPassConfig.xml";

            string programExecPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string pathRoot = Path.GetDirectoryName(programExecPath);
            string configFullPath = Path.Combine(pathRoot, configFileName);

            XElement xmlConfig;
            try
            {
                xmlConfig = XElement.Load(configFullPath);
            }
            catch
            {
                throw;
            }

            int reportingTimeSpan = (int)xmlConfig.Element("ReportingTimeSpan");

            XElement xmlPassword = xmlConfig.Element("Password");

            string startPassword = (string)xmlPassword.Element("StartPassword");
            startPassword = startPassword.Trim();

            int passwordLength = (int)xmlPassword.Element("Length");

            // Getting all elements with names ending with the "Set" and "use" set to "yes"
            IEnumerable<XElement> charSets = from parameter in xmlPassword.Descendants()
                                             where parameter.Name.LocalName.EndsWith("Set")
                                             where parameter.Attribute("use").Value == "yes"
                                             select parameter;

            // Forming the alphabet to use for brute force
            string alphabet = string.Empty;
            foreach (var charSet in charSets)
            {
                alphabet += charSet.Value.Trim();
            }

            PDFRestartConfig pdfRecoverConfig = new PDFRestartConfig(startPassword, passwordLength, alphabet, reportingTimeSpan);
            return pdfRecoverConfig;
        }
    }
}
