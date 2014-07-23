using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
//using System.Web.Security;
using CST.Security;
using System.Text;
using CST.Localization;

namespace CST.Security
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "User", ResourceType = typeof(LocalStr))]
        public string Login { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(LocalStr))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(LocalStr))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(LocalStr))]
        [Compare("NewPassword", ErrorMessageResourceName = "CompareError", ErrorMessageResourceType = typeof(LocalStr))]
        public string ConfirmPassword { get; set; }
    }

    public class LogonChangePasswordModel
    {
        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "User", ResourceType = typeof(LocalStr))]
        public string Login { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(LocalStr))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} character long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(LocalStr))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(LocalStr))]
        [Compare("NewPassword", ErrorMessageResourceName = "CompareError", ErrorMessageResourceType = typeof(LocalStr))]
        public string ConfirmPassword { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(LocalStr))]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class LogOnModel
    {
        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Login", ResourceType = typeof(LocalStr))]
        public string Login { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(LocalStr))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(LocalStr))]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class PasswordSettings
    {
        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "MinimumLength", ResourceType = typeof(LocalStr))]
        public int MinLength { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "MaximumLength", ResourceType = typeof(LocalStr))]
        //password maximum length
        public int MaxLength { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "NumberofDigits", ResourceType = typeof(LocalStr))]
        public int NumsLength { get; set; }

        [Display(Name = "NumberofLowercase", ResourceType = typeof(LocalStr))]
        public int LowerLength { get { return lowerLength; } }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "NumberofUppercase", ResourceType = typeof(LocalStr))]
        public int UpperLength { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "NumberofSpecialCharacters", ResourceType = typeof(LocalStr))]
        public int SpecialLength { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Bar Width")]
        public int BarWidth { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Bar Color")]
        public string BarColor { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "Use Mutitple Colors")]
        public int UseMultipleColors { get; set; }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "ValidSpecialCharacters", ResourceType = typeof(LocalStr))]
        public string SpecialChars { get; set; }

        public string PasswordRegex
        {
            get
            {
                StringBuilder sbPasswordRegx = new StringBuilder(string.Empty);

                //min and max
                sbPasswordRegx.Append(@"(?=^.{" + MinLength + "," + MaxLength + "}$)");

                //numbers length
                sbPasswordRegx.Append(@"(?=(?:.*?\d){" + NumsLength + "})");

                //a-z characters
                sbPasswordRegx.Append(@"(?=.*[a-z])");

                //A-Z length
                sbPasswordRegx.Append(@"(?=(?:.*?[A-Z]){" + UpperLength + "})");

                //special characters length
                sbPasswordRegx.Append(@"(?=(?:.*?[" + SpecialChars + "]){" + SpecialLength + "})");

                //(?!.*\s) - no spaces
                //[0-9a-zA-Z!@#$%*()_+^&] -- valid characters
                sbPasswordRegx.Append(@"(?!.*\s)[0-9a-zA-Z" + SpecialChars + "]*$");

                return sbPasswordRegx.ToString();
            }
        }

        private const int lowerLength = 1;
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numChars = "0123456789";

        private class LenString
        {
            int _maxLen = 0;
            string _srcStr = "";
            string _str = "";
            Random _random = null;

            public LenString(int maxLen, string srcStr, Random rand)
            {
                _maxLen = maxLen;
                _srcStr = srcStr;
                _random = rand;
            }

            public int MaxLen { get { return _maxLen; } }

            public string Str { get { return _str; } }

            public int Remaining { get { return _maxLen - _str.Length; } }

            public bool Done { get { return Remaining <= 0; } }

            public char NextChar()
            {
                char nextChr = RandChar;
                _str += nextChr;
                return nextChr;
            }

            public int RandNum { get { return _random.Next(0, _srcStr.Length); } }

            public char RandChar { get { return _srcStr[RandNum]; } }

        };

        public string GeneratePassword(int? len = null)
        {
            int pwLen = len ?? MinLength;

            int lows = LowerLength;
            int ups = UpperLength;
            int nums = NumsLength;
            int specs = SpecialLength;
            int baseLen = lows + ups + nums + specs;

            int extra = 0;
            if (pwLen > baseLen)
            {
                extra = pwLen - baseLen;
            }

            if (extra >= 4)
            {
                int each = extra / 4;
                int left = extra - (each * 4);
                lows += each + left;
                ups += each;
                nums += each;
                specs += each;
            }
            else
            {
                if (extra > 0)
                {
                    lows += 1;
                }
                if (extra > 1)
                {
                    ups += 1;
                }
                if (extra > 2)
                {
                    nums += 1;
                }
            }

            Random random = new Random();

            List<LenString> parts = new List<LenString> { 
                new LenString(lows, lowerChars, random),
                new LenString(ups, upperChars, random),
                new LenString(nums, numChars, random),
                new LenString(specs, SpecialChars, random)
            };

            string password = "";

            LenString part;
            int idx;
            while (parts.Any())
            {
                idx = random.Next(0, parts.Count());
                part = parts[idx];

                password += part.NextChar();

                if (part.Done)
                {
                    parts.Remove(part);
                }
            }

            return password;
        }

        public static PasswordSettings GetPasswordSettings()
        {
            SecurityConfiguration securityConfig = SecurityConfiguration.SecuritySection();

            if (securityConfig.IsNotNull(s => s.PasswordPolicy))
            {
                PasswordSettings passwordSetting = new PasswordSettings
                {
                    MinLength = securityConfig.PasswordPolicy.MinLength,
                    MaxLength = securityConfig.PasswordPolicy.MaxLength,
                    NumsLength = securityConfig.PasswordPolicy.NumsLength,
                    UpperLength = securityConfig.PasswordPolicy.UpperLength,
                    SpecialLength = securityConfig.PasswordPolicy.SpecialLength,
                    BarWidth = securityConfig.PasswordPolicy.BarWidth,
                    BarColor = securityConfig.PasswordPolicy.BarColor,
                    UseMultipleColors = securityConfig.PasswordPolicy.UseMultipleColors,
                    SpecialChars = securityConfig.PasswordPolicy.SpecialChars
                };

                return passwordSetting;
            }
            else
            {
                return null;
            }
        }

    }

    public class ChangeLanguageModel : ICultureModel
    {
        public ChangeLanguageModel()
        {
            LocalizationHelper.LoadModelCulture(this);
        }

        [Required(ErrorMessageResourceName = "RequiredError", ErrorMessageResourceType = typeof(LocalStr))]
        [Display(Name = "User", ResourceType = typeof(LocalStr))]
        public string Login { get; set; }

        [Display(Name = "Language", ResourceType = typeof(LocalStr))]
        public string Culture { get; set; }

        [Display(Name = "Languages")]
        public SelectList CultureList { get; set; }

    }

}