using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PDFPassRecovery
{
    public static class PDFParserLib
    {
        #region ***** PDF Validity Check *****
        /// <summary>
        /// Method checks whether the PDF file is valid (header, trailer and encryption object presense)
        /// </summary>
        /// <param name="fileContent">Object with the file content</param>
        /// <exception cref="ArgumentNullException">This exception is thrown, if the PDFFileContent object is null or empty</exception>
        /// <exception cref="InvalidDataException">The exception is thrown, if one of the checks fail</exception>
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
            if (!IsEncryptionObjectPresent(fileContent))
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
        /// <returns>False is returned, if the encryption section is basent in the files</returns>
        public static bool IsEncryptionObjectPresent(PDFFileContent fileContent)
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
        /// Method returns version of the PDF file
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
        /// Method reads and stores the content of the PDF as a byte array and as a string
        /// </summary>
        /// <param name="fileName">Name of a PDF file to read in</param>
        /// <returns>PDFFileContentObject with the file content</returns>
        public static PDFFileContent GetPDFFileContent(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("The file name cannot be null or empty!");
            }

            byte[] byteFileContent;
            string stringFileContent;
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
            const int O_ENTRY_SIZE = 32;
            const int U_ENTRY_SIZE = 32;

            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");

            int pValue = GetNumericalEntry("/P", encryptionObject);

            BasePasswordData pdf12PasswordData = new BasePasswordData()
            {
                OEntry = GetArrayEntry("/O", encryptionObject, O_ENTRY_SIZE),
                UEntry = GetArrayEntry("/U", encryptionObject, U_ENTRY_SIZE),
                P = BitConverter.GetBytes(pValue),
                R = GetNumericalEntry("/R", encryptionObject),
                V = GetNumericalEntry("/V", encryptionObject),

                Id = ExtractFileID(fileContent)
            };

            return pdf12PasswordData;
        }

        /// <summary>
        /// Function extracts data related to the password from the file PDF1.4 file
        /// </summary>
        /// <returns>PDFPasswordData object</returns>
        public static PDF14PasswordData ExtractPDF14PasswordData(PDFFileContent fileContent)
        {
            const int O_ENTRY_SIZE = 32;
            const int U_ENTRY_SIZE = 32;

            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");

            int pValue = GetNumericalEntry("/P", encryptionObject);

            PDF14PasswordData pdf14PasswordData = new PDF14PasswordData()
            {
                OEntry = GetArrayEntry("/O", encryptionObject, O_ENTRY_SIZE),
                UEntry = GetArrayEntry("/U", encryptionObject, U_ENTRY_SIZE),
                P = BitConverter.GetBytes(pValue),
                R = GetNumericalEntry("/R", encryptionObject),
                V = GetNumericalEntry("/V", encryptionObject),
                KeyLength = GetNumericalEntry("/Length", encryptionObject),

                Id = ExtractFileID(fileContent)
            };

            return pdf14PasswordData;
        }

        /*
        Hex View  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F
        000002D0                              32 31 20 30 20 6F 62           21 0 ob
        000002E0  6A 3C 3C 2F 52 20 34 2F  4C 65 6E 67 74 68 20 31  j<</R 4/Length 1
        000002F0  32 38 2F 46 69 6C 74 65  72 2F 53 74 61 6E 64 61  28/Filter/Standa
        00000300  72 64 2F 4F 28 B1 F4 A0  77 BE 87 C3 C0 F5 97 5D  rd/O(...w......]
        00000310  5C 72 B1 7F CD 9E 7C 88  4B 40 33 48 79 A5 27 51  \r....|.K@3Hy.'Q
        00000320  33 B2 C5 FF 67 07 29 2F  50 20 2D 33 31 33 32 2F  3...g.)/P -3132/
        00000330  55 28 41 94 E6 58 50 EF  3D 20 E1 C9 BA 2D 30 CF  U(A..XP.= ...-0.
        00000340  C4 B0 00 00 00 00 00 00  00 00 00 00 00 00 00 00  ................
        00000350  00 00 29 2F 56 20 34 2F  43 46 3C 3C 2F 53 74 64  ..)/V 4/CF<</Std
        00000360  43 46 3C 3C 2F 4C 65 6E  67 74 68 20 31 36 2F 43  CF<</Length 16/C
        00000370  46 4D 2F 56 32 2F 41 75  74 68 45 76 65 6E 74 2F  FM/V2/AuthEvent/
        00000380  44 6F 63 4F 70 65 6E 3E  3E 3E 3E 2F 53 74 6D 46  DocOpen>>>>/StmF
        00000390  2F 53 74 64 43 46 2F 53  74 72 46 2F 53 74 64 43  /StdCF/StrF/StdC
        000003A0  46 2F 45 6E 63 72 79 70  74 4D 65 74 61 64 61 74  F/EncryptMetadat
        000003B0  61 20 66 61 6C 73 65 3E  3E 0D 65 6E 64 6F 62 6A  a false>>.endobj
         */

        /*
        Hex View  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F
        0007FA20                    38 35  20 30 20 6F 62 6A 0D 0A        85 0 obj..
        0007FA30  3C 3C 2F 43 46 3C 3C 2F  53 74 64 43 46 3C 3C 2F  <</CF<</StdCF<</
        0007FA40  41 75 74 68 45 76 65 6E  74 2F 44 6F 63 4F 70 65  AuthEvent/DocOpe
        0007FA50  6E 2F 43 46 4D 2F 41 45  53 56 32 2F 4C 65 6E 67  n/CFM/AESV2/Leng
        0007FA60  74 68 20 31 36 3E 3E 3E  3E 2F 46 69 6C 74 65 72  th 16>>>>/Filter
        0007FA70  2F 53 74 61 6E 64 61 72  64 2F 4C 65 6E 67 74 68  /Standard/Length
        0007FA80  20 31 32 38 2F 4F 3C 44  45 33 31 44 43 42 43 45   128/O<DE31DCBCE
        0007FA90  42 46 45 44 30 46 36 43  42 43 36 44 31 42 33 45  BFED0F6CBC6D1B3E
        0007FAA0  44 34 31 45 42 36 41 39  34 31 39 35 46 35 41 34  D41EB6A94195F5A4
        0007FAB0  41 41 32 42 42 31 44 36  39 43 44 41 41 37 39 42  AA2BB1D69CDAA79B
        0007FAC0  38 39 36 32 41 43 31 3E  2F 50 20 2D 31 30 36 30  8962AC1>/P -1060
        0007FAD0  2F 52 20 34 2F 53 74 6D  46 2F 53 74 64 43 46 2F  /R 4/StmF/StdCF/
        0007FAE0  53 74 72 46 2F 53 74 64  43 46 2F 55 3C 45 31 31  StrF/StdCF/U<E11
        0007FAF0  34 34 42 31 46 41 46 43  41 34 31 41 41 36 45 43  44B1FAFCA41AA6EC
        0007FB00  36 44 30 39 31 33 34 31  37 39 42 30 46 30 30 30  6D09134179B0F000
        0007FB10  30 30 30 30 30 30 30 30  30 30 30 30 30 30 30 30  0000000000000000
        0007FB20  30 30 30 30 30 30 30 30  30 30 30 30 30 3E 2F 56  0000000000000>/V
        0007FB30  20 34 3E 3E 0D 0A 65 6E  64 6F 62 6A               4>>..endobj
         */

        public static PDF15PasswordData ExtractPDF15PasswordData(PDFFileContent fileContent)
        {
            const int O_ENTRY_SIZE = 32;
            const int U_ENTRY_SIZE = 32;

            byte[] encryptionObject = ExtractEncryptionObject(fileContent) ?? throw new InvalidDataException($"The extraction of encryption object failed");
            byte[] trimmedEncryptionObject = TrimCFDictionary(encryptionObject);

            int pValue = GetNumericalEntry("/P", trimmedEncryptionObject);

            bool encryptMetadata;
            if (TryGetBooleanEntryValue("/EncryptMetadata", trimmedEncryptionObject, out bool entryValue))
            {
                encryptMetadata = entryValue;
            }
            else
            {
                encryptMetadata = false;
            }

            PDF15PasswordData pdf15PasswordData = new PDF15PasswordData()
            {
                OEntry = GetArrayEntry("/O", trimmedEncryptionObject, O_ENTRY_SIZE),
                UEntry = GetArrayEntry("/U", trimmedEncryptionObject, U_ENTRY_SIZE),
                P = BitConverter.GetBytes(pValue),
                R = GetNumericalEntry("/R", trimmedEncryptionObject),
                V = GetNumericalEntry("/V", trimmedEncryptionObject),
                KeyLength = GetNumericalEntry("/Length", trimmedEncryptionObject),
                EncryptMetadata = encryptMetadata,

                Id = ExtractFileID(fileContent)
            };

            return pdf15PasswordData;
        }

        /// <summary>
        /// Method excludes the CF dictionary from the encryption object for correct extraction of the password related entries
        /// </summary>
        /// <param name="encryptionObject"></param>
        /// <returns>Encryption object without the CF dictionary as a byte array</returns>
        private static byte[] TrimCFDictionary(byte[] encryptionObject)
        {
            const string CF_ENTRY_START = "/CF";
            const string CF_ENTRY_END = ">>>>";

            int cfEntryStart = GetStringIndexInArray(CF_ENTRY_START, encryptionObject);
            int cfEntryEnd = GetStringIndexInArray(CF_ENTRY_END, encryptionObject);

            int cfEntrySize = cfEntryEnd + CF_ENTRY_END.Length - cfEntryStart;

            byte[] finedEncryptionObject = new byte[encryptionObject.Length - cfEntrySize];

            Buffer.BlockCopy(encryptionObject, 0, finedEncryptionObject, 0, cfEntryStart);

            int srcOffset = cfEntryEnd + CF_ENTRY_END.Length;
            int dstOffset = cfEntryStart;
            int byteCount = encryptionObject.Length - srcOffset;

            Buffer.BlockCopy(encryptionObject, srcOffset, finedEncryptionObject, dstOffset, byteCount);

            return finedEncryptionObject;
        }

        /// <summary>
        /// Method returns a byte array entry from an input array. The entry in the input array can be stored as a hexadecimal string or as a string
        /// </summary>
        /// <param name="entryName">Name of the entry to search for</param>
        /// <param name="inputArray">Input array to search in</param>
        /// <param name="entrySize">Size of the entry to return</param>
        /// <returns>Entry value as a byte array</returns>
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

        /// <summary>
        /// Method gets a numerical value of an entry
        /// </summary>
        /// <param name="entryName">Name of the entry</param>
        /// <param name="inArray">Input array to seach in</param>
        /// <returns>Numerical value of the entry as an int</returns>
        /// <exception cref="FormatException">Exception is thrown, if the entry's value cannot be converted into an int</exception>
        private static int GetNumericalEntry(string entryName, byte[] inArray)
        {
            string valueString = string.Empty;

            int startIndex = GetStringIndexInArray(entryName, inArray);
            int curPointer = startIndex + entryName.Length;

            while ((inArray[curPointer] != '/') && (inArray[curPointer] != '>'))
            {
                valueString += Convert.ToChar(inArray[curPointer]);
                curPointer++;
            }
            valueString = valueString.Trim();

            if (!int.TryParse(valueString, out int value))
            {
                throw new FormatException($"Error converting the extracted \"/{entryName}\" value");
            }
            else return value;
        }

        /// <summary>
        /// Method tries to get a value of the boolean entry from the input byte array.
        /// </summary>
        /// <param name="entryName">Name of the entry as a string to search for</param>
        /// <param name="inArray">Input array to search the value in</param>
        /// <param name="entryValue">Output parameter to store the value of the boolean entry</param>
        /// <returns>Fasle, if entry has not been found. True and the entry value as the output parameter</returns>
        /// <exception cref="InvalidDataException">An exception is thrown, if the entry has been found, but has a value different from true/ false</exception>
        public static bool TryGetBooleanEntryValue(string entryName, byte[] inArray, out bool entryValue)
        {
            if (string.IsNullOrEmpty(entryName))
            {
                throw new InvalidDataException($"The entry name cannot be null or empty");
            }

            if ((inArray is null) || (inArray.Length == 0))
            {
                throw new InvalidDataException($"The input array cannot by null or empty");
            }

            if (entryName.Length > inArray.Length)
            {
                throw new InvalidDataException($"The entry name is longer than the array to search in");
            }

            string valueString = string.Empty;

            int startIndex = GetStringIndexInArray(entryName, inArray);

            // The entry has not been found in the byte array
            if (startIndex == -1)
            {
                entryValue = false;
                return false;
            }

            int curPointer = startIndex + entryName.Length;

            while ((inArray[curPointer] != '/') && (inArray[curPointer] != '>'))
            {
                valueString += Convert.ToChar(inArray[curPointer]);
                curPointer++;
            }
            valueString = valueString.Trim();

            switch (valueString)
            {
                case "true":
                    entryValue = true;
                    return true;
                case "false":
                    entryValue = false;
                    return true;
                default:
                    throw new InvalidDataException($"The bool entry contains an unknown value");
            }
        }

        /// <summary>
        /// Method extracts the encryption object from the PDF file
        /// </summary>
        /// <param name="fileContent">PDFFileContent object that contains content of a PDF file as a string and as a byte array</param>
        /// <returns>Encryption object as a byte array, if it was sucessfully extracted and null otherwise</returns>
        public static byte[] ExtractEncryptionObject(PDFFileContent fileContent)
        {
            // Pattern to find the encryption section in the PDF file
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
        /// Method extracts the ID value from the PDF document as a byte array
        /// </summary>
        /// <param name="fileContent">PDFFileContent object with the content of the PDF file as a string and as an array of bytes</param>
        /// <returns>ID value as a byte array</returns>
        public static byte[] ExtractFileID(PDFFileContent fileContent)
        {
            const int ID_ENTRY_LENGTH = 16;

            if (fileContent is null) throw new InvalidDataException($"The file content cannot be null");

            // The /ID shall present in any encrypted PDF file
            // Regexp for the /ID value in the file
            string idValuePattern = @"/ID.*\[\<.*?\>\]";
            Regex idValueRegex = new Regex(idValuePattern, RegexOptions.Singleline);
            Match regexMatch = idValueRegex.Match(fileContent.AsString);
            if (!regexMatch.Success)
            {
                throw new InvalidDataException($"The encrypted PDF file is malformed- the file identifier (\"/ID\" array) is missing");
            }

            // Regexp to extract the first entry in the ID array
            string idFirstValuePattern = @"\<.*?\>";
            Regex idFirstValueRegexp = new Regex(idFirstValuePattern, RegexOptions.Singleline);
            regexMatch = idFirstValueRegexp.Match(regexMatch.Value);
            if (!regexMatch.Success)
            {
                throw new InvalidDataException($"The encrypted PDF file is malformed- the first string in the \"/ID\" array is missing");
            }
            string idFirstValue = regexMatch.Value.Trim('<', '>');

            if (string.IsNullOrEmpty(idFirstValue))
            {
                throw new InvalidDataException($"The encrypted PDF file is malformed- the first ID string is empty");
            }

            if (idFirstValue.Length != ID_ENTRY_LENGTH * 2)
            {
                throw new InvalidDataException($"The extracted \"/ID\" array is too short");
            }

            byte[] idArray = PDFPassRecoverLib.ConvertHexStringToByteArray(idFirstValue);

            return idArray;
        }

        /// <summary>
        /// Method extracts a literal byte array (ending with the ")") from the input array starting from a certain poisition and stores it in the output array of the predefined size
        /// </summary>
        /// <param name="inArray">Array where to search for</param>
        /// <param name="startPosition">Starting position in the input array</param>
        /// <param name="outArraySize">Size of the output array</param>
        /// <returns>Literal byte array</returns>
        public static byte[] ExtractLiteralByteArray(byte[] inArray, int startPosition, int outArraySize)
        {
            if (inArray is null)
            {
                throw new InvalidDataException($"The input array cannnot be null");
            }

            if (inArray.Length < 3)
            {
                throw new InvalidDataException($"The length of the inout array should be at least 3 bytes");
            }

            if (startPosition <= 0)
            {
                throw new InvalidDataException($"The start position in the input array cannot be negative or zero");
            }

            if (startPosition > inArray.Length)
            {
                throw new InvalidDataException($"The start position exceeds the size of the input array");
            }

            if (outArraySize <= 0)
            {
                throw new InvalidDataException($"The output array size cannot be negative or zero");
            }

            byte[] outArray = new byte[outArraySize];

            int i = 0;          // Counter to track 
            int inCnt = 0;      // Counter to navigate in the input array
            int outCnt = 0;     // Counter to navigate in the output array
            while ((startPosition + i) < inArray.Length)
            {
                if (inArray[startPosition + inCnt] == ')')
                {
                    return outArray;
                }

                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == 'n'))
                {
                    outArray[outCnt] = 0x0A;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }

                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == 'r'))
                {
                    outArray[outCnt] = 0x0D;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }

                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == 't'))
                {
                    outArray[outCnt] = 0x09;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }

                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == 'b'))
                {
                    outArray[outCnt] = 0x08;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }

                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == 'f'))
                {
                    outArray[outCnt] = 0x0C;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }
                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == '\\'))
                {
                    outArray[outCnt] = 0x5C;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }
                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == '('))
                {
                    outArray[outCnt] = 0x28;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }
                if ((inArray[startPosition + inCnt] == '\\') && (inArray[startPosition + inCnt + 1] == ')'))
                {
                    outArray[outCnt] = 0x29;
                    inCnt += 2;
                    outCnt++;
                    continue;
                }

                outArray[outCnt] = inArray[startPosition + inCnt];
                inCnt++;
                outCnt++;
            }

            throw new InvalidDataException($"The input literal array doesn't contain the closing parenthese");
        }

        /// <summary>
        /// Method resturns an index of a string in the array, but firstly the string is converted into a byte array
        /// </summary>
        /// <param name="inString">String to search for</param>
        /// <param name="inArray">Input array to search in</param>
        /// <returns>Returns an index of the input string or -1, if the string has not been found</returns>
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

            if (inString.Length > inArray.Length)
            {
                throw new InvalidDataException($"The input string is longer than the input byte array");
            }

            // Converting the string into a byte representation first
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
