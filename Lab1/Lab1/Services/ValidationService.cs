using System.Text.RegularExpressions;

namespace Lab1.Services
{
    public class ValidationService
    {
        public int ValidatePrice(string price)
        {
            //Convert string to int
            var validatedPrice = Convert.ToInt32(RemoveWhitespacesUsingRegex(price));
            return validatedPrice;
        }

        public string ValidateResolution(string resolution)
        {
            //Remove all spaces from string. EX: " 1920 x 1080 " -> "1920x1080"
            return RemoveWhitespacesUsingRegex(resolution);
        }
        public static string RemoveWhitespacesUsingRegex(string source)
        {
            return Regex.Replace(source, @"\s", string.Empty);
        }
    }
}