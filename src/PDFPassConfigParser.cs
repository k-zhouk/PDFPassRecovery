using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace PDFPassRecovery
{
    public static class PDFPassConfigParser
    {
        // Field for the configruation file name
        const string configFileName = "PDFPassConfig.xml";

        // Field for the restart configruation file name
        const string restartConfigFileName = "PDFPassState.xml";

        #region *************** APPLICATION AND PASSWORD SETTINGS PARSING ***************
        /// <summary>
        /// Method returns application settings
        /// </summary>
        /// <returns>PDFPassAppSettings object with application settings</returns>
        /// <exception cref="InvalidDataException">Exception is thrown, if required elements are missing</exception>
        public static PDFPassAppSettings GetAppSettings()
        {
            XElement configContent = GetConfigFileContent();

            string timeSpanAsString = (string)configContent.Element("ReportingTimeSpan") ?? throw new InvalidDataException($"The \"ReportingTimeSpan\" section is missing in the configuration file");

            if(!int.TryParse(timeSpanAsString, out int reportingTimeSpan))
            {
                throw new InvalidDataException($"The \"ReportingTimeSpan\" setting is not a proper decimal number");
            }
            PDFPassAppSettings appSettings = new PDFPassAppSettings(reportingTimeSpan);

            return appSettings;
        }

        /// <summary>
        /// Method gets password settings for the first application run
        /// </summary>
        /// <returns>PDFInitPassSettings object with initial password, total password length and the alphabet</returns>
        /// <exception cref="InvalidDataException"></exception>
        public static PDFInitPassSettings GetInitPassSettings()
        {
            XElement configContent = GetConfigFileContent();

            XElement xmlPassword = configContent.Element("PasswordSettings") ?? throw new InvalidDataException($"The \"PasswordSettings\" section is missing in the configuration file");

            string startPassword = (string)xmlPassword.Element("StartPassword") ?? throw new InvalidDataException($"The \"StartPassword\" setting is missing in the \"PasswordSettings\" section");
            startPassword = startPassword.Trim();

            string lengthAsString = (string)xmlPassword.Element("Length") ?? throw new InvalidDataException($"The \"Length\" setting is missing in the \"PasswordSettings\" section");

            if (!int.TryParse(lengthAsString, out int passwordLength))
            {
                throw new InvalidDataException($"The \"Length\" setting is not a proper decimal number");
            }

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

            PDFInitPassSettings initPassSettings = new PDFInitPassSettings(startPassword, passwordLength, alphabet);
            return initPassSettings;
        }
        #endregion

        #region *************** RESTAR SETTINGS PARSING ***************
        public static PDFRestartState GetRestartSettings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The function returns the content of the application restart configruation file
        /// </summary>
        /// <returns></returns>
        private static XElement GetRestartConfigContent()
        {
            throw new NotImplementedException();
        }
        #endregion

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
    }
}
