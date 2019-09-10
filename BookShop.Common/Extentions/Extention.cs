using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Common.Extentions
{
    public static class Extention
    {
        private const string PricePropertyName = "price";
        
        public static string JoinWithNewLine<T>(this IEnumerable<T> collection)
        {
            return string.Join($"{Environment.NewLine}", collection);
        }

        public static string BuildStringFromDTO<T>(this IEnumerable<T> collection,
                                                                    string separator = " ",
                                                                    string currencySymbol = "",
                                                                    string pricePropertyName = PricePropertyName)
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();

            foreach (var item in collection)
            {
                foreach (var property in properties)
                {
                    if (property.Name.Trim().ToLower() == pricePropertyName)
                    {
                        sb.Append(currencySymbol + property.GetValue(item));
                    }
                    else
                    {
                        sb.Append(property.GetValue(item));
                    }
                    sb.Append(separator);
                }
                int separatorLength = separator.Length;
                sb.Remove(sb.Length - separatorLength, separatorLength);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
