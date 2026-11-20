using System;
using System.IO;
using System.Reflection;

namespace PDFPassRecovery
{
    internal class PDFPassRecovery
    {
        static void Main(string[] args)
        {
            // Getting version of the programm
            string programVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine($"{Environment.NewLine}... PDF Password Recovery Tool (ver. {programVer}) ...{Environment.NewLine}");

            // Step 1: Checking program arguments
            string fileName = string.Empty;
            string programArgsCheck = Utils.CheckProgramArgs(args);
            if (programArgsCheck != string.Empty)
            {
                Utils.PrintError(programArgsCheck);
                Environment.Exit(1);
            }
            else
            {
                fileName = args[0];
            }

            // Step 2: Getting initial password configuration
            AppSettings appSettings = null;
            PassSettings passwordSettings = null;
            Console.Write($"Getting program and initial password configuration...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            try
            {
                appSettings = ConfigParser.GetAppSettings();
                passwordSettings = ConfigParser.GetInitPassSettings();
            }
            catch (Exception ex)
            {
                Utils.PrintError($"{Environment.NewLine}Error parsing configuration file: {ex.Message}");
                Environment.Exit(0);
            }
            Utils.PrintOK($"OK", false);


            // Step 3: Checking the PDF validity
            Console.Write($"Checking the file...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            Console.WriteLine(Environment.NewLine);

            // Step 3.1: Checking file existence
            Console.Write($"Checking file existence...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            FileInfo fi = Utils.CheckFileExists(fileName);
            if (fi is null)
            {
                Utils.PrintError($"{Environment.NewLine}The file \"{fileName}\" doesn't exist");
                Environment.Exit(1);
            }
            Utils.PrintOK($"OK", false);

            // Step 3.2: Checking file extension
            Console.Write($"Checking the file extension...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            string fileExtension = Path.GetExtension(fileName).ToUpper();
            if (fileExtension != ".PDF")
            {
                Utils.PrintError($"File extension must be \"PDF\"", false);
                Environment.Exit(11);
            }
            Utils.PrintOK($"OK", false);

            // Step 3.3: Checking file size
            if (fi.Length == 0)
            {
                Utils.PrintError($"File is empty", false);
                Environment.Exit(1);
            }
            Console.Write("File size:".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            Utils.PrintOK($"{fi.Length} (bytes)", false);

            // Step 3.4: Reading file content
            PDFFileContent pdfFileContent = null;
            Console.Write("Reading the file content...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            try
            {
                pdfFileContent = PDFParserLib.GetPDFFileContent(fileName);
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Error happened while file reading: {ex.Message}");
                Environment.Exit(1);
            }
            Utils.PrintOK("OK");

            // 
            Console.WriteLine();
            Console.WriteLine($"Checking PDF file validity...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            try
            {
                PDFParserLib.CheckPDFValidity(pdfFileContent);
            }
            catch (Exception ex)
            {
                Utils.PrintError($"{ex.Message}");
                Environment.Exit(0);
            }




            ////////////////////////////////////////////////////////////////////////////////////////////////////////




            Console.Write($"Checking PDF version...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            string pdfVersion = PDFParserLib.GetPDFVersion(pdfFileContent.AsString);
            Console.Write($"{pdfVersion}");
            Console.WriteLine();

            Console.Write($"Extracting password related data...".PadRight(Constants.PADDING_OFFSET, Constants.PADDING_CHAR));
            SecHandlerR2PasswordData passwordData = PDFParserLib.ExtractPasswordData(pdfFileContent);
            Utils.PrintOK($"Done");

            if (passwordData.GetType() == typeof(SecHandlerR2PasswordData))
            {
                RecoveryLib.RestoreEncryptedPassword(RecoveryLib.BruteForceSecHandlerR2Password, passwordData, passwordSettings);
            }

            if (passwordData.GetType() == typeof(SecHandlerR3PasswordData))
            {
                RecoveryLib.RestoreEncryptedPassword(RecoveryLib.BruteForceSecHandlerR3Password, (SecHandlerR3PasswordData)passwordData, passwordSettings);
            }

            if (passwordData.GetType() == typeof(SecHandlerR4PasswordData))
            {
                RecoveryLib.RestoreEncryptedPassword(RecoveryLib.BruteForceSecHandlerR4Password, (SecHandlerR4PasswordData)passwordData, passwordSettings);
            }

            if (passwordData.GetType() == typeof(SecHandlerR6PasswordData))
            {
                switch (passwordData.R)
                {
                    case 5:
                        RecoveryLib.RestoreEncryptedPassword(RecoveryLib.BruteForceSecHandlerR5Password, (SecHandlerR6PasswordData)passwordData, passwordSettings);
                        break;
                    case 6:
                        RecoveryLib.RestoreEncryptedPassword(RecoveryLib.BruteForceSecHandlerR6Password, (SecHandlerR6PasswordData)passwordData, passwordSettings);
                        break;
                }
            }
        }
    }
}
