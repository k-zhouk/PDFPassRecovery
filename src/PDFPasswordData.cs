﻿namespace PDFPassRecovery
{
    /// <summary>
    /// Class stores the password related data extarcted from a PDF 1.2 file
    /// </summary>
    public class BasePasswordData
    {
        // Not described in the PDF1.2 standard, but present in an encrypted file
        public int V { get; set; }

        // Revision of the standard security handler
        public int R { get; set; }

        // O-entry size is 32 bytes
        public byte[] OEntry { get; set; }

        // U-entry size is 32 bytes
        public byte[] UEntry { get; set; }

        // Signed P value holds a combination of bits that define access permissions
        public byte[] P { get; set; }

        // ID entry size is 16 bytes
        public byte[] IdValue { get; set; }
    }

    /// <summary>
    /// Class stores the password related data extarcted from a PDF 1.4 file
    /// </summary>
    public class PDF14PasswordData : BasePasswordData
    {
        // Default constructor
        public PDF14PasswordData() { }

        // Copy constructor
        public PDF14PasswordData(BasePasswordData pdf12passwordData)
        {
            V = pdf12passwordData.V;
            R = pdf12passwordData.R;
            OEntry = pdf12passwordData.OEntry;
            UEntry = pdf12passwordData.UEntry;
            P = pdf12passwordData.P;
            IdValue = pdf12passwordData.IdValue;
        }

        // Length of the encryption key
        public int KeyLength { get; set; }
    }

    public class PDF15PasswordData : PDF14PasswordData
    {
        // Default constructor
        public PDF15PasswordData() { }

        //TODO: This need to be fixed--> inheritance should be from BasePasswordData, as the key length is extracted differently in PDF 1.4 and PDF 1.5
        // Copy constructor
        public PDF15PasswordData(PDF14PasswordData pdf14passwordData) : base(pdf14passwordData)
        {
            KeyLength = pdf14passwordData.KeyLength;
        }

        // Option to encrypt the metadata or not
        public bool EncryptMetadata { get; set; }
    }
}
