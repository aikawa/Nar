namespace Nar {
    public interface IParseResult<out T> {
        int Consumed { get; }
        T Value { get; }
        bool IsOk { get; }

        public static IParseResult<T> Success(int consumed, T value) => new Success<T>(consumed, value);
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
