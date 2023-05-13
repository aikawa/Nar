namespace Nar {
    /// <summary>
    /// Result of parser
    /// </summary>
    /// <typeparam name="T">Type of the result</typeparam>
    public interface IParseResult<out T> {
        /// <summary>
        /// Number of consumed elements. Negative if failed to parse.
        /// </summary>
        int Consumed { get; }
        /// <summary>
        /// The result value
        /// </summary>
        T Value { get; }
        /// <summary>
        /// Whether the parser succeeds?
        /// </summary>
        bool IsOk { get; }

        /// <summary>
        /// Create a parser result represents success.
        /// </summary>
        /// <param name="consumed">number of consumed elements in the source</param>
        /// <param name="value">the result value</param>
        /// <returns>a parser result represents success</returns>
        public static IParseResult<T> Success(int consumed, T value) => new Success<T>(consumed, value);
        /// <summary>
        /// Create a parser result represents failure.
        /// </summary>
        /// <returns>a parser result represents failure</returns>
        public static IParseResult<T> Failure() => new Failure<T>();
    }// interface IParseResult

    internal struct Success<T> : IParseResult<T> {
        public int Consumed { get; init; }
        public T Value { get; init; }
        public bool IsOk => true;
        public Success(int consumed, T value) {
            Value = value;
            Consumed = consumed;
        }
    }

    internal struct Failure<T> : IParseResult<T> {
        public int Consumed => -1;
        public T Value => throw new NotSupportedException("There is no value, because failed to parse.");
        public bool IsOk => false;
    }
}// namespace Nar
