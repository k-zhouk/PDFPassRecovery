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
            string programVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine($"{Environment.NewLine}... PDF Password Recovery Tool (ver. {programVersion}) ...{Environment.NewLine}");

            FileInfo fi = null;
            string fullFileName = string.Empty;

            switch (args.Length)
            {
                case 0:
                    PDFPassRecoverLib.PrintColoredText($"No arguments have been provided!", ConsoleColor.Red);
                    PDFPassRecoverLib.PrintHelp();
                    Environment.Exit(0);
                    break;

                case 1:
                    string parameter = args[0];

                    // Display help
                    if (parameter == "-h")
                    {
                        PDFPassRecoverLib.PrintHelp();
                        Environment.Exit(0);
                    }

                    // Otherwise treat the 1st argument as a path to the PDF file
                    fullFileName = parameter;
                    fi = new FileInfo(parameter);
                    if (!fi.Exists)
                    {
                        PDFPassRecoverLib.PrintColoredText($"{Environment.NewLine}The file \"{parameter}\" doesn't exist", ConsoleColor.Red);
                        Environment.Exit(0);
                    }
                    break;

                default:
                    PDFPassRecoverLib.PrintFatal($"Too many arguments have been provided!");
                    PDFPassRecoverLib.PrintHelp();
                    Environment.Exit(0);
                    break;
            }

            PDFPassAppSettings appSettings = null;
            PDFInitPassSettings passwordSettings = null;

            // Getting initial password configuration
            Console.Write($"Getting program and initial password configuration...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            try
            {
                appSettings = PDFPassConfigParser.GetAppSettings();
                passwordSettings = PDFPassConfigParser.GetInitPassSettings();
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintFatal($"{Environment.NewLine}Error parsing configuration file: {ex.Message}");
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintInfo($"OK");

            /**************************************** MAIN BODY ****************************************/
            Console.Write($"Checking the file...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));

            Console.WriteLine(Environment.NewLine);
            Console.Write($"Checking the file extension...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            string fileExtension = Path.GetExtension(fullFileName).ToUpper();
            if (fileExtension != ".PDF")
            {
                PDFPassRecoverLib.PrintFatal($"File extension must be \"PDF\"");
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintInfo($"OK");

            long fileSize = fi.Length;
            Console.Write("File size:".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            PDFPassRecoverLib.PrintInfo($"{fileSize} (bytes)");

            PDFFileContent pdfFileContent = null;
            Console.Write("Reading the file content...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            try
            {
                pdfFileContent = PDFParserLib.GetPDFFileContent(fullFileName);
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintFatal($"Error happened while file reading: {ex.Message}");
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintInfo("OK");

            Console.WriteLine();
            Console.WriteLine($"Checking PDF file validity...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            try
            {
                PDFParserLib.CheckPDFValidity(pdfFileContent);
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintFatal($"{ex.Message}");
                Environment.Exit(0);
            }

            Console.Write($"Checking PDF version...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            string pdfVersion = PDFParserLib.GetPDFVersion(pdfFileContent.AsString);
            Console.Write($"{pdfVersion}");
            Console.WriteLine();

            Console.Write($"Extracting password related data...".PadRight(PDFPassConstants.PADDING_OFFSET, PDFPassConstants.PADDING_CHAR));
            SecHandlerR2PasswordData passwordData = PDFParserLib.ExtractPDFPassData(pdfFileContent);
            PDFPassRecoverLib.PrintInfo($"Done");

            if (passwordData.GetType() == typeof(SecHandlerR2PasswordData))
            {
                PDFPassRecoverLib.RestoreEncryptedPassword(PDFPassRecoverLib.BruteForceSecHandlerR2Password, passwordData, passwordSettings);
            }
            
            if (passwordData.GetType() == typeof(SecHandlerR3PasswordData))
            {
                PDFPassRecoverLib.RestoreEncryptedPassword(PDFPassRecoverLib.BruteForceSecHandlerR3Password, (SecHandlerR3PasswordData)passwordData, passwordSettings);
            }
            
            if (passwordData.GetType() == typeof(SecHandlerR4PasswordData))
            {
                PDFPassRecoverLib.RestoreEncryptedPassword(PDFPassRecoverLib.BruteForceSecHandlerR4Password, (SecHandlerR4PasswordData)passwordData, passwordSettings);
            }

            if (passwordData.GetType() == typeof(SecHandlerR6PasswordData))
            {
                switch (passwordData.R)
                {
                    case 5:
                        PDFPassRecoverLib.RestoreEncryptedPassword(PDFPassRecoverLib.BruteForceSecHandlerR5Password, (SecHandlerR6PasswordData)passwordData, passwordSettings);
                        break;
                    case 6:
                        PDFPassRecoverLib.RestoreEncryptedPassword(PDFPassRecoverLib.BruteForceSecHandlerR6Password, (SecHandlerR6PasswordData)passwordData, passwordSettings);
                        break;
                }
            }
        }
    }
}
