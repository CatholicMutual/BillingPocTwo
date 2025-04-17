using System.Collections.Generic;
using System;
using System.Linq;

namespace BillingPocTwo.WebUI.Client.Services
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> OrderByDynamic<T>(this IEnumerable<T> source, string propertyName, bool ascending)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null) throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");

            return ascending
                ? source.OrderBy(x => property.GetValue(x, null))
                : source.OrderByDescending(x => property.GetValue(x, null));
        }
    }
}
