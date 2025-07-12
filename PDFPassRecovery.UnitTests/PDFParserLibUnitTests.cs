using System;
using System.IO;
using Xunit;

namespace PDFPassRecovery.UnitTests
{
    /// <summary>
    /// Class with unit tests for PDF parser lib
    /// </summary>
    public class PDFParserLibUnitTests
    {
        #region *************** ExtractIDEntry Tests ***************

        [Fact]
        // Extract PDF ID entry stored as "ID [<><>]" or with other allowed space symbols between the "ID" and the start of the array
        public void ExractIDEntry_WithSpacesTest()
        {
            // Arrange
            string testID1 = "/ID" + ' ' + "[<47290f1b92a06444885ac3a06c1f5877><50bdbeff05f1c346844267eaa943c792>]";
            string testID2 = "/ID" + '\n' + "[<47290f1b92a06444885ac3a06c1f5877><50bdbeff05f1c346844267eaa943c792>]";
            string testID3 = "/ID" + '\n' + '\r' + "[<47290f1b92a06444885ac3a06c1f5877><50bdbeff05f1c346844267eaa943c792>]";

            PDFFileContent fileContent1 = new PDFFileContent(testID1, null);
            PDFFileContent fileContent2 = new PDFFileContent(testID2, null);
            PDFFileContent fileContent3 = new PDFFileContent(testID3, null);

            // Act
            byte[] array1 = PDFParserLib.ExtractFileID(fileContent1);
            string arrayString1 = BitConverter.ToString(array1);

            byte[] array2 = PDFParserLib.ExtractFileID(fileContent2);
            string arrayString2 = BitConverter.ToString(array2);

            byte[] array3 = PDFParserLib.ExtractFileID(fileContent3);
            string arrayString3 = BitConverter.ToString(array3);

            // Assert--> array 1
            Assert.NotNull(array1);
            Assert.Equal(16, array1.Length);
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString1);

            // Assert--> array 2
            Assert.NotNull(array2);
            Assert.Equal(16, array2.Length);
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString2);

            // Assert--> array 3
            Assert.NotNull(array3);
            Assert.Equal(16, array3.Length);
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString3);
        }

        [Fact]
        // Extract PDF ID entry stored as "ID[<><>]" and without any allowed space symbols between the "ID" and the start of the array
        public void ExractIDEntryWithoutSpacesTest()
        {
            // Arrange
            string testID = "/ID[<47290f1b92a06444885ac3a06c1f5877><50bdbeff05f1c346844267eaa943c792>]";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            // Act
            byte[] array1 = PDFParserLib.ExtractFileID(fileContent);
            string arrayString1 = BitConverter.ToString(array1);

            // Assert
            Assert.NotNull(array1);
            Assert.Equal(16, array1.Length);
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString1);
        }

        [Fact]
        public void ExractIDEntry_FileContentIsNull_Test()
        {
            // Arrange
            PDFFileContent fileContent = null;
            Type exceptionType = typeof(InvalidDataException);
            string expectedMessage = $"The file content cannot be null";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_NoIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            string expectedMessage = $"The encrypted PDF file is malformed- the file identifier (\"/ID\" array) is missing";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_MissingBothIDEntriesInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            string expectedMessage = $"The encrypted PDF file is malformed- the file identifier (\"/ID\" array) is missing";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_MissingSecondIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<b87c2efcf74c814a8b289862bced8585>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            // Act
            byte[] array = PDFParserLib.ExtractFileID(fileContent);
            string arrayString = BitConverter.ToString(array);

            // Assert
            Assert.NotNull(array);
            Assert.Equal(16, array.Length);
            Assert.Equal("b8-7c-2e-fc-f7-4c-81-4a-8b-28-98-62-bc-ed-85-85".ToUpper(), arrayString);
        }

        [Fact]
        public void ExractIDEntry_EmptyBothIDEntriesInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<><>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            string expectedMessage = $"The encrypted PDF file is malformed- the first ID string is empty";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_EmptyFirstIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<><e22450907051744d8a778195fd01a6c1>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            string expectedMessage = $"The extracted \"/ID\" array is too short";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_EmptySecondIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<b87c2efcf74c814a8b289862bced8585><>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            // Act
            byte[] array = PDFParserLib.ExtractFileID(fileContent);
            string arrayString = BitConverter.ToString(array);

            // Assert
            Assert.NotNull(array);
            Assert.Equal(16, array.Length);
            Assert.Equal("b8-7c-2e-fc-f7-4c-81-4a-8b-28-98-62-bc-ed-85-85".ToUpper(), arrayString);
        }

        [Fact]
        public void ExractIDEntry_IDEntryIsTooShort_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<2efcf74c814a8b289862bced8585><>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            string expectedMessage = $"The extracted \"/ID\" array is too short";

            // Act

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractFileID(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }
        #endregion

        #region *************** Bool Entry Extraction Tests ***************
        [Fact]
        public void TryGetBooleanEntryValue_True_Test()
        {
            // Arrange
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
            000003B0  61 20 66 61 6C 73 65 3E  3E 0D 65 6E 64 6F 62 6A  a true>>.endobj
             */

            byte[] encryptionArray = {0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x4C,
                                    0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72,
                                    0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x2F, 0x4F, 0x28, 0xB1, 0xF4, 0xA0, 0x77,
                                    0xBE, 0x87, 0xC3, 0xC0, 0xF5, 0x97, 0x5D, 0x5C, 0x72, 0xB1, 0x7F, 0xCD, 0x9E, 0x7C, 0x88, 0x4B,
                                    0x40, 0x33, 0x48, 0x79, 0xA5, 0x27, 0x51, 0x33, 0xB2, 0xC5, 0xFF, 0x67, 0x07, 0x29, 0x2F, 0x50,
                                    0x20, 0x2D, 0x33, 0x31, 0x33, 0x32, 0x2F, 0x55, 0x28, 0x41, 0x94, 0xE6, 0x58, 0x50, 0xEF, 0x3D,
                                    0x20, 0xE1, 0xC9, 0xBA, 0x2D, 0x30, 0xCF, 0xC4, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0x2F, 0x56, 0x20, 0x34, 0x2F, 0x43,
                                    0x46, 0x3C, 0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x4C, 0x65, 0x6E, 0x67,
                                    0x74, 0x68, 0x20, 0x31, 0x36, 0x2F, 0x43, 0x46, 0x4D, 0x2F, 0x56, 0x32, 0x2F, 0x41, 0x75, 0x74,
                                    0x68, 0x45, 0x76, 0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x3E, 0x3E,
                                    0x3E, 0x3E, 0x2F, 0x53, 0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74,
                                    0x72, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x45, 0x6E, 0x63, 0x72, 0x79, 0x70, 0x74,
                                    0x4D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x20, 0x74, 0x72, 0x75, 0x65, 0x3E, 0x3E,
                                    0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};

            string entryName = "/EncryptMetadata";

            // Act
            bool extractionResult = PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionArray, out bool entryValue);

            // Assert
            Assert.True(extractionResult);
            Assert.True(entryValue);
        }

        [Fact]
        public void TryGetBooleanEntryValue_False_Test()
        {
            // Arrange
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

            byte[] encryptionArray = {0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x4C,
                                    0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72,
                                    0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x2F, 0x4F, 0x28, 0xB1, 0xF4, 0xA0, 0x77,
                                    0xBE, 0x87, 0xC3, 0xC0, 0xF5, 0x97, 0x5D, 0x5C, 0x72, 0xB1, 0x7F, 0xCD, 0x9E, 0x7C, 0x88, 0x4B,
                                    0x40, 0x33, 0x48, 0x79, 0xA5, 0x27, 0x51, 0x33, 0xB2, 0xC5, 0xFF, 0x67, 0x07, 0x29, 0x2F, 0x50,
                                    0x20, 0x2D, 0x33, 0x31, 0x33, 0x32, 0x2F, 0x55, 0x28, 0x41, 0x94, 0xE6, 0x58, 0x50, 0xEF, 0x3D,
                                    0x20, 0xE1, 0xC9, 0xBA, 0x2D, 0x30, 0xCF, 0xC4, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0x2F, 0x56, 0x20, 0x34, 0x2F, 0x43,
                                    0x46, 0x3C, 0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x4C, 0x65, 0x6E, 0x67,
                                    0x74, 0x68, 0x20, 0x31, 0x36, 0x2F, 0x43, 0x46, 0x4D, 0x2F, 0x56, 0x32, 0x2F, 0x41, 0x75, 0x74,
                                    0x68, 0x45, 0x76, 0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x3E, 0x3E,
                                    0x3E, 0x3E, 0x2F, 0x53, 0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74,
                                    0x72, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x45, 0x6E, 0x63, 0x72, 0x79, 0x70, 0x74,
                                    0x4D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x20, 0x66, 0x61, 0x6C, 0x73, 0x65, 0x3E, 0x3E,
                                    0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};

            string entryName = "/EncryptMetadata";

            // Act
            bool extractionResult = PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionArray, out bool entryValue);

            // Assert
            Assert.True(extractionResult);
            Assert.False(entryValue);
        }

        [Fact]
        public void TryGetBooleanEntryValue_NullEntryName_Test()
        {
            // Arrange
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

            byte[] encryptionArray = {0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x4C,
                                    0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72,
                                    0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x2F, 0x4F, 0x28, 0xB1, 0xF4, 0xA0, 0x77,
                                    0xBE, 0x87, 0xC3, 0xC0, 0xF5, 0x97, 0x5D, 0x5C, 0x72, 0xB1, 0x7F, 0xCD, 0x9E, 0x7C, 0x88, 0x4B,
                                    0x40, 0x33, 0x48, 0x79, 0xA5, 0x27, 0x51, 0x33, 0xB2, 0xC5, 0xFF, 0x67, 0x07, 0x29, 0x2F, 0x50,
                                    0x20, 0x2D, 0x33, 0x31, 0x33, 0x32, 0x2F, 0x55, 0x28, 0x41, 0x94, 0xE6, 0x58, 0x50, 0xEF, 0x3D,
                                    0x20, 0xE1, 0xC9, 0xBA, 0x2D, 0x30, 0xCF, 0xC4, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0x2F, 0x56, 0x20, 0x34, 0x2F, 0x43,
                                    0x46, 0x3C, 0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x4C, 0x65, 0x6E, 0x67,
                                    0x74, 0x68, 0x20, 0x31, 0x36, 0x2F, 0x43, 0x46, 0x4D, 0x2F, 0x56, 0x32, 0x2F, 0x41, 0x75, 0x74,
                                    0x68, 0x45, 0x76, 0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x3E, 0x3E,
                                    0x3E, 0x3E, 0x2F, 0x53, 0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74,
                                    0x72, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x45, 0x6E, 0x63, 0x72, 0x79, 0x70, 0x74,
                                    0x4D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x20, 0x66, 0x61, 0x6C, 0x73, 0x65, 0x3E, 0x3E,
                                    0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};

            string entryName = null;
            string expectedMessage = $"The entry name cannot be null or empty";
            Type exceptionType = typeof(InvalidDataException);

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionArray, out bool entryValue));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void TryGetBooleanEntryValue_NullInputArray_Test()
        {
            byte[] encryptionObject = null;

            string entryName = "EncryptMetadata";
            string expectedMessage = $"The input array cannot by null or empty";
            Type exceptionType = typeof(InvalidDataException);

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionObject, out bool entryValue));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void TryGetBooleanEntryValue_EmptyInputArray_Test()
        {
            byte[] encryptionObject = { };

            string entryName = "EncryptMetadata";
            string expectedMessage = $"The input array cannot by null or empty";
            Type exceptionType = typeof(InvalidDataException);

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionObject, out bool entryValue));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void TryGetBooleanEntryValue_TooLongEntryName_Test()
        {
            byte[] encryptionObject = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };

            string entryName = "/EncryptMetadata";
            string expectedMessage = $"The entry name is longer than the array to search in";
            Type exceptionType = typeof(InvalidDataException);

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionObject, out bool entryValue));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void TryGetBooleanEntryValue_UnknownEntryValue_Test()
        {
            // Arrange
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
            000003B0  61 20 66 61 6C 73 65 3E  3E 0D 65 6E 64 6F 62 6A  a unknown>>.endobj
             */

            byte[] encryptionArray = {0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x4C,
                                    0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72,
                                    0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x2F, 0x4F, 0x28, 0xB1, 0xF4, 0xA0, 0x77,
                                    0xBE, 0x87, 0xC3, 0xC0, 0xF5, 0x97, 0x5D, 0x5C, 0x72, 0xB1, 0x7F, 0xCD, 0x9E, 0x7C, 0x88, 0x4B,
                                    0x40, 0x33, 0x48, 0x79, 0xA5, 0x27, 0x51, 0x33, 0xB2, 0xC5, 0xFF, 0x67, 0x07, 0x29, 0x2F, 0x50,
                                    0x20, 0x2D, 0x33, 0x31, 0x33, 0x32, 0x2F, 0x55, 0x28, 0x41, 0x94, 0xE6, 0x58, 0x50, 0xEF, 0x3D,
                                    0x20, 0xE1, 0xC9, 0xBA, 0x2D, 0x30, 0xCF, 0xC4, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0x2F, 0x56, 0x20, 0x34, 0x2F, 0x43,
                                    0x46, 0x3C, 0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x4C, 0x65, 0x6E, 0x67,
                                    0x74, 0x68, 0x20, 0x31, 0x36, 0x2F, 0x43, 0x46, 0x4D, 0x2F, 0x56, 0x32, 0x2F, 0x41, 0x75, 0x74,
                                    0x68, 0x45, 0x76, 0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x3E, 0x3E,
                                    0x3E, 0x3E, 0x2F, 0x53, 0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74,
                                    0x72, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x45, 0x6E, 0x63, 0x72, 0x79, 0x70, 0x74,
                                    0x4D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x20, 0x75, 0x6E, 0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x3E, 0x3E,
                                    0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};

            string entryName = "/EncryptMetadata";
            string expectedMessage = $"The bool entry contains an unknown value";
            Type exceptionType = typeof(InvalidDataException);

            // Assert
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionArray, out bool entryValue));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void TryGetBooleanEntryValue_EntryNotFound_Test()
        {
            // Arrange

            /*
             * Test encryption object from a PDF1.6 file with no "/EncryptMetadata" entry
             * 
             * "85 0 obj <</ CF <</ StdCF <</ AuthEvent / DocOpen / CFM / AESV2 / Length 16 >>>>
             * / Filter / Standard / Length 128 / O < DE31DCBCEBFED0F6CBC6D1B3ED41EB6A94195F5A4AA2BB1D69CDAA79B8962AC1 >
             * / P - 1060 / R 4 / StmF / StdCF / StrF / StdCF / U < E1144B1FAFCA41AA6EC6D09134179B0F00000000000000000000000000000000 >
             * / V 4 >> endobj";
             */
            byte[] encryptionArray = { 0x38, 0x35, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0D, 0x0A, 0x3C, 0x3C, 0x2F, 0x43, 0x46, 0x3C,
                            0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x41, 0x75, 0x74, 0x68, 0x45, 0x76,
                            0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x2F, 0x43, 0x46, 0x4D, 0x2F,
                            0x41, 0x45, 0x53, 0x56, 0x32, 0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x36, 0x3E,
                            0x3E, 0x3E, 0x3E, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72, 0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64,
                            0x61, 0x72, 0x64, 0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x4F,
                            0x3C, 0x44, 0x45, 0x33, 0x31, 0x44, 0x43, 0x42, 0x43, 0x45, 0x42, 0x46, 0x45, 0x44, 0x30, 0x46,
                            0x36, 0x43, 0x42, 0x43, 0x36, 0x44, 0x31, 0x42, 0x33, 0x45, 0x44, 0x34, 0x31, 0x45, 0x42, 0x36,
                            0x41, 0x39, 0x34, 0x31, 0x39, 0x35, 0x46, 0x35, 0x41, 0x34, 0x41, 0x41, 0x32, 0x42, 0x42, 0x31,
                            0x44, 0x36, 0x39, 0x43, 0x44, 0x41, 0x41, 0x37, 0x39, 0x42, 0x38, 0x39, 0x36, 0x32, 0x41, 0x43,
                            0x31, 0x3E, 0x2F, 0x50, 0x20, 0x2D, 0x31, 0x30, 0x36, 0x30, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x53,
                            0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74, 0x72, 0x46, 0x2F, 0x53,
                            0x74, 0x64, 0x43, 0x46, 0x2F, 0x55, 0x3C, 0x45, 0x31, 0x31, 0x34, 0x34, 0x42, 0x31, 0x46, 0x41,
                            0x46, 0x43, 0x41, 0x34, 0x31, 0x41, 0x41, 0x36, 0x45, 0x43, 0x36, 0x44, 0x30, 0x39, 0x31, 0x33,
                            0x34, 0x31, 0x37, 0x39, 0x42, 0x30, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                            0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                            0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x3E, 0x2F, 0x56, 0x20, 0x34, 0x3E, 0x3E, 0x0D, 0x0A,
                            0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};

            string entryName = "/EncryptMetadata";

            // Act
            bool extractionResult = PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionArray, out bool entryValue);

            // Assert
            Assert.False(extractionResult);
        }
        #endregion

        #region *************** Byte Array Entry Extraction Tests ***************
        [Fact]
        public void ExtractLiteralByteArray_NullInArray_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void ExtractLiteralByteArray_EmptyInArray_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void ExtractLiteralByteArray_NegativeStartPosition_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void ExtractLiteralByteArray_StartPositionGreaterThanInputLength_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void ExtractLiteralByteArray_NegativeOrZeroOutArraySize_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        // Extract a normal entry
        public void ExtractLiteralByteArray_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        // Extract an entry without the closing brace
        public void ExtractLiteralByteArray_WithoutClosingBrace_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }
        #endregion

        #region *************** Numeric Entry Extraction Tests ***************

        #endregion

        #region *************** Encryption Key Length Extraction Tests ***************
        [Fact]
        public void PDF14_40bits_KeyLengthExtraction_Test()
        {
            // Arrange
            /*
            Hex View  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F
            000003A0                    32 31  20 30 20 6F 62 6A 0D 3C        21 0 obj.<
            000003B0  3C 20 0D 2F 46 69 6C 74  65 72 20 2F 53 74 61 6E < ./ Filter / Stan
            000003C0  64 61 72 64 20 0D 2F 52  20 32 20 0D 2F 4F 20 28  dard./ R 2./ O(
            000003D0  5D 03 A4 78 D5 0F 9F D8  DD 80 24 26 F4 5E D6 C5  ]..x......$&.^ ..000003E0  F9 23 A1 2C 39 FD FD AC FC D6 32 9D E0 D3 E4 45.#.,9.....2....E
            000003F0  29 0D 2F 55 20 28 01 69  8C D8 97 12 AD B9 E5 CE  )./ U(.i........00000400  C9 CA 95 AC C9 90 31 1B  EC 72 AA 97 5F F1 8A D8......1..r.._...
            00000410  7C F9 BB A8 F8 07 29 0D  2F 50 20 2D 32 34 20 0D | .....)./ P - 24.
            00000420  2F 56 20 31 20 0D 2F 4C  65 6E 67 74 68 20 34 30 / V 1./ Length 40
            00000430  20 0D 3E 3E 20 0D 65 6E  64 6F 62 6A.>> .endobj
            */
            byte[] encryptionArray = { 0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0D, 0x3C, 0x3C, 0x20, 0x0D, 0x2F, 0x46, 0x69,
                            0x6C, 0x74, 0x65, 0x72, 0x20, 0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x20, 0x0D,
                            0x2F, 0x52, 0x20, 0x32, 0x20, 0x0D, 0x2F, 0x4F, 0x20, 0x28, 0x5D, 0x03, 0xA4, 0x78, 0xD5, 0x0F,
                            0x9F, 0xD8, 0xDD, 0x80, 0x24, 0x26, 0xF4, 0x5E, 0xD6, 0xC5, 0xF9, 0x23, 0xA1, 0x2C, 0x39, 0xFD,
                            0xFD, 0xAC, 0xFC, 0xD6, 0x32, 0x9D, 0xE0, 0xD3, 0xE4, 0x45, 0x29, 0x0D, 0x2F, 0x55, 0x20, 0x28,
                            0x01, 0x69, 0x8C, 0xD8, 0x97, 0x12, 0xAD, 0xB9, 0xE5, 0xCE, 0xC9, 0xCA, 0x95, 0xAC, 0xC9, 0x90,
                            0x31, 0x1B, 0xEC, 0x72, 0xAA, 0x97, 0x5F, 0xF1, 0x8A, 0xD8, 0x7C, 0xF9, 0xBB, 0xA8, 0xF8, 0x07,
                            0x29, 0x0D, 0x2F, 0x50, 0x20, 0x2D, 0x32, 0x34, 0x20, 0x0D, 0x2F, 0x56, 0x20, 0x31, 0x20, 0x0D,
                            0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x34, 0x30, 0x20, 0x0D, 0x3E, 0x3E, 0x20, 0x0D,
                            0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A};
            string entryName = "/Length";
            int expectedKeyLength = 40;

            // Act
            int keyLength = PDFParserLib.GetNumericalEntry(entryName, encryptionArray);

            // Assert
            Assert.Equal(expectedKeyLength, keyLength);
        }

        [Fact]
        public void PDF14_128bits_KeyLengthExtraction_Test()
        {
            // Arrange
            /*
            Hex View  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F
            000003A0                       32  31 20 30 20 6F 62 6A 0D         21 0 obj.
            000003B0  3C 3C 20 0D 2F 46 69 6C  74 65 72 20 2F 53 74 61  << ./Filter /Sta
            000003C0  6E 64 61 72 64 20 0D 2F  52 20 33 20 0D 2F 4F 20  ndard ./R 3 ./O 
            000003D0  28 1F 1A 3A EA 3A 71 C8  C7 9E F8 04 62 D8 DE DB  (..:.:q.....b...
            000003E0  F6 D3 97 AD AE 94 15 D1  F7 4D CC 8B 06 9D 14 BF  .........M......
            000003F0  6D 29 0D 2F 55 20 28 43  5E CB 35 9B 4E 16 5C 72  m)./U (C^.5.N.\r
            00000400  8F BB 5C 28 8B D3 4B 78  DE 00 00 00 00 00 00 00  ..\(..Kx........
            00000410  00 00 00 00 00 00 00 00  00 29 0D 2F 50 20 2D 33  .........)./P -3
            00000420  36 30 30 20 0D 2F 56 20  32 20 0D 2F 4C 65 6E 67  600 ./V 2 ./Leng
            00000430  74 68 20 31 32 38 20 0D  3E 3E 20 0D 65 6E 64 6F  th 128 .>> .endo
            00000440  62 6A 0D 32 32 20 30 20  6F 62 6A                 bj.22 0 obj
             */

            byte[] encryptionArray = { 0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0D, 0x3C, 0x3C, 0x20, 0x0D, 0x2F, 0x46, 0x69,
                                       0x6C, 0x74, 0x65, 0x72, 0x20, 0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x20, 0x0D,
                                       0x2F, 0x52, 0x20, 0x33, 0x20, 0x0D, 0x2F, 0x4F, 0x20, 0x28, 0x1F, 0x1A, 0x3A, 0xEA, 0x3A, 0x71,
                                       0xC8, 0xC7, 0x9E, 0xF8, 0x04, 0x62, 0xD8, 0xDE, 0xDB, 0xF6, 0xD3, 0x97, 0xAD, 0xAE, 0x94, 0x15,
                                       0xD1, 0xF7, 0x4D, 0xCC, 0x8B, 0x06, 0x9D, 0x14, 0xBF, 0x6D, 0x29, 0x0D, 0x2F, 0x55, 0x20, 0x28,
                                       0x43, 0x5E, 0xCB, 0x35, 0x9B, 0x4E, 0x16, 0x5C, 0x72, 0x8F, 0xBB, 0x5C, 0x28, 0x8B, 0xD3, 0x4B,
                                       0x78, 0xDE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                       0x00, 0x00, 0x29, 0x0D, 0x2F, 0x50, 0x20, 0x2D, 0x33, 0x36, 0x30, 0x30, 0x20, 0x0D, 0x2F, 0x56,
                                       0x20, 0x32, 0x20, 0x0D, 0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x20,
                                       0x0D, 0x3E, 0x3E, 0x20, 0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0D, 0x32, 0x32, 0x20, 0x30,
                                       0x20, 0x6F, 0x62, 0x6A };
            string entryName = "/Length";
            int expectedKeyLength = 128;

            // Act
            int keyLength = PDFParserLib.GetNumericalEntry(entryName, encryptionArray);

            // Assert
            Assert.Equal(expectedKeyLength, keyLength);
        }

        [Fact]
        public void PDF15_KeyLengthExtraction_Test_1()
        {
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
            byte[] encryptionArray = { 0x32, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x3C, 0x3C, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x4C,
                                      0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72,
                                      0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x2F, 0x4F, 0x28, 0xB1, 0xF4, 0xA0, 0x77,
                                      0xBE, 0x87, 0xC3, 0xC0, 0xF5, 0x97, 0x5D, 0x5C, 0x72, 0xB1, 0x7F, 0xCD, 0x9E, 0x7C, 0x88, 0x4B,
                                      0x40, 0x33, 0x48, 0x79, 0xA5, 0x27, 0x51, 0x33, 0xB2, 0xC5, 0xFF, 0x67, 0x07, 0x29, 0x2F, 0x50,
                                      0x20, 0x2D, 0x33, 0x31, 0x33, 0x32, 0x2F, 0x55, 0x28, 0x41, 0x94, 0xE6, 0x58, 0x50, 0xEF, 0x3D,
                                      0x20, 0xE1, 0xC9, 0xBA, 0x2D, 0x30, 0xCF, 0xC4, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0x2F, 0x56, 0x20, 0x34, 0x2F, 0x43,
                                      0x46, 0x3C, 0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x4C, 0x65, 0x6E, 0x67,
                                      0x74, 0x68, 0x20, 0x31, 0x36, 0x2F, 0x43, 0x46, 0x4D, 0x2F, 0x56, 0x32, 0x2F, 0x41, 0x75, 0x74,
                                      0x68, 0x45, 0x76, 0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x3E, 0x3E,
                                      0x3E, 0x3E, 0x2F, 0x53, 0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74,
                                      0x72, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x45, 0x6E, 0x63, 0x72, 0x79, 0x70, 0x74,
                                      0x4D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x20, 0x66, 0x61, 0x6C, 0x73, 0x65, 0x3E, 0x3E,
                                      0x0D, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A };
            
            PDFEncryptionObject fullEncryptionObject= new PDFEncryptionObject();
            fullEncryptionObject.AsString = @"21 0 obj<</R 4/Length 128/Filter/Standard/O(...w......]\r....|.K@3Hy.'Q3...g.)/P -3132/U(A..XP.= ...-0...................)/V 4/CF<</StdCF<</Length 16/CFM/V2/AuthEvent/DocOpen>>>>/StmF/StdCF/StrF/StdCF/EncryptMetadata false>>.endobj";
            fullEncryptionObject.AsBytes= encryptionArray;
            fullEncryptionObject.Size= encryptionArray.Length;

            byte[] trimmedEncryptionArray= PDFParserLib.TrimCFDictionary(fullEncryptionObject).AsBytes;

            string entryName = "/Length";
            int expectedKeyLength = 128;

            // Act
            int keyLength = PDFParserLib.GetNumericalEntry(entryName, trimmedEncryptionArray);

            // Assert
            Assert.Equal(expectedKeyLength, keyLength);
        }

        [Fact]
        public void PDF16_KeyLengthExtraction_Test_2()
        {
            // Arrange
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

            byte[] encryptionArray = { 0x38, 0x35, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0D, 0x0A, 0x3C, 0x3C, 0x2F, 0x43, 0x46, 0x3C,
                                       0x3C, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x3C, 0x3C, 0x2F, 0x41, 0x75, 0x74, 0x68, 0x45, 0x76,
                                       0x65, 0x6E, 0x74, 0x2F, 0x44, 0x6F, 0x63, 0x4F, 0x70, 0x65, 0x6E, 0x2F, 0x43, 0x46, 0x4D, 0x2F,
                                       0x41, 0x45, 0x53, 0x56, 0x32, 0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x36, 0x3E,
                                       0x3E, 0x3E, 0x3E, 0x2F, 0x46, 0x69, 0x6C, 0x74, 0x65, 0x72, 0x2F, 0x53, 0x74, 0x61, 0x6E, 0x64,
                                       0x61, 0x72, 0x64, 0x2F, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x20, 0x31, 0x32, 0x38, 0x2F, 0x4F,
                                       0x3C, 0x44, 0x45, 0x33, 0x31, 0x44, 0x43, 0x42, 0x43, 0x45, 0x42, 0x46, 0x45, 0x44, 0x30, 0x46,
                                       0x36, 0x43, 0x42, 0x43, 0x36, 0x44, 0x31, 0x42, 0x33, 0x45, 0x44, 0x34, 0x31, 0x45, 0x42, 0x36,
                                       0x41, 0x39, 0x34, 0x31, 0x39, 0x35, 0x46, 0x35, 0x41, 0x34, 0x41, 0x41, 0x32, 0x42, 0x42, 0x31,
                                       0x44, 0x36, 0x39, 0x43, 0x44, 0x41, 0x41, 0x37, 0x39, 0x42, 0x38, 0x39, 0x36, 0x32, 0x41, 0x43,
                                       0x31, 0x3E, 0x2F, 0x50, 0x20, 0x2D, 0x31, 0x30, 0x36, 0x30, 0x2F, 0x52, 0x20, 0x34, 0x2F, 0x53,
                                       0x74, 0x6D, 0x46, 0x2F, 0x53, 0x74, 0x64, 0x43, 0x46, 0x2F, 0x53, 0x74, 0x72, 0x46, 0x2F, 0x53,
                                       0x74, 0x64, 0x43, 0x46, 0x2F, 0x55, 0x3C, 0x45, 0x31, 0x31, 0x34, 0x34, 0x42, 0x31, 0x46, 0x41,
                                       0x46, 0x43, 0x41, 0x34, 0x31, 0x41, 0x41, 0x36, 0x45, 0x43, 0x36, 0x44, 0x30, 0x39, 0x31, 0x33,
                                       0x34, 0x31, 0x37, 0x39, 0x42, 0x30, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                                       0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                                       0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x3E, 0x2F, 0x56, 0x20, 0x34, 0x3E, 0x3E, 0x0D, 0x0A,
                                       0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A };

            PDFEncryptionObject fullEncryptionObject = new PDFEncryptionObject();
            fullEncryptionObject.AsString = @"85 0 obj..<</CF<</StdCF<</AuthEvent/DocOpen/CFM/AESV2/Length 16>>>>/Filter/Standard/Length 128/O<DE31DCBCEBFED0F6CBC6D1B3ED41EB6A94195F5A4AA2BB1D69CDAA79B8962AC1> /P -1060/R 4/StmF /StdCF /StrF /StdCF /U< E1144B1FAFCA41AA6EC6D09134179B0F00000000000000000000000000000000>/V4>> ..endobj";
            fullEncryptionObject.AsBytes = encryptionArray;
            fullEncryptionObject.Size = encryptionArray.Length;

            byte[] trimmedEncryptionArray = PDFParserLib.TrimCFDictionary(fullEncryptionObject).AsBytes;

            string entryName = "/Length";
            int expectedKeyLength = 128;

            // Act
            int keyLength = PDFParserLib.GetNumericalEntry(entryName, trimmedEncryptionArray);

            // Assert
            Assert.Equal(expectedKeyLength, keyLength);
        }
        #endregion
    }
}

//[Fact]
//public void TEST_NAME_Test()
//{
//    // Arrange
//    // Act
//    // Assert
//    throw new NotImplementedException();
//}
