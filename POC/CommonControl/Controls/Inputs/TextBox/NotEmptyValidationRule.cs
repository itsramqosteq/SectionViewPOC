using System.Globalization;
using System.Windows.Controls;

namespace POC
{
    public class NotEmptyValidationRule : ValidationRule
    {
        private string _errorContent = string.Empty;
        public string errorContent
        {
            get
            {
                return _errorContent;
            }
            set
            {
                _errorContent = value;
            }
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (_errorContent == string.Empty)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return string.IsNullOrWhiteSpace((value ?? "").ToString())
                    ? new ValidationResult(false, _errorContent)
                    : ValidationResult.ValidResult;
            }
        }
    }
}