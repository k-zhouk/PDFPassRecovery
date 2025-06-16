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
                    PDFPassRecoverLib.PrintHelp();
                    Environment.Exit(0);
                    break;

                case 1:
                    fullFileName = args[0];

                    fi = new FileInfo(fullFileName);
                    if (!fi.Exists)
                    {
                        PDFPassRecoverLib.PrintColoredText($"The file \"{fullFileName}\" doesn't exist", ConsoleColor.Red);
                        Environment.Exit(0);
                    }
                    break;

                default:
                    PDFPassRecoverLib.PrintColoredText("Too many arguments have been provided", ConsoleColor.Red);
                    PDFPassRecoverLib.PrintHelp();
                    Environment.Exit(0);
                    break;
            }

            PDFPassConfig programConfig = null;
            PDFInitPassSettings passwordSettings = null;

            // Reading program configuration
            Console.Write($"Getting program configuration...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            try
            {
                programConfig = PDFPassConfigParser.ParseInitPassConfig();
                passwordSettings = new PDFPasswordSettings(programConfig.StartPassword, programConfig.PasswordLength, programConfig.Alphabet);
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintColoredText($"{Environment.NewLine}Error parsing configuration file: {ex.Message}", ConsoleColor.Red);
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintColoredText("OK", ConsoleColor.Green);

            /****************************************
             Main body
             ****************************************/
            Console.Write($"Checking the file...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));

            Console.WriteLine(Environment.NewLine);
            Console.Write($"Checking the file extension...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            string fileExtension = Path.GetExtension(fullFileName).ToUpper();
            if (fileExtension != ".PDF")
            {
                PDFPassRecoverLib.PrintColoredText($"The extension of the file must be \"PDF\"", ConsoleColor.Red);
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintColoredText("OK", ConsoleColor.Green);

            long fileSize = fi.Length;
            Console.WriteLine("File size:".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR) + $"{fileSize} (bytes)");

            PDFFileContent pdfFileContent = null;
            Console.Write("Reading the file content...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            try
            {
                pdfFileContent = PDFParserLib.GetPDFFileConent(fullFileName);
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintColoredText($"Error happened while file reading: {ex.Message}", ConsoleColor.Red);
                Environment.Exit(0);
            }
            PDFPassRecoverLib.PrintColoredText("OK", ConsoleColor.Green);

            Console.WriteLine();
            Console.WriteLine($"Checking PDF file validity...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            try
            {
                PDFParserLib.CheckPDFValidity(pdfFileContent);
            }
            catch (Exception ex)
            {
                PDFPassRecoverLib.PrintColoredText($"{ex.Message}", ConsoleColor.Red);
                Environment.Exit(0);
            }

            Console.Write("Checking PDF version...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
            string pdfVersion = PDFParserLib.GetPDFVersion(pdfFileContent.AsString);
            Console.Write($"{pdfVersion}");

            switch (pdfVersion)
            {
                case "":
                    PDFPassRecoverLib.PrintColoredText("The version of the PDF format is missing", ConsoleColor.Red);
                    Environment.Exit(0);
                    break;

                case "1.2":
                case "1.3":
                    Console.WriteLine();
                    Console.Write($"Extracting password related data...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
                    BasePasswordData pdf13PasswordData = PDFParserLib.ExtractPDF12PasswordData(pdfFileContent);
                    PDFPassRecoverLib.PrintColoredText("Done", ConsoleColor.Green);

                    PDFPassRecoverLib.RestorePDF12EncryptedPassword(pdf13PasswordData, passwordSettings);
                    Environment.Exit(0);
                    break;

                case "1.4":
                    Console.WriteLine();
                    Console.Write($"Extracting password related data...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
                    PDF14PasswordData pdf14PasswordData = PDFParserLib.ExtractPDF14PasswordData(pdfFileContent);
                    PDFPassRecoverLib.PrintColoredText("Done", ConsoleColor.Green);

                    PDFPassRecoverLib.RestorePDF14Password(pdf14PasswordData, passwordSettings);
                    Environment.Exit(0);
                    break;

                case "1.5":
                case "1.6":
                    Console.WriteLine();
                    Console.Write($"Extracting password related data...".PadRight(PDFPassRecoverLib.PADDING_OFFSET, PDFPassRecoverLib.PADDING_CHAR));
                    PDF15PasswordData pdf16PasswordData = PDFParserLib.ExtractPDF15PasswordData(pdfFileContent);
                    PDFPassRecoverLib.PrintColoredText("Done", ConsoleColor.Green);

                    PDFPassRecoverLib.RestorePDF15Password(pdf16PasswordData, passwordSettings);
                    Environment.Exit(0);
                    break;

                case "1.7":
                    Console.WriteLine($"{pdfVersion}");
                    PDFPassRecoverLib.PrintColoredText("The version 1.7 is not yet supported", ConsoleColor.Red);
                    Environment.Exit(0);
                    break;

                case "2.0":
                    Console.WriteLine($"{pdfVersion}");
                    PDFPassRecoverLib.PrintColoredText("The version 2.0 is not yet supported", ConsoleColor.Red);
                    Environment.Exit(0);
                    break;

                default:
                    PDFPassRecoverLib.PrintColoredText("Unknown version of the PDF format", ConsoleColor.Red);
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
