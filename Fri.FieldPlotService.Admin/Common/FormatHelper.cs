namespace Fri.FieldPlotService.Admin.Common
{
    /// <summary>
    /// Provides formatting features.
    /// </summary>
    public class FormatHelper
    {
        static readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        /// <summary>
        /// Converts bytes to another unit.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatSize(Int64 bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        /// <summary>
        /// Safely gets a string value from a dictionary to display.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string? GetValue(Dictionary<string, object?>? dictionary, string key)
        {
            if (dictionary != null)
            {
                if (dictionary.TryGetValue(key, out object? value))
                {
                    if (key.Contains("PERSON") || key.Contains("CREW"))
                    {
                        return MaskString(value?.ToString(), 2, 0, '*');
                    }
                    return value?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        static string? MaskString(string? inputString, int leftUnMaskLength, int rightUnMaskLength, char mask)
        {
            if(string.IsNullOrWhiteSpace(inputString))
            {
                return inputString;
            }

            if ((leftUnMaskLength + rightUnMaskLength) > inputString.Length)
                return inputString;

            return inputString.Substring(0, leftUnMaskLength) +
                new string(mask, inputString.Length - (leftUnMaskLength + rightUnMaskLength)) +
                inputString.Substring(inputString.Length - rightUnMaskLength);
        }
    }
}
