using System;

namespace PDFPassRecovery
{
    /// <summary>
    /// Class for the application settings
    /// </summary>
    public class PDFPassAppSettings
    {
        public int ReportingTimeSpan { get; }

        public PDFPassAppSettings(int timeSpan)
        {
            ReportingTimeSpan = timeSpan;
        }
    }

    /// <summary>
    /// Class for the initial password settings
    /// </summary>
    public class PDFInitPassSettings
    {
        // Password to start the brute force from
        public string StartPassword { get; }

        // Total length of the password to check
        public int PasswordLength { get; }

        // Alphabet to guess the password
        public string Alphabet { get; }

        public PDFInitPassSettings(string startPassword, int passwordLength, string alphabet)
        {
            StartPassword = startPassword;
            PasswordLength = passwordLength;
            Alphabet = alphabet;
        }
    }

    /// <summary>
    /// Class for the settings in case of application restart
    /// </summary>
    public class PDFRestartState
    {
        // Date and time when the last run was stopped
        public DateTime stopDateTime { get; set; }

        // Information on how long the last run took
        public TimeSpan previousRunTime { get; set; }

        // The last checked password
        public string lastCheckedPassword { get; set; }

        // The alphabet used for password guessing
        public string Alphabet { get; set; }

        // How many password has been already checked
        public long passwordsChecked { get; set; }

        // How many passwordss left to check
        public long passwordsLeft { get; set; }

    }
}
