using System;
using System.Collections.Generic;
using System.Linq;
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
                                                                    string pricePropertyName = PricePropertyName,
                                                                    params string[] pricesProps)
        {
            var pricesPropsLs = pricesProps.ToList();
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();

            foreach (var item in collection)
            {
                foreach (var property in properties)
                {
                    if (property.Name.Trim().ToLower() == pricePropertyName.Trim().ToLower())
                    {
                        sb.Append(currencySymbol + property.GetValue(item));
                    }
                    else if (pricesPropsLs.Contains(property.Name) && pricePropertyName == PricePropertyName)
                    {
                        sb.Append(currencySymbol + property.GetValue(item));
                        pricesPropsLs.Remove(property.Name);
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
                pricesPropsLs = pricesProps.ToList();
            }
            return sb.ToString();
        }
    }
}
