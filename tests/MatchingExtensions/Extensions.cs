using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MatchingExtensions
{
    public static class Extensions
    {
        /// <summary>Compares two sets</summary>
        /// <example>
        /// var a = new List<int[]> { new int[] { 0, 1, 2 }, new int[] { 3, 4 } };
        /// var b = new int[][] { new int[] { 0, 1, 2 }, new int[] { 3, 4 } };
        /// a.AreDeepEqual(b); // true
        /// </example>
        public static bool AreDeepEqual<T>(this IEnumerable<T> a, IEnumerable<T> b)
            => AreDeepEqual((IEnumerable)a, (IEnumerable)b);
        public static bool AreDeepEqual(this IEnumerable a, IEnumerable b)
        {
            if (a.Equals(b)) return true;

            var ea = a.GetEnumerator();
            var eb = b.GetEnumerator();
            while (true)
            {
                if (!ea.MoveNext())
                    return !eb.MoveNext();
                if (!eb.MoveNext())
                    return false;
                if ((ea.Current == eb.Current) ||
                    (ea.Current != null && ea.Current.Equals(eb.Current)))
                {
                    continue;
                }
                var ca = ea.Current as IEnumerable;
                var cb = eb.Current as IEnumerable;
                if (ca == null || cb == null || !AreDeepEqual(ca, cb)) return false;
            }
        }


        // [1, 2] == [2, 1]
        // [1, 2, 2] != [1, 1, 2]
        public static bool SequenceEqualInSomeOrder<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            var countA = (a as ICollection<T>)?.Count;
            var countB = (b as ICollection<T>)?.Count;
            if (countA != null && countB != null && countA.Value != countB.Value)
                return false;

            // add each a[i] to dictionary,
            // remove each b[i] from dictionary
            // dictionary must be empty

            var valueCountA = new Dictionary<T, int>();
            foreach (var item in a)
            {
                valueCountA.TryGetValue(item, out int count);
                valueCountA[item] = count + 1;
            }
            foreach (var item in b)
            {
                valueCountA.TryGetValue(item, out var count);
                if (count == 0)  // also if value is 0
                    return false;

                if (count == 1)
                    valueCountA.Remove(item);
                else
                    valueCountA[item] = count - 1;
            }
            return valueCountA.Count == 0;
        }

        public static bool InnerSequencesEqualInSomeOrder<T>(this IList<T[]> a, IList<T[]> b)
        {
            int size = a.Count;
            if (size != b.Count) return false;

            var isCompared = new bool[size];
            int numberOfCompared = 0;
            for (int i = 0; i < size; i++)
            {
                int j;
                for (j = 0; j < size; j++)
                {
                    if (!isCompared[j] && a[i].SequenceEqual(b[j]))
                    {
                        isCompared[j] = true;
                        numberOfCompared++;
                        break;
                    }
                }
                if (j == size) return false;
            }
            return numberOfCompared == size;
        }

        public static bool AreUnique<T>(this IEnumerable<T> items)
        {
            if (items is IList<T> list)
                return list.AreUnique(0, list.Count);

            var buff = new Dictionary<T, T>();
            foreach (var item in items)
            {
                if (buff.ContainsKey(item)) return false;
                buff.Add(item, item);
            }
            return true;
        }
        public static bool AreUnique<T>(this IList<T> list, int startIndex, int count)
        {
            var buff = new Dictionary<T, T>(count);
            for (int i = startIndex; i < count - startIndex; i++)
            {
                var item = list[i];
                if (buff.ContainsKey(item)) return false;
                buff.Add(item, item);
            }
            return true;
        }
    }
}
