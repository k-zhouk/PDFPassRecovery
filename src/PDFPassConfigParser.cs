using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace PDFPassRecovery
{
    internal static class PDFPassConfigParser
    {
        // Field for the configruation file name
        const string configFileName = "PDFPassConfig.xml";

        // Field for the restart configruation file name
        const string restartConfigFileName = "PDFPassState.xml";

        public static PDFRestartState ParseRestartSettings(XElement restartConfigContent)
        {
            throw new NotImplementedException();
        }

        public static PDFPassAppSettings ParseAppSettings(XElement configContent)
        {
            throw new NotImplementedException();
        }

        public static PDFInitPassSettings ParseInitPassConfig(XElement configContent)
        {
            try
            {
                int reportingTimeSpan = (int)xmlConfig.Element("ReportingTimeSpan");
                if (reportingTimeSpan <= 0)
                {
                    throw new InvalidDataException($"The \"ReportingTimeSpan\" should be greater than 0");
                }
            }
            catch
            {
                throw new InvalidDataException($"The \"ReportingTimeSpan\" element should be a decimal number");
            }


            XElement xmlPassword = xmlConfig.Element("PasswordSettings");

            string startPassword = (string)xmlPassword.Element("StartPassword");
            startPassword = startPassword.Trim();

            int passwordLength = (int)xmlPassword.Element("Length");
            if (startPassword.Length > passwordLength)
            {
                throw new InvalidDataException($"The start password length is greater than the password length");
            }

            // Getting all elements with names ending with the "Set" and "use" set to "yes"
            IEnumerable<XElement> charSets = from parameter in xmlPassword.Descendants()
                                             where parameter.Name.LocalName.EndsWith("Set")
                                             where parameter.Attribute("use").Value == "yes"
                                             select parameter;

            // Forming the alphabet to use for brute force
            string alphabet = string.Empty;
            foreach (var charSet in charSets)
            {
                alphabet += charSet.Value.Trim();
            }

            if (string.IsNullOrEmpty(alphabet))
            {
                throw new InvalidDataException($"The alphabet used for password geeneration is null or empty");
            }

            PDFInitPassSettings pdfRecoverConfig = new PDFInitPassSettings(startPassword, passwordLength, alphabet);
            return pdfRecoverConfig;
        }

        /// <summary>
        /// The function returns the content of the configuration file
        /// </summary>
        /// <returns>XElement with the configuration file content</returns>
        private static XElement GetConfigFileContent()
        {
            string programExecPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string pathRoot = Path.GetDirectoryName(programExecPath);
            string configFullPath = Path.Combine(pathRoot, configFileName);

            XElement xmlConfig;
            try
            {
                xmlConfig = XElement.Load(configFullPath);
            }
            catch
            {
                throw;
            }

            return xmlConfig;
        }

        /// <summary>
        /// The function returns the content of the application restart configruation file
        /// </summary>
        /// <returns></returns>
        private static XElement GetRestartConfigContent()
        {
            throw new NotImplementedException();
        }
    }
}
