using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PDFPassRecovery
{
    public static class PDFParserLib
    {
        #region ***** PDF Validity Check *****
        public static void CheckPDFValidity(PDFFileContent fileContent)
        {
            if (fileContent is null)
            {
                throw new ArgumentNullException("The file content cannot be null or empty");
            }

            Console.Write("Checking the presense of the standard header...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            // 1st check of the valid PDF- whether the "%PDF" string presents in the file
            if (!IsPDFFileHeaderPresent(fileContent))
            {
                throw new InvalidDataException("The standard PDF header is missing");
            }
            PDFPassRecoverLib.PrintColoredText("OK", ConsoleColor.Green);

            Console.Write("Checking the presense of the standard trailer...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            // 2nd check of the valid PDF- whether the "%%EOF" string is in the end of the file
            if (!IsPDFFileTrailerPresent(fileContent))
            {
                throw new InvalidDataException("The standard PDF trailer is missing");
            }
            PDFPassRecoverLib.PrintColoredText("OK", ConsoleColor.Green);

            Console.Write("Checking if the file is encrypted...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            if (!IsPDFFileEncrypted(fileContent))
            {
                throw new InvalidDataException("The PDF file is not encrypted");
            }
            PDFPassRecoverLib.PrintColoredText("Yes", ConsoleColor.Green);
        }

        /// <summary>
        /// The function checks whether the PDF header is in the file supplied
        /// </summary>
        /// <param name="fileContent">Content of a file as a string</param>
        /// <returns>True, if header has been found</returns>
        /// <exception cref="ArgumentException">The "ArgumentException" is raised, if the content of the string passed is null or empty</exception>
        static bool IsPDFFileHeaderPresent(PDFFileContent fileContent)
        {
            const string PDF_HEADER = "%PDF";

            if (fileContent.AsString.Contains(PDF_HEADER))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The function checks whether the PDF trailer is in the file supplied
        /// </summary>
        /// <param name="fileContent">Content of a file as a string</param>
        /// <returns>True, if the trailer has been found</returns>
        /// <exception cref="ArgumentException">The "ArgumentException" is raised, if the content of the string passed is null or empty</exception>
        static bool IsPDFFileTrailerPresent(PDFFileContent fileContent)
        {
            const string PDF_TRAILER = "%%EOF";

            if (fileContent.AsString.Contains(PDF_TRAILER))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Function checks if the PDF file is encrypted
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns>False is returned, if file is not encrypted and true otherwise</returns>
        public static bool IsPDFFileEncrypted(PDFFileContent fileContent)
        {
            // Pattern to find the encryption section in the file
            string encrytionObjRefPattern = @"/Encrypt.*R";
            Regex encryptionObjRefRegex = new Regex(encrytionObjRefPattern, RegexOptions.Singleline);
            Match regexMatch = encryptionObjRefRegex.Match(fileContent.AsString);

            if (regexMatch.Value == string.Empty)
            {
                return false;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// The function returns version of the PDF file
        /// </summary>
        /// <param name="fileContent">fileContent should be supplied as a string</param>
        /// <returns>PDF version returned as a string. If the version cannot be extracted, then an empty string is returned</returns>
        static public string GetPDFVersion(string fileContent)
        {
            // Regexp for the PDF file header
            string pdfFullHeaderPattern = @"\A%.*?%";
            Regex pdfFullHeaderRegex = new Regex(pdfFullHeaderPattern, RegexOptions.Singleline);
            Match regexpMatch = pdfFullHeaderRegex.Match(fileContent);
            string pdfFullHeader = regexpMatch.Value;

            // Regexp for the PDF file version
            string pdfVersionPattern = @"\d+\.\d+";
            Regex pdfVersionRegex = new Regex(pdfVersionPattern, RegexOptions.Singleline);
            regexpMatch = pdfVersionRegex.Match(pdfFullHeader);

            return regexpMatch.Value;
        }

        /// <summary>
        /// The function reads the file provided and stores the content as a byte array and as a string
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>PDFFileContentObject with the file content</returns>
        public static PDFFileContent GetPDFFileConent(string fileName)
        {
            byte[] byteFileContent;
            string stringFileContent;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("The file name cannot be null or empty!");
            }

            try
            {
                byteFileContent = File.ReadAllBytes(fileName);
                stringFileContent = Encoding.ASCII.GetString(byteFileContent);
            }
            catch (Exception)
            {
                throw;
            }

            PDFFileContent pdfFileContent = new PDFFileContent(stringFileContent, byteFileContent);
            return pdfFileContent;
        }

        public static BasePasswordData ExtractPDF12PasswordData(PDFFileContent fileContent)
        {
            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");

            BasePasswordData pdf12PasswordData = ExtractPDF12EncryptionObjectData(encryptionObject);

            pdf12PasswordData.IdValue = ExtractIDValue(fileContent);

            return pdf12PasswordData;
        }

        private static BasePasswordData ExtractPDF12EncryptionObjectData(byte[] encryptionObject)
        {
            const int O_ENTRY_SIZE = 32;
            const int U_ENTRY_SIZE = 32;

            BasePasswordData pdf12PasswordData = new BasePasswordData();

            pdf12PasswordData.OEntry = GetArrayEntry("/O", encryptionObject, O_ENTRY_SIZE);
            pdf12PasswordData.UEntry = GetArrayEntry("/U", encryptionObject, U_ENTRY_SIZE);

            int pValue = GetNumericalEntry("/P", encryptionObject);
            pdf12PasswordData.P = BitConverter.GetBytes(pValue);

            pdf12PasswordData.R = GetNumericalEntry("/R", encryptionObject);
            pdf12PasswordData.V = GetNumericalEntry("/V", encryptionObject);

            return pdf12PasswordData;
        }

        /// <summary>
        /// Function extracts data related to the password from the file PDF1.4 file
        /// </summary>
        /// <returns>PDFPasswordData object</returns>
        public static PDF14PasswordData ExtractPDF14PasswordData(PDFFileContent fileContent)
        {
            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");

            PDF14PasswordData pdf14PasswordData = ExtractPDF14EncryptionObjectData(encryptionObject);
            pdf14PasswordData.IdValue = ExtractIDValue(fileContent);

            return pdf14PasswordData;
        }

        private static PDF14PasswordData ExtractPDF14EncryptionObjectData(byte[] encryptionObject)
        {
            BasePasswordData pdf12PasswordData = ExtractPDF12EncryptionObjectData(encryptionObject);
            PDF14PasswordData pdf14PasswordData = new PDF14PasswordData(pdf12PasswordData)
            {
                KeyLength = GetNumericalEntry("/Length", encryptionObject)
            };

            return pdf14PasswordData;
        }

        public static PDF15PasswordData ExtractPDF15PasswordData(PDFFileContent fileContent)
        {
            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");

            PDF15PasswordData pdf15PasswordData = ExtractPDF15EncryptionObjectData(encryptionObject);
            pdf15PasswordData.IdValue = ExtractIDValue(fileContent);

            return pdf15PasswordData;
        }

        private static PDF15PasswordData ExtractPDF15EncryptionObjectData(byte[] encryptionObject)
        {
            PDF14PasswordData pdf14PasswordData = ExtractPDF14EncryptionObjectData(encryptionObject);
            PDF15PasswordData pdf15PasswordData = new PDF15PasswordData(pdf14PasswordData);

            try
            {
                pdf15PasswordData.EncryptMetadata = GetBooleanEntry("/EncryptMetadata", encryptionObject);
            }
            catch { }
            {
                pdf15PasswordData.EncryptMetadata = true;
            }

            return pdf15PasswordData;
        }

        /// <summary>
        /// The function returns a byte array entry from an input array. The entry in the input array can be stored as a hexadecimal string or as a string
        /// </summary>
        /// <param name="entryName">Name of the entry to search for</param>
        /// <param name="inputArray">Input array to search in</param>
        /// <param name="entrySize">Size of the entry to return</param>
        /// <returns></returns>
        private static byte[] GetArrayEntry(string entryName, byte[] inputArray, int entrySize)
        {
            // Processing possible errors
            if (string.IsNullOrEmpty(entryName))
            {
                throw new InvalidDataException($"The etnry name to extract cann't be null or empty");
            }

            if (inputArray is null)
            {
                throw new InvalidDataException("The input array cannot be null");
            }

            if (inputArray.Length == 0)
            {
                throw new InvalidDataException("The input array cannot be empty");
            }

            if (entrySize <= 2)
            {
                throw new InvalidDataException("The entry size to extract cann't be less than 2");
            }

            byte[] outArray = null;

            int startIndex = GetStringIndexInArray(entryName, inputArray);
            int curPointer = startIndex + entryName.Length;

            // Looking for the beginning of the array that could be the '<' or '(' character
            while (inputArray[curPointer] != '(' && inputArray[curPointer] != '<')
            {
                curPointer++;
            }

            char oArrayStart = Convert.ToChar(inputArray[curPointer]);
            curPointer++;
            switch (oArrayStart)
            {
                case '<':
                    string hexadecimalStr = string.Empty;
                    while (inputArray[curPointer] != '>')
                    {
                        hexadecimalStr += Convert.ToChar(inputArray[curPointer]);
                        curPointer++;
                    }
                    outArray = PDFPassRecoverLib.ConvertHexStringToByteArray(hexadecimalStr);
                    break;

                case '(':
                    outArray = ExtractLiteralByteArray(inputArray, curPointer, entrySize);
                    break;

                default:
                    break;
            }

            return outArray;
        }

        private static int GetNumericalEntry(string entryName, byte[] inputArray)
        {
            string valueString = string.Empty;

            int startIndex = GetStringIndexInArray(entryName, inputArray);
            int curPointer = startIndex + entryName.Length;

            while (inputArray[curPointer] != '/' &&
                inputArray[curPointer] != '>')
            {
                valueString += Convert.ToChar(inputArray[curPointer]);
                curPointer++;
            }
            valueString = valueString.Trim();

            if (!int.TryParse(valueString, out int value))
            {
                throw new FormatException($"Error converting the extracted \"/{entryName}\" value");
            }
            else return value;
        }

        private static bool GetBooleanEntry(string entryName, byte[] inputArray)
        {
            string valueString = string.Empty;

            int startIndex = GetStringIndexInArray(entryName, inputArray);
            int curPointer = startIndex + entryName.Length;

            while (inputArray[curPointer] != '/' &&
                inputArray[curPointer] != '>')
            {
                valueString += Convert.ToChar(inputArray[curPointer]);
                curPointer++;
            }

            valueString = valueString.Trim();

            switch (valueString)
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    throw new FormatException($"The unknown value in the bool entry");
            }
        }

        /// <summary>
        /// Function extracts the size encryption section and encryption object offset in the PDF document
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        private static byte[] ExtractEncryptionObject(PDFFileContent fileContent)
        {
            // Pattern to find the encryption section in the file
            string encrytionObjRefPattern = @"/Encrypt.*?R";
            Regex encryptionObjRefRegex = new Regex(encrytionObjRefPattern, RegexOptions.Singleline);
            Match regexMatch = encryptionObjRefRegex.Match(fileContent.AsString);

            string[] matchSplit = regexMatch.Value.Split(' ');
            string encryptionObjRef = matchSplit[1] + ' ' + matchSplit[2];

            // Searching for the encryption object
            string encryptionObjPattern = encryptionObjRef + ' ' + @"obj.*?endobj";
            Regex encryptionObjRegex = new Regex(encryptionObjPattern, RegexOptions.Singleline);
            regexMatch = encryptionObjRegex.Match(fileContent.AsString);

            if (regexMatch.Success)
            {
                byte[] encryptionObject = new byte[regexMatch.Value.Length];
                Buffer.BlockCopy(fileContent.AsByteArray, regexMatch.Index, encryptionObject, 0, encryptionObject.Length);
                return encryptionObject;
            }

            return null;
        }

        /// <summary>
        /// Function extracts the ID value from the PDF document
        /// </summary>
        /// <param name="fileContent">PDFFileContent object with the content of the PDF file as a string and as a binary array</param>
        /// <returns>ID value as a byte array</returns>
        public static byte[] ExtractIDValue(PDFFileContent fileContent)
        {
            if (fileContent is null) throw new InvalidDataException($"The file content cannot be null");

            // The /ID shall present in any encrypted PDF file
            // Regexp for the /ID value in the file
            string idValuePattern = @"/ID.*\[\<.*?\>\]";
            Regex idValueRegex = new Regex(idValuePattern, RegexOptions.Singleline);
            Match regexMatch = idValueRegex.Match(fileContent.AsString);
            if (!regexMatch.Success)
            {
                throw new InvalidDataException($"The ID file identifier is missing in the PDF document");
            }

            // Regexp to extract the first entry in the ID array
            string idFirstValuePattern = @"\<.*?\>";
            Regex idFirstValueRegexp = new Regex(idFirstValuePattern, RegexOptions.Singleline);
            regexMatch = idFirstValueRegexp.Match(regexMatch.Value);
            if (!regexMatch.Success)
            {
                throw new InvalidDataException($"The hexadecimal string with the ID entry is malformed");
            }
            string idFirstValue = regexMatch.Value.Trim('<', '>');

            byte[] idArray = PDFPassRecoverLib.ConvertHexStringToByteArray(idFirstValue);

            if (idArray.Length == 0)
            {
                throw new InvalidDataException($"The ID entry has not been converted int a byte array");
            }

            return idArray;
        }

        /// <summary>
        /// Funtion extracts a literal byte array (ending with the ")") from the input array starting from a certain poisition in the output array of the predefined size
        /// </summary>
        /// <param name="inputArray">Array where to search for</param>
        /// <param name="startPosition">Starting position in the input array</param>
        /// <param name="outputArraySize">Size of the output array</param>
        /// <returns></returns>
        private static byte[] ExtractLiteralByteArray(byte[] inputArray, int startPosition, int outputArraySize)
        {
            byte[] resultingArray = new byte[outputArraySize];

            int i = 0;  // Counter to navigate in the input array
            int j = 0;  // Counter to navigate in the resulting array
            while (inputArray[startPosition + i] != ')')
            {
                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == 'n'))
                {
                    resultingArray[j] = 0x0A;
                    i += 2;
                    j++;
                    continue;
                }

                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == 'r'))
                {
                    resultingArray[j] = 0x0D;
                    i += 2;
                    j++;
                    continue;
                }

                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == 't'))
                {
                    resultingArray[j] = 0x09;
                    i += 2;
                    j++;
                    continue;
                }

                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == 'b'))
                {
                    resultingArray[j] = 0x08;
                    i += 2;
                    j++;
                    continue;
                }

                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == 'f'))
                {
                    resultingArray[j] = 0x0C;
                    i += 2;
                    j++;
                    continue;
                }
                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == '\\'))
                {
                    resultingArray[j] = 0x5C;
                    i += 2;
                    j++;
                    continue;
                }
                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == '('))
                {
                    resultingArray[j] = 0x28;
                    i += 2;
                    j++;
                    continue;
                }
                if ((inputArray[startPosition + i] == '\\') && (inputArray[startPosition + i + 1] == ')'))
                {
                    resultingArray[j] = 0x29;
                    i += 2;
                    j++;
                    continue;
                }

                resultingArray[j] = inputArray[startPosition + i];
                i++;
                j++;
            }

            return resultingArray;
        }

        /// <summary>
        /// The function resturns an index of a string in the array, first the string is converted into a byte array
        /// </summary>
        /// <param name="inString">Input string to search for</param>
        /// <param name="inArray">Input array</param>
        /// <returns></returns>
        private static int GetStringIndexInArray(string inString, byte[] inArray)
        {
            if (inArray is null)
            {
                throw new InvalidDataException($"The input byte array cannot be null");
            }

            if (string.IsNullOrEmpty(inString))
            {
                throw new InvalidDataException($"The input string array cannot be null or empty");
            }

            // Converting the string into a subarray first
            byte[] subArray = Encoding.ASCII.GetBytes(inString);

            int index = -1;
            for (int i = 0; i < inArray.Length - inString.Length; i++)
            {
                if (inArray[i] == subArray[0])
                {
                    int j = 0;
                    while ((j != subArray.Length) && (inArray[i + j] == subArray[j]))
                    {
                        j++;
                    }
                    if (j == subArray.Length) return i;
                }
            }

            return index;
        }
    }
}
