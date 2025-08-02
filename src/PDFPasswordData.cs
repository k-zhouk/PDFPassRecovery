namespace PDFPassRecovery
{
    /// <summary>
    /// Class stores the password related data extracted from a PDF 1.2~ PDF 1.3 file
    /// </summary>
    public class SecHandlerR2PasswordData
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
    /// Class stores the password related data extracted from a PDF 1.4 file
    /// </summary>
    public class SecHandlerR3PasswordData : SecHandlerR2PasswordData
    {
        // Default constructor
        public SecHandlerR3PasswordData() { }

        // Copy constructor
        public SecHandlerR3PasswordData(SecHandlerR2PasswordData secHandlerR2PassData)
        {
            this.V = secHandlerR2PassData.V;
            this.R = secHandlerR2PassData.R;
            this.OEntry = secHandlerR2PassData.OEntry;
            this.UEntry = secHandlerR2PassData.UEntry;
            this.P = secHandlerR2PassData.P;
            this.Id = secHandlerR2PassData.Id;
        }

        // Length of the encryption key
        public int KeyLength { get; set; }
    }

    /// <summary>
    /// Class stores the password related data extracted from a PDF 1.5~ PDF 1.6 file
    /// </summary>
    public class SecHandlerR4PasswordData : SecHandlerR3PasswordData
    {
        // Default constructor
        public SecHandlerR4PasswordData() { }

        // Copy constructor
        public SecHandlerR4PasswordData(SecHandlerR3PasswordData secHandlerR3PassData) : base(secHandlerR3PassData) { }

        // Option to encrypt the metadata or not
        public bool EncryptMetadata { get; set; }
    }

    /// <summary>
    /// Class stores the password related data extracted from a PDF 1.7~ PDF 2.0 file
    /// </summary>
    public class SecHandlerR6PasswordData : SecHandlerR4PasswordData
    {
        // Default constructor
        public SecHandlerR6PasswordData() { }

        // Copy constructor
        public SecHandlerR6PasswordData(SecHandlerR4PasswordData secHandlerR4PassData) : base(secHandlerR4PassData) { }

        // O-entry size for PDF 1.7 is 48 bytes
        public static new int OEntrySize { get => 48; }
        private byte[] _OEntry = new byte[OEntrySize];
        public override byte[] OEntry
        {
            get => _OEntry;
            set => _OEntry = value;
        }

        // U-entry size for PDF 1.7 is 48 bytes
        public static new int UEntrySize { get => 48; }
        private byte[] _UEntry = new byte[UEntrySize];
        public override byte[] UEntry
        {
            get => _UEntry;
            set => _UEntry = value;
        }

        // UE-entry size for PDF 1.7 is 32 bytes
        public static int UEEntrySize { get => 32; }
        private byte[] _UEEntry = new byte[UEEntrySize];
        public byte[] UEEntry
        {
            get => _UEEntry;
            set => _UEEntry = value;
        }

        public static int SaltSize { get => 8; }

        private byte[] _ValidationSalt = new byte[SaltSize];
        public byte[] ValidationSalt
        {
            get => _ValidationSalt;
            set => _ValidationSalt = value;
        }
    }
}
