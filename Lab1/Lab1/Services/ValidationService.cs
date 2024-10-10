using System.Text.RegularExpressions;

namespace Lab1.Services
{
    public class ValidationService
    {
        public decimal ValidatePrice(string price)
        {
            //Convert string to decimal
            var validatedPrice = Convert.ToDecimal(RemoveWhitespacesUsingRegex(price));
            return validatedPrice;
        }

        public string ValidateResolution(string resolution)
        {
            //Remove all spaces from string. EX: " 1920 x 1080 " -> "1920x1080"
            return RemoveWhitespacesUsingRegex(resolution);
        }
        private string RemoveWhitespacesUsingRegex(string source)
        {
            return Regex.Replace(source, @"\s", string.Empty);
        }
    }
}