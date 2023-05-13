using System.Text;

namespace Nar {
    /// <summary>
    /// Holder for functions to collect values into an object.
    /// </summary>
    /// <typeparam name="T">Type of the element to collect</typeparam>
    /// <typeparam name="A">Type of the intermediate object to accumulate values</typeparam>
    /// <typeparam name="R">Type of the result of collecting values</typeparam>
    public class Collector<T, A, R> {
        /// <summary>
        /// A function to supply an intermediate object to accumulate values 
        /// </summary>
        public Func<A> Supplier { get; init; }
        /// <summary>
        /// A function to accumulate the intermediate object and the value
        /// </summary>
        public Func<A, T, A> Accumulator { get; init; }
        /// <summary>
        /// A function to convert the accumulator into the result value
        /// </summary>
        public Func<A, R> Finisher { get; init; }

        internal Collector(Func<A> supplier, Func<A, T, A> accumulator, Func<A, R> finisher) {
            Supplier = supplier;
            Accumulator = accumulator;
            Finisher = finisher;
        }
        /// <summary>
        /// Create a collector that use the intermediate object to accumulate values for the result.<br />
        /// Short hand for <seealso cref="Of(Func{A}, Func{A, T, A}, Func{A, R})">Collector&gt;T, R, R>.Of(supplier, accumulator, x => x)</seealso>
        /// </summary>
        /// <param name="supplier">a function to supply an intermediate object to accumulate values </param>
        /// <param name="accumulator">a function to accumulate the intermediate object and the value</param>
        /// <returns>collector</returns>
        public static Collector<T, R, R> Of(Func<R> supplier, Func<R, T, R> accumulator) {
            return new Collector<T, R, R>(supplier, accumulator, x => x);
        }
        /// <summary>
        /// Create a collector
        /// </summary>
        /// <param name="supplier">a function to supply an intermediate object to accumulate values </param>
        /// <param name="accumulator">a function to accumulate the intermediate object and the value</param>
        /// <param name="finisher">a function to convert the accumulator into the result value</param>
        /// <returns>collector</returns>
        public static Collector<T, A, R> Of(Func<A> supplier, Func<A, T, A> accumulator, Func<A, R> finisher) {
            return new Collector<T, A, R>(supplier, accumulator, finisher);
        }

        /// <summary>
        /// Create new collector that consumes the value as initial input
        /// </summary>
        /// <param name="value">the value as initial input</param>
        /// <returns>new collector</returns>
        public Collector<T, A, R> Accept(T value) {
            return new Collector<T, A, R>(
                () => Accumulator(Supplier(), value),
                Accumulator,
                Finisher);
        }
    }// class Collector

    public static class Collectors {
        /// <summary>
        /// Create a collector that counts the values
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <returns>a collector that counting the values</returns>
        public static Collector<T, int, int> Counting<T>() => new(() => 0, (acc, _) => acc + 1, x => x);
        /// <summary>
        /// Create a collector that counts the values
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <returns>a collector that counting the values</returns>
        public static Collector<T, long, long> LongCounting<T>() => new(() => 0L, (acc, _) => acc + 1, x => x);

        /// <summary>
        /// Create a collector that aggregates characters into a string.
        /// </summary>
        /// <returns>a collector that aggregates characters into a string</returns>
        public static Collector<char, StringBuilder, string> Stringify() =>
            new(() => new StringBuilder(),
            (acc, val) => {
                return acc.Append(val);
            }, x => x.ToString());
        /// <summary>
        /// Create a collector that joins strings into a string with the delimiter, prefix, and suffix.
        /// </summary>
        /// <param name="delimiter">delimiter for values</param>
        /// <param name="prefix">prefix for the result string</param>
        /// <param name="suffix">suffix for the result string</param>
        /// <returns>a collector that joins strings into a string with the delimiter, prefix, and suffix</returns>
        public static Collector<string, StringBuilder, string> Joining(string delimiter = "", string prefix = "", string suffix = "") =>
            new(() => new StringBuilder(prefix),
            (acc, val) => {
                if (acc.Length != prefix.Length) {
                    acc.Append(delimiter);
                }
                return acc.Append(val);
            }, x => x.Append(suffix).ToString());
        /// <summary>
        /// Create a collector that selects the minimum value.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <param name="comparer">a function to compare values</param>
        /// <param name="maximumValue">maxmum value for the comparer. i.e. the identity for the min operation.</param>
        /// <returns>a collector that selects the minimum value</returns>
        public static Collector<T, T, T> MinBy<T>(Comparer<T> comparer, T maximumValue) =>
            new(() => maximumValue,
            (acc, val) => comparer.Compare(acc, val) >= 0 ? val : acc
            , x => x);
        /// <summary>
        /// Create a collector that selects the maximum value.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <param name="comparer">a function to compare values</param>
        /// <param name="minimumValue">minimum value for the comparer. i.e. the identity for the max operation.</param>
        /// <returns>a collector that selects the maximum value</returns>
        public static Collector<T, T, T> MaxBy<T>(Comparer<T> comparer, T minimumValue) =>
            new(() => minimumValue,
            (acc, val) => comparer.Compare(acc, val) > 0 ? acc : val
            , x => x);
        /// <summary>
        /// Create a collector that aggregate values into a list
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <returns>a collector that aggregate values into a list</returns>
        public static Collector<T, List<T>, List<T>> ToList<T>() => ToCollection<T, List<T>>(() => new List<T>());
        /// <summary>
        /// Create a collector that aggregate values into a set
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <returns>a collector that aggregate values into a set</returns>
        public static Collector<T, ISet<T>, ISet<T>> ToSet<T>() => ToCollection<T, ISet<T>>(() => new HashSet<T>());
        /// <summary>
        /// Create a collector that aggregate values into a collection
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <typeparam name="C">Type of the collection for the values</typeparam>
        /// <returns>a collector that aggregate values into a collection</returns>
        public static Collector<T, C, C> ToCollection<T, C>(Func<C> supplier) where C : ICollection<T> =>
            new(supplier,
            (acc, val) => {
                acc.Add(val);
                return acc;
            }, x => x);
        /// <summary>
        /// Create a collector that aggregate values into a dictionary
        /// </summary>
        /// <typeparam name="T">Type of the values to aggregate</typeparam>
        /// <typeparam name="K">Type of the keys in the dictionary</typeparam>
        /// <typeparam name="V">Type of the values in the dictionary</typeparam>
        /// <param name="keyExtractor">a function to get dictionary key from value</param>
        /// <param name="valueExtractor">a function to get dictionary value from value</param>
        /// <returns>a collector that aggregate values into a dictionary</returns>
        /// <exception cref="NotSupportedException">when keys conflict in collecting</exception>
        public static Collector<T, Dictionary<K, V>, Dictionary<K, V>> ToDictionary<T, K, V>(Func<T, K> keyExtractor, Func<T, V> valueExtractor) where K : notnull =>
            ToDictionary(() => new Dictionary<K, V>(), keyExtractor, valueExtractor, (_, _) => { throw new NotSupportedException("Cannot merge"); });
        /// <summary>
        /// Create a collector that aggregate values into a dictionary
        /// </summary>
        /// <typeparam name="T">Type of the values to aggregate</typeparam>
        /// <typeparam name="K">Type of the keys in the dictionary</typeparam>
        /// <typeparam name="V">Type of the values in the dictionary</typeparam>
        /// <param name="keyExtractor">a function to get dictionary key from value</param>
        /// <param name="valueExtractor">a function to get dictionary value from value</param>
        /// <param name="merge">a function to merge dictionary values whose dictionary key conflict</param>
        /// <returns>a collector that aggregate values into a dictionary</returns>
        public static Collector<T, Dictionary<K, V>, Dictionary<K, V>> ToDictionary<T, K, V>(Func<T, K> keyExtractor, Func<T, V> valueExtractor, Func<V, V, V> merge) where K : notnull =>
            ToDictionary(() => new Dictionary<K, V>(), keyExtractor, valueExtractor, merge);
        /// <summary>
        /// Create a collector that aggregate values into a dictionary
        /// </summary>
        /// <typeparam name="T">Type of the values to aggregate</typeparam>
        /// <typeparam name="K">Type of the keys in the dictionary</typeparam>
        /// <typeparam name="V">Type of the values in the dictionary</typeparam>
        /// <typeparam name="D">Type of the dictionary</typeparam>
        /// <param name="supplier">a function to supply a dictionary</param>
        /// <param name="keyExtractor">a function to get dictionary key from value</param>
        /// <param name="valueExtractor">a function to get dictionary value from value</param>
        /// <param name="merge">a function to merge dictionary values whose dictionary key conflict</param>
        /// <returns>a collector that aggregate values into a dictionary</returns>
        public static Collector<T, D, D> ToDictionary<T, K, V, D>(Func<D> supplier, Func<T, K> keyExtractor, Func<T, V> valueExtractor, Func<V, V, V> merge) where D : IDictionary<K, V> =>
            new(supplier,
            (acc, val) => {
                K key = keyExtractor(val);
                V value = valueExtractor(val);
                if (acc.TryGetValue(key, out V? oldVal)) {
                    acc[key] = merge(oldVal, value);
                } else {
                    acc[key] = value;
                }
                return acc;
            }, x => x
            );
        /// <summary>
        /// Create a collector that aggregate values with the function
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <typeparam name="U">Type of the result</typeparam>
        /// <param name="identity">the identity value for the operation</param>
        /// <param name="op">a function to aggregate values</param>
        /// <returns>a collector that aggregate values with the function</returns>
        public static Collector<T, U, U> Aggregate<T, U>(U identity, Func<U, T, U> op) =>
            new(() => identity,
            (acc, val) => op(acc, val),
            x => x
            );
        /// <summary>
        /// Create a collector that discards all values.<br />
        /// The result of collecting is always null.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <returns>a collector that discards all values</returns>
        public static Collector<T, object?, object?> Discarding<T>() =>
            new(() => null,
            (_, _) => null,
            _ => null
            );
        /// <summary>
        /// Create a collector that apply the down stream after mapping
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <typeparam name="U">Type of the values to aggregate</typeparam>
        /// <typeparam name="A">Type of the intermediate object to accumulate values</typeparam>
        /// <typeparam name="R">Type of the result of collecting values</typeparam>
        /// <param name="mapper">a function to map values for aggregation</param>
        /// <param name="downStream">a collector</param>
        /// <returns>a collector that apply the down stream after mapping</returns>
        public static Collector<T, A, R> Mapping<T, U, A, R>(Func<T, U> mapper, Collector<U, A, R> downStream) =>
            new(downStream.Supplier,
            (acc, val) => downStream.Accumulator(acc, mapper(val)),
            downStream.Finisher
            );
        /// <summary>
        /// Create a collector that apply the down stream and, finally map the result into the other.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <typeparam name="A">Type of the intermediate object to accumulate values</typeparam>
        /// <typeparam name="R">Type of the result of collecting values</typeparam>
        /// <typeparam name="RR">Type of the final result of collecting</typeparam>
        /// <param name="downStream">a collector</param>
        /// <param name="finisher">a function to convert the result into the other</param>
        /// <returns>a collector that apply the down stream and, finally map the result into the other</returns>
        public static Collector<T, A, RR> CollectingAndThen<T, A, R, RR>(Collector<T, A, R> downStream, Func<R, RR> finisher) =>
            new(downStream.Supplier,
            (acc, val) => downStream.Accumulator(acc, val),
            x => finisher(downStream.Finisher(x))
            );
    }// class Collectors
}// namespace Nar
