namespace PDFPassRecovery
{
    /// <summary>
    /// Class stores the password related data extarcted from a PDF 1.2~ PDF 1.3 file
    /// </summary>
    public class BasePasswordData
    {
        // Not described in the PDF1.2 standard, but present in an actual encrypted file
        public int V { get; set; }

        // Revision of the standard security handler
        public int R { get; set; }

        // O-entry size for PDF 1.2 is 32 bytes
        public static int OEntrySize { get => 32; }
        private byte[] _OEntry = new byte[OEntrySize];
        public virtual byte[] OEntry
        {
            get => _OEntry;
            set => _OEntry = value;
        }

        // U-entry size for PDF 1.2 is 32 bytes
        public static int UEntrySize { get => 32; }
        private byte[] _UEntry = new byte[UEntrySize];
        public virtual byte[] UEntry
        {
            get => _UEntry;
            set => _UEntry = value;
        }

        // Signed P value holds a combination of bits that define access permissions
        public static int PEntrySize { get => 4; }
        private byte[] _P = new byte[PEntrySize];
        public byte[] P
        {
            get => _P;
            set => _P = value;
        }

        // ID entry size is 16 bytes
        public static int IdEntrySize { get => 16; }
        private byte[] _Id = new byte[IdEntrySize];
        public byte[] Id
        {
            get => _Id;
            set => _Id = value;
        }
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
            this.V = pdf12passwordData.V;
            this.R = pdf12passwordData.R;
            this.OEntry = pdf12passwordData.OEntry;
            this.UEntry = pdf12passwordData.UEntry;
            this.P = pdf12passwordData.P;
            this.Id = pdf12passwordData.Id;
        }

        // Length of the encryption key
        public int KeyLength { get; set; }
    }

    /// <summary>
    /// Class stores the password related data extarcted from a PDF 1.5~ PDF 1.6 file
    /// </summary>
    public class PDF15PasswordData : PDF14PasswordData
    {
        // Default constructor
        public PDF15PasswordData() { }

        // Copy constructor
        public PDF15PasswordData(PDF14PasswordData pdf14passwordData) : base(pdf14passwordData) { }

        // Option to encrypt the metadata or not
        public bool EncryptMetadata { get; set; }
    }

    /// <summary>
    /// Class stores the password related data extarcted from a PDF 1.7~ PDF 2.0 file
    /// </summary>
    public class PDF17PasswordData
    {
        public int V { get; set; }

        public int R { get; set; }

        // O-entry size for PDF 1.7 is 48 bytes
        public static int OEntrySize { get => 48; }
        private byte[] _OEntry = new byte[OEntrySize];
        public byte[] OEntry
        {
            get => _OEntry;
            set => _OEntry = value;
        }

        // U-entry size for PDF 1.7 is 48 bytes
        public static int UEntrySize { get => 48; }
        private byte[] _UEntry = new byte[UEntrySize];
        public byte[] UEntry
        {
            get => _UEntry;
            set => _UEntry = value;
        }

        public static int PEntrySize { get => 4; }
        private byte[] _P = new byte[PEntrySize];
        public byte[] P
        {
            get => _P;
            set => _P = value;
        }

        public int KeyLength { get; set; }

        public bool EncryptMetadata { get; set; }

        // UE-entry size for PDF 1.7 is 32 bytes
        public static int UEEntrySize { get => 32; }
        private byte[] _UEEntry = new byte[UEEntrySize];
        public byte[] UEEntry
        {
            get => _UEEntry;
            set => _UEEntry = value;
        }

        // ID entry size is 16 bytes
        public static int IdEntrySize { get => 16; }
        private byte[] _Id = new byte[IdEntrySize];
        public byte[] Id
        {
            get => _Id;
            set => _Id = value;
        }
    }
}
