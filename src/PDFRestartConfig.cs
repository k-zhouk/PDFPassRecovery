namespace PDFPassRecovery
{
    public class PDFPasswordSettings
    {
        public string StartPassword { get; }
        public int PasswordLength { get; }
        public string Alphabet { get; }

        public PDFPasswordSettings(string startPassword, int passwordLength, string alphabet)
        {
            StartPassword = startPassword;
            PasswordLength = passwordLength;
            Alphabet = alphabet;
        }
    }

    public class PDFRestartConfig
    {
        public string StartPassword { get; }
        public int PasswordLength { get; }
        public string Alphabet { get; }
        public int ReportingTimeSpan { get; }

        public PDFRestartConfig(string startPassword, int passwordLength, string alphabet, int reportingTimeSpan)
        {
            StartPassword = startPassword;
            PasswordLength = passwordLength;
            Alphabet = alphabet;
            ReportingTimeSpan = reportingTimeSpan;
        }
    }
}
