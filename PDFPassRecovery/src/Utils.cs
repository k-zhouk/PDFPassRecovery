using System;
using System.Globalization;
using System.IO;

namespace PDFPassRecovery
{
    public static class Utils
    {
        public static string CheckProgramArgs(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    return $"No arguments have been provided!";

                case 1:
                    // Display help
                    if (args[0] == "-h")
                    {
                        PrintHelp();
                        return string.Empty;
                    }

                    // Otherwise treat the 1st argument as a path to a PDF file
                    return args[0];

                default:
                    return $"Too many arguments";
            }
        }

        public static FileInfo CheckFileExists(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                return fi;
            }

            return null;
        }

        /// <summary>
        /// Function claculates a number of passwords to check based on the password length and alphabet provided
        /// </summary>
        /// <param name="passwordLength">Length of the password</param>
        /// <param name="alphabet">Password alphabet</param>
        /// <param name="startString">Password to start with</param>
        /// <returns>A number of passwords to go through</returns>
        public static long CalcTotalPasswordsNumber(int passwordLength, int alphabetLength)
        {
            long totalPasswords = (long)Math.Pow(alphabetLength, passwordLength);

            return totalPasswords;
        }

        /// <summary>
        /// Simple (no input validity checks for the purpose of speed) comparison of 2 arrays of bytes
        /// </summary>
        /// <param name="firstArray">First byte array</param>
        /// <param name="secondArray">Second byte array</param>
        /// <returns>True if the data in the arrays is the same</returns>
        public static bool ByteArraysCompare(byte[] firstArray, byte[] secondArray)
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
        public static byte[] ConvertHexStrToByteArr(string hexString)
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
            Console.WriteLine($"Usage:{Environment.NewLine} PDFPassRecover.exe <file_name.pdf>{Environment.NewLine}");
        }

        /// <summary>
        /// Prints a fatal message in red color
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintError(string message, bool writeLine = true)
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
        /// Prints a warning message in yellow color
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
        /// Prints an OK message in green color
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="writeLine">True will print a new line character</param>
        public static void PrintOK(string message, bool writeLine = true)
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
