using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Calculator
{
    public class UserSettings
    {
        private const string SettingsFileName = "CalculatorSettings.xml";
        private CalculatorSettings settings;

        public UserSettings()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CalculatorSettings));
                    using (FileStream stream = new FileStream(SettingsFileName, FileMode.Open))
                    {
                        settings = (CalculatorSettings)serializer.Deserialize(stream);
                    }
                }
                else
                {
                    // Create default settings
                    settings = new CalculatorSettings
                    {
                        UseDigitGrouping = false,
                        CalculatorMode = "Standard",
                        NumberBase = 10,
                        UseOperatorPrecedence = false
                    };
                    SaveSettings();
                }
            }
            catch (Exception)
            {
                // If anything goes wrong, use default settings
                settings = new CalculatorSettings
                {
                    UseDigitGrouping = false,
                    CalculatorMode = "Standard",
                    NumberBase = 10,
                    UseOperatorPrecedence = false
                };
            }
        }

        private void SaveSettings()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CalculatorSettings));
                using (FileStream stream = new FileStream(SettingsFileName, FileMode.Create))
                {
                    serializer.Serialize(stream, settings);
                }
            }
            catch (Exception)
            {
                // Ignore errors when saving settings
            }
        }

        public bool GetUseDigitGrouping()
        {
            return settings.UseDigitGrouping;
        }

        public void SetUseDigitGrouping(bool value)
        {
            settings.UseDigitGrouping = value;
            SaveSettings();
        }

        public string GetCalculatorMode()
        {
            return settings.CalculatorMode;
        }

        public void SetCalculatorMode(string mode)
        {
            settings.CalculatorMode = mode;
            SaveSettings();
        }

        public int GetNumberBase()
        {
            return settings.NumberBase;
        }

        public void SetNumberBase(int baseValue)
        {
            settings.NumberBase = baseValue;
            SaveSettings();
        }

        public bool GetUseOperatorPrecedence()
        {
            return settings.UseOperatorPrecedence;
        }

        public void SetUseOperatorPrecedence(bool value)
        {
            settings.UseOperatorPrecedence = value;
            SaveSettings();
        }
    }

    [Serializable]
    public class CalculatorSettings
    {
        public bool UseDigitGrouping { get; set; }
        public string CalculatorMode { get; set; }
        public int NumberBase { get; set; }
        public bool UseOperatorPrecedence { get; set; }
    }
}
