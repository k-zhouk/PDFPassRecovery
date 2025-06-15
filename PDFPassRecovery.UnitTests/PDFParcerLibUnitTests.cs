using System;
using System.IO;

using PDFPassRecovery;
using Xunit;

namespace PDFParcerLib.UnitTests
{
    public class PDFParcerLibUnitTests
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
        public void ExractIDEntry_IDEntryIsTooShort()
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

        #region *************** Bool Entry Extraction Test ***************
        #endregion

        #region *************** Byte Array Entry Extraction Test ***************
        [Fact]
        public void ProcessMalformedByteArrayAsStringEntry_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ProcessMalformedByteArrayAsHexadecimalStringEntry_Test()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }
        #endregion

        #region *************** Numeric Entry Extraction Test ***************

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
