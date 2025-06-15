using System;
using Xunit;

namespace PDFPassRecovery.UnitTests
{
    public class ConversionFunctionsUnitTests
    {
        [Fact]
        public void ConvertHexStringToByteArray_EmptyInput_Test()
        {
            // Arrange
            string testString = string.Empty;
            string expectedMessage = $"The input hex string can't be null or empty";

            // Act

            // Assert
            ArgumentException ex = Assert.Throws<ArgumentException>(() => PDFPassRecoverLib.ConvertHexStringToByteArray(testString));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ConvertHexStringToByteArray_NullInput_Test()
        {
            // Arrange
            string testString = null;
            string expectedMessage = $"The input hex string can't be null or empty";

            // Act

            // Assert
            ArgumentException ex = Assert.Throws<ArgumentException>(() => PDFPassRecoverLib.ConvertHexStringToByteArray(testString));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ConvertHexStringToByteArray_MalformedString_Test()
        {
            // Arrange
            string testString = "47290f1b92Q06444885ac3a06c1f5877";
            string expectedMessage = $"The hex string provided contains non-hex characters";

            // Act

            // Assert
            FormatException ex = Assert.Throws<FormatException>(() => PDFPassRecoverLib.ConvertHexStringToByteArray(testString));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ConvertHexStringToByteArray_CorrectString_Test()
        {
            // Arrange
            string testString = "47290f1b92a06444885ac3a06c1f5877";

            // Act
            byte[] testArray = PDFPassRecoverLib.ConvertHexStringToByteArray(testString);

            // Assert
            string arrayAsString = BitConverter.ToString(testArray).Replace("-", string.Empty);
            Assert.Equal(testString.ToUpper(), arrayAsString);
        }

        [Fact]
        public void ConvertHexStringToByteArray_UnevenLengthInput_Test()
        {
            // Arrange
            Random rnd = new Random();

            string hexDigits = "0123456789abcdef";

            string testString = string.Empty;

            int i = 31;
            while (i-- > 0)
            {
                testString += hexDigits[rnd.Next(16)];
            }

            // Act
            byte[] testArray = PDFPassRecoverLib.ConvertHexStringToByteArray(testString);

            // Assert
            string arrayAsString = BitConverter.ToString(testArray).Replace("-", string.Empty);
            Assert.Equal((testString + '0').ToUpper(), arrayAsString);
            Assert.Equal(16, testArray.Length);
        }

        [Fact]
        public void ConvertHexStringToByteArray_RandomCorrectString_Test()
        {
            // Arrange
            Random rnd = new Random();

            string hexDigits = "0123456789abcdef";

            string testString = string.Empty;

            int i = 32;
            while (i-- > 0)
            {
                testString += hexDigits[rnd.Next(16)];
            }

            // Act
            byte[] testArray = PDFPassRecoverLib.ConvertHexStringToByteArray(testString);

            // Assert
            string arrayAsString = BitConverter.ToString(testArray).Replace("-", string.Empty);
            Assert.Equal(testString.ToUpper(), arrayAsString);
        }
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
