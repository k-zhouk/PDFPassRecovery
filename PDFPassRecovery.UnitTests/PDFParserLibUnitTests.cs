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
            byte[] array1 = PDFParserLib.ExtractIDValue(fileContent1);
            string arrayString1 = BitConverter.ToString(array1);

            byte[] array2 = PDFParserLib.ExtractIDValue(fileContent2);
            string arrayString2 = BitConverter.ToString(array2);

            byte[] array3 = PDFParserLib.ExtractIDValue(fileContent3);
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
            byte[] array1 = PDFParserLib.ExtractIDValue(fileContent);
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_MissingSecondIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<b87c2efcf74c814a8b289862bced8585>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            // Act
            byte[] array = PDFParserLib.ExtractIDValue(fileContent);
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExractIDEntry_EmptySecondIDEntryInFileContent_Test()
        {
            // Arrange
            string testID = "trailer <</Size 33/Prev 6712/Root 21 0 R/Info 19 0 R/ID[<b87c2efcf74c814a8b289862bced8585><>]>> startxref 0 %%EOF";
            PDFFileContent fileContent = new PDFFileContent(testID, null);

            // Act
            byte[] array = PDFParserLib.ExtractIDValue(fileContent);
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
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => PDFParserLib.ExtractIDValue(fileContent));
            Assert.Equal(expectedMessage, ex.Message);
        }
        #endregion

        #region *************** Bool Entry Extraction Tests ***************
        [Fact]
        public void TryGetBooleanEntryValue_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_MissingEntryName_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_NullInputArray_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_EmptyInputArray_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_TooLongEntryName_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_UnknownEntryValue_Test()
        {
            // Arrange
            // Act
            // Assert
            throw new NotImplementedException();
        }

        [Fact]
        public void TryGetBooleanEntryValue_EntryNotFound_Test()
        {
            // Arrange

            // Test encryption object from PDF1.6 file with no "/EncryptMetadata" entry
            // "85 0 obj <</ CF <</ StdCF <</ AuthEvent / DocOpen / CFM / AESV2 / Length 16 >>>>/ Filter / Standard / Length 128 / O < DE31DCBCEBFED0F6CBC6D1B3ED41EB6A94195F5A4AA2BB1D69CDAA79B8962AC1 >/ P - 1060 / R 4 / StmF / StdCF / StrF / StdCF / U < E1144B1FAFCA41AA6EC6D09134179B0F00000000000000000000000000000000 >/ V 4 >> endobj";
            byte[] encryptionObject = { 0x38, 0x35, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0D, 0x0A, 0x3C, 0x3C, 0x2F, 0x43, 0x46, 0x3C,
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
            bool extractionResult = PDFParserLib.TryGetBooleanEntryValue(entryName, encryptionObject, out bool entryValue);

            // Assert
            Assert.False(extractionResult);
        }
        #endregion

        #region *************** Encryption Key Length Extraction Tests ***************

        #endregion

        #region *************** Byte Array Entry Extraction Tests ***************
        [Fact]
        public void ExtractLiteralByteArray_NullInArray_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExtractLiteralByteArray_EmptyInArray_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExtractLiteralByteArray_NegativeStartPosition_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExtractLiteralByteArray_StartPositionGreaterThanInputLength_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExtractLiteralByteArray_NegativeOrZeroOutArraySize_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        // Extract a normal entry
        public void ExtractLiteralByteArray_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        // Extract an entry without the closing brace
        public void ExtractLiteralByteArray_WithoutClosingBrace_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }


        #endregion

        #region *************** Numeric Entry Extraction Tests ***************

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
