namespace PDFPassRecovery
{
    /// <summary>
    /// The class contains content of the PDF file both as a string and a byte array
    /// </summary>
    public class PDFFileContent
    {
        // Both setters are private in order protect the content stored from accident modification
        public string AsString { get; }
        public byte[] AsByteArray { get; }

        // Parameter constructor
        public PDFFileContent(string inputString, byte[] inputByteArray)
        {
            AsString = inputString;
            AsByteArray = inputByteArray;
        }
    }
}
