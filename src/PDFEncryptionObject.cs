namespace PDFPassRecovery
{
    public class PDFEncryptionObject
    {
        // String representation of the encryption object
        public string AsString {  get; set; }
        
        // Byte representation of the encryption object
        public byte[] AsBytes { get; set; }

        // Index of the encryption object in the PDF file
        public int Index { get; set; }

        // Size of the encryption object 
        public int Size { get; set; }

        // Flg to indicate whether the encryption object has some entries deleted
        public bool Trimmed { get; set; }

        // Name of the deleted section
        public string TrimmedSection { get; set; }
    }
}
