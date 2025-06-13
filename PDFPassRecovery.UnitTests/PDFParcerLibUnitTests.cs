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

            // Assert--> NotNull section
            Assert.NotNull(array1);
            Assert.NotNull(array2);
            Assert.NotNull(array3);

            // Assert--> Array size
            Assert.Equal(16, array1.Length);
            Assert.Equal(16, array2.Length);
            Assert.Equal(16, array2.Length);

            // Assert--> Array content
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString1);
            Assert.Equal("47-29-0f-1b-92-a0-64-44-88-5a-c3-a0-6c-1f-58-77".ToUpper(), arrayString2);
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
        public void ExractIDEntry_FileContentIsNull()
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
        public void ExractIDEntry_NoIDEntryInFileContent()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExractIDEntry_EmptyIDEntryInFileContent()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
        }

        [Fact]
        public void ExractIDEntry_IDEntryIsTooShort()
        {
            throw new NotImplementedException();

            // Arrange
            // Act
            // Assert
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
//    throw new NotImplementedException();
//    // Arrange
//    // Act
//    // Assert
//}
