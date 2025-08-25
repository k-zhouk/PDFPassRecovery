namespace PDFPassRecovery
{
    /// <summary>
    /// Reporting object that holds the state of the passowrd recovering progress
    /// </summary>
    public class Reporting
    {
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public DateTime EndTimeEstimate { get; set; }
        public long TotalPasswords { get; set; }
        public int PasswordsPerSec { get; set; }
        public string CurrentPassword { get; set; }
   }
}
