using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;

namespace CST.Security
{
    public class SecurityConfiguration : ConfigurationSection
    {

        ConfigurationProperty _PasswordPolicy;

        public SecurityConfiguration()
        {
            _PasswordPolicy = new ConfigurationProperty("PasswordPolicy", typeof(PasswordPolicyElement), null);

            this.Properties.Add(_PasswordPolicy);
        }

        public PasswordPolicyElement PasswordPolicy
        {
            get { return this[_PasswordPolicy] as PasswordPolicyElement; }
            set { this[_PasswordPolicy] = value; }
        }

        public static SecurityConfiguration SecuritySection()
        {
            return WebConfigurationManager.GetSection("SecuritySection") as SecurityConfiguration;
        }

    }

    public class PasswordPolicyElement : ConfigurationElement
    {
        public PasswordPolicyElement()
        {
            _MinLength = new ConfigurationProperty("minLength", typeof(int), 10);
            _MaxLength = new ConfigurationProperty("maxLength", typeof(int), 20);
            _NumsLength = new ConfigurationProperty("numsLength", typeof(int), 2);
            _UpperLength = new ConfigurationProperty("upperLength", typeof(int), 1);
            _SpecialLength = new ConfigurationProperty("specialLength", typeof(int), 1);
            _BarWidth = new ConfigurationProperty("barWidth", typeof(int), 200);
            _BarColor = new ConfigurationProperty("barColor", typeof(string), "<UNDEFINED>");
            _UseMultipleColors = new ConfigurationProperty("useMultipleColors", typeof(int), 1);  // 1=Yes, 0=No
            _SpecialChars = new ConfigurationProperty("specialChars", typeof(string), "<UNDEFINED>");

            this.Properties.Add(_MinLength);
            this.Properties.Add(_MaxLength);
            this.Properties.Add(_NumsLength);
            this.Properties.Add(_UpperLength);
            this.Properties.Add(_SpecialLength);
            this.Properties.Add(_BarWidth);
            this.Properties.Add(_BarColor);
            this.Properties.Add(_UseMultipleColors);
            this.Properties.Add(_SpecialChars);
        }

        ConfigurationProperty _MinLength;
        [IntegerValidator(MinValue = 1, MaxValue = 30)]
        public int MinLength
        {
            get { return (int)this[_MinLength]; }
            set { this[_MinLength] = value; }
        }

        ConfigurationProperty _MaxLength;
        [IntegerValidator(MinValue = 5, MaxValue = 30)]
        public int MaxLength
        {
            get { return (int)this[_MaxLength]; }
            set { this[_MaxLength] = value; }
        }

        ConfigurationProperty _NumsLength;
        [IntegerValidator(MinValue = 0, MaxValue = 10)]
        public int NumsLength
        {
            get { return (int)this[_NumsLength]; }
            set { this[_NumsLength] = value; }
        }

        ConfigurationProperty _UpperLength;
        [IntegerValidator(MinValue = 0, MaxValue = 10)]
        public int UpperLength
        {
            get { return (int)this[_UpperLength]; }
            set { this[_UpperLength] = value; }
        }

        ConfigurationProperty _SpecialLength;
        [IntegerValidator(MinValue = 0, MaxValue = 10)]
        public int SpecialLength
        {
            get { return (int)this[_SpecialLength]; }
            set { this[_SpecialLength] = value; }
        }

        ConfigurationProperty _BarWidth;
        public int BarWidth
        {
            get { return (int)this[_BarWidth]; }
            set { this[_BarWidth] = value; }
        }

        ConfigurationProperty _BarColor;
        public string BarColor
        {
            get { return (string)this[_BarColor]; }
            set { this[_BarColor] = value; }
        }

        ConfigurationProperty _UseMultipleColors;
        [IntegerValidator(MinValue = 0, MaxValue = 1)]
        public int UseMultipleColors
        {
            get { return (int)this[_UseMultipleColors]; }
            set { this[_UseMultipleColors] = value; }
        }

        ConfigurationProperty _SpecialChars;
        public string SpecialChars
        {
            get { return (string)this[_SpecialChars]; }
            set { this[_SpecialChars] = value; }
        }

    }
}