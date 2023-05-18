namespace Nar {
    /// <summary>
    /// Extensions to allow us to use Nar.Parsers.* as a method.
    /// </summary>
    public static class ParserExtensions {
        // Basic combinators
        // Basic combinators - Concatenate/Sequence
        public static Parser<E, R> Cat<E, T, U, R>(this Parser<E, T> p0, Parser<E, U> p1, Func<T, U, R> combiner)
            => Parsers.Cat(p0, p1, combiner);
        public static Parser<E, (T, U)> Cat<E, T, U>(this Parser<E, T> p0, Parser<E, U> p1)
            => Parsers.Cat(p0, p1);
        public static Parser<E, U> DiscardLeft<E, T, U>(this Parser<E, T> p0, Parser<E, U> p1)
            => Parsers.Cat(p0, p1, (_, x) => x);
        public static Parser<E, T> DiscardRight<E, T, U>(this Parser<E, T> p0, Parser<E, U> p1)
            => Parsers.Cat(p0, p1, (x, _) => x);
        // Basic combinators - Alternation/Choice
        public static Parser<E, T> Alt<E, T>(this Parser<E, T> p0, Parser<E, T> p1)
            => Parsers.Alt(p0, p1);
        // Basic combinators - Optional
        public static Parser<E, T> Optional<E, T>(this Parser<E, T> p0, T defaultValue)
            => Parsers.Optional(p0, defaultValue);
        // Basic combinators - Repetition
        public static Parser<E, R> Repeat<E, T, A, R>(this Parser<E, T> p, Collector<T, A, R> collector, int min = 0, int max = -1)
            => Parsers.Repeat(p, collector, min, max);
        public static Parser<E, R> ZeroOrMore<E, T, A, R>(this Parser<E, T> p, Collector<T, A, R> collector)
            => Parsers.ZeroOrMore(p, collector);
        public static Parser<E, R> OneOrMore<E, T, A, R>(this Parser<E, T> p, Collector<T, A, R> collector)
            => Parsers.OneOrMore(p, collector);
        public static Parser<E, R> DelimitedBy<E, T, D, U, A, R>(this Parser<E, T> p, Parser<E, D> delimiter, Func<D, T, U> delimiterAndValue, Func<T, Collector<U, A, R>> headToCollector)
            => Parsers.Delimited(p, delimiter, delimiterAndValue, headToCollector);
        public static Parser<E, R> DelimitedBy<E, T, A, R, __>(this Parser<E, T> p, Parser<E, __> delimiter, Collector<T, A, R> collector)
            => Parsers.Delimited(p, delimiter, collector);
        // Basic combinators - Lookahead
        // Basic combinators - Lookahead - Positive
        public static Parser<E, R> And<E, T, U, R>(this Parser<E, T> p0, Parser<E, U> p1, Func<T, U, R> combiner)
            => p0.Cat(Parsers.And(p1), (t, u) => combiner(t, u));
        public static Parser<E, T> And<E, T, U>(this Parser<E, T> p0, Parser<E, U> p1)
            => p0.And(p1, (t, _) => t);
        // Basic combinators - Lookahead - Negative
        public static Parser<E, R> Not<E, T, __, U, R>(this Parser<E, T> p0, Parser<E, __> p1, U value, Func<T, U, R> combiner)
            => p0.Cat(Parsers.Not(p1, value), (t, u) => combiner(t, u));
        public static Parser<E, T> Not<E, T, __>(this Parser<E, T> p0, Parser<E, __> p1)
            => p0.Not<E, T, __, T?, T>(p1, default, (t, _) => t);
        // Functional utilities
        // Functional utilities - Map
        public static Parser<E, R> Map<E, T, R>(this Parser<E, T> p, Func<T, R> mapper) => Parsers.Map(p, mapper);
    }// class ParserExtensions
}// namespace Nar
