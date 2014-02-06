
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (action == null) return;

            foreach (var item in items)
                action(item);
        }
        public static void ForEach<T, T1>(this IEnumerable<T> items, Action<T, T1> action, T1 t1)
        {
            if (action == null) return;

            foreach (var item in items)
                action(item, t1);
        }

        public static IEnumerable<string> Retrieve<T>(this IEnumerable<T> list, Func<T, string> valueFunc)
        {
            var values = from l in list
                         let value = valueFunc(l)
                         where value != null
                         select value ?? value.Trim().ToLowerInvariant();
            return values;
        }

        public static T PlayDice<T>(this IEnumerable<T> list, Func<T, int> Percentage, Random randomGenerator)
        {
            if (list == null || list.Count() == 0)
                return default(T);

            if (list.Count() == 1)
                return list.First();

            T selectedEntry = default(T);
            int upperBound = list.Sum(x => Percentage(x));

            var randomValue = randomGenerator.Next(upperBound);
            var dice = 0;

            foreach (var entry in list)
            {
                dice += Percentage(entry);
                if (randomValue < dice)
                {
                    selectedEntry = entry;
                    break;
                }
            }

            return selectedEntry;
        }
    }
}
