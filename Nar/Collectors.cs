using System.Text;

namespace Nar {
    public class Collector<T, A, R> {
        public Func<A> Supplier { get; init; }
        public Func<A, T, A> Accumulator { get; init; }
        public Func<A, R> Finisher { get; init; }

        public Collector(Func<A> supplier, Func<A, T, A> accumulator, Func<A, R> finisher) {
            Supplier = supplier;
            Accumulator = accumulator;
            Finisher = finisher;
        }
        public static Collector<T, R, R> Of(Func<R> supplier, Func<R, T, R> accumulator) {
            return new Collector<T, R, R>(supplier, accumulator, x => x);
        }
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
        public static Collector<int, int, int> Counting() => new(() => 0, (acc, val) => acc + val, x => x);
        public static Collector<long, long, long> LongCounting() => new(() => 0L, (acc, val) => acc + val, x => x);

        public static Collector<char, StringBuilder, string> Stringify() =>
            new(() => new StringBuilder(),
            (acc, val) => {
                return acc.Append(val);
            }, x => x.ToString());
        public static Collector<string, StringBuilder, string> Joining(string delimiter = "", string prefix = "", string suffix = "") =>
            new(() => new StringBuilder(prefix),
            (acc, val) => {
                if (acc.Length != prefix.Length) {
                    acc.Append(delimiter);
                }
                return acc.Append(val);
            }, x => x.Append(suffix).ToString());

        public static Collector<T, T, T> MinBy<T>(Comparer<T> comparer) =>
            new(() => default,
            (acc, val) => {
                if (EqualityComparer<T>.Default.Equals(acc, default(T))) return val;
                return comparer.Compare(acc, val) >= 0 ? val : acc;
            }, x => x);
        public static Collector<T, T, T> MaxBy<T>(Comparer<T> comparer) =>
            new(() => default,
            (acc, val) => {
                if (EqualityComparer<T>.Default.Equals(acc, default(T))) return val;
                return comparer.Compare(acc, val) > 0 ? acc : val;
            }, x => x);

        public static Collector<T, List<T>, List<T>> ToList<T>() => ToCollection<T, List<T>>(() => new List<T>());
        public static Collector<T, ISet<T>, ISet<T>> ToSet<T>() => ToCollection<T, ISet<T>>(() => new HashSet<T>());
        public static Collector<T, C, C> ToCollection<T, C>(Func<C> supplier) where C : ICollection<T> =>
            new(supplier,
            (acc, val) => {
                acc.Add(val);
                return acc;
            }, x => x);

        public static Collector<T, Dictionary<K, V>, Dictionary<K, V>> ToDictionary<T, K, V>(Func<T, K> keyExtractor, Func<T, V> valueExtractor) where K : notnull =>
            ToDictionary(() => new Dictionary<K, V>(), keyExtractor, valueExtractor, (_, _) => { throw new NotSupportedException("Cannot merge"); });
        public static Collector<T, Dictionary<K, V>, Dictionary<K, V>> ToDictionary<T, K, V>(Func<T, K> keyExtractor, Func<T, V> valueExtractor, Func<V, V, V> merge) where K : notnull =>
            ToDictionary(() => new Dictionary<K, V>(), keyExtractor, valueExtractor, merge);
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

        public static Collector<T, U, U> Aggregate<T, U>(U identity, Func<U, T, U> op) =>
            new(() => identity,
            (acc, val) => op(acc, val),
            x => x
            );

        public static Collector<T, object?, object?> Discarding<T>() =>
            new(() => null,
            (_, _) => null,
            _ => null
            );

        public static Collector<T, A, R> Mapping<T, U, A, R>(Func<T, U> mapper, Collector<U, A, R> downStream) =>
            new(downStream.Supplier,
            (acc, val) => downStream.Accumulator(acc, mapper(val)),
            downStream.Finisher
            );

        public static Collector<T, A, RR> CollectingAndThen<T, A, R, RR>(Collector<T, A, R> downStream, Func<R, RR> finisher) =>
            new(downStream.Supplier,
            (acc, val) => downStream.Accumulator(acc, val),
            x => finisher(downStream.Finisher(x))
            );
    }// class Collectors
}// namespace Nar
