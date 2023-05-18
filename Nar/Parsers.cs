namespace Nar {

    /// <summary>
    /// Function signature for parser<br />
    /// I want declare Parser as returns (int consumed, T result), but
    /// C# does not allow me to use Tuple<int, T> with cavariance.<br />
    /// </summary>
    /// <typeparam name="E">Type of the elements in the source</typeparam>
    /// <typeparam name="T">Type of the result</typeparam>
    /// <param name="source">input data</param>
    /// <param name="pos">current position</param>
    /// <returns>result to output</returns>
    public delegate IParseResult<T> Parser<E, out T>(ReadOnlySpan<E> source, int pos = 0);

    /// <summary>
    /// Parser combinator utilities
    /// </summary>
    public static class Parsers {
        // Basic parsers
        /// <summary>
        /// Create a parser that consumes a element if it satisfies the test.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <param name="test">a function to test a element</param>
        /// <returns>a parser that consumes a element if it satisfies the test</returns>
        public static Parser<E, E> Satisfy<E>(Func<E, bool> test) {
            ArgumentNullException.ThrowIfNull(test, nameof(test));
            return (source, pos) => {
                if (source.Length <= pos) {
                    return IParseResult<E>.Failure();
                } else {
                    E ch = source[pos];
                    if (test(ch)) {
                        return IParseResult<E>.Success(1, ch);
                    } else {
                        return IParseResult<E>.Failure();
                    }
                }
            };
        }
        /// <summary>
        /// Create a parser that consumes any element.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <returns>a parser that consumes any element</returns>
        public static Parser<E, E> Any<E>() => Satisfy<E>(_ => true);

        // Basic combinators

        // Basic combinators - Concatenate/Sequence
        /// <summary>
        /// Create a parser that concatenates p0 and p1 combining the result with the combiner.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser p0</typeparam>
        /// <typeparam name="U">Type of the result of the parser p1</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p0">a parser</param>
        /// <param name="p1">a parser</param>
        /// <param name="combiner">a function to combine the results of p0 and p1</param>
        /// <returns>a parser that concatenates p0 and p1 combining the result with the combiner</returns>
        public static Parser<E, R> Cat<E, T, U, R>(Parser<E, T> p0, Parser<E, U> p1, Func<T, U, R> combiner) {
            ArgumentNullException.ThrowIfNull(p0, nameof(p0));
            ArgumentNullException.ThrowIfNull(p1, nameof(p1));
            ArgumentNullException.ThrowIfNull(combiner, nameof(combiner));
            return (source, pos) => {
                IParseResult<T> r0 = p0(source, pos);
                if (!r0.IsOk) {
                    return IParseResult<R>.Failure();
                }
                IParseResult<U> r1 = p1(source, pos + r0.Consumed);
                if (!r1.IsOk) {
                    return IParseResult<R>.Failure();
                }
                return IParseResult<R>.Success(r0.Consumed + r1.Consumed, combiner(r0.Value, r1.Value));
            };
        }
        /// <summary>
        /// Short hand for concatenating and combining results into a tuple.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser p0</typeparam>
        /// <typeparam name="U">Type of the result of the parser p1</typeparam>
        /// <param name="p0">a parser</param>
        /// <param name="p1">a parser</param>
        /// <returns>a parser that concatenates p0 and p1 combining the result with the combiner</returns>
        public static Parser<E, (T, U)> Cat<E, T, U>(Parser<E, T> p0, Parser<E, U> p1) => Cat(p0, p1, (t, u) => (t, u));
        // Basic combinators - Alternation/Choice
        /// <summary>
        /// Create a parser that use p0 and if failed, altenates with the result of p1.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parsers</typeparam>
        /// <param name="p0">a parser</param>
        /// <param name="p1">a parser</param>
        /// <returns>a parser that use p0 and if failed, altenates with the result of p1</returns>
        public static Parser<E, T> Alt<E, T>(Parser<E, T> p0, Parser<E, T> p1) {
            ArgumentNullException.ThrowIfNull(p0, nameof(p0));
            ArgumentNullException.ThrowIfNull(p1, nameof(p1));
            return (source, pos) => {
                IParseResult<T> r0 = p0(source, pos);
                if (r0.IsOk) {
                    return r0;
                }
                return p1(source, pos);
            };
        }
        // Basic combinators - Optional
        /// <summary>
        /// Create a parser that always succeeds whether p does or not.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="defaultValue">the result value in case p failed</param>
        /// <returns>a parser that always succeeds whether p does or not</returns>
        public static Parser<E, T> Optional<E, T>(Parser<E, T> p, T defaultValue) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            return (source, pos) => {
                IParseResult<T> r = p(source, pos);
                if (!r.IsOk) {
                    return IParseResult<T>.Success(0, defaultValue);
                }
                return r;
            };
        }
        // Basic combinators - Repetition
        /// <summary>
        /// Create a parser that applies p again and again.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <typeparam name="A">Type of the intermediate object for the collector</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="collector">collecting functions</param>
        /// <param name="min">minimum repetition count to be expected</param>
        /// <param name="max">maximum repetition count to apply. -1 means infinity.</param>
        /// <returns>a parser that applies p again and again</returns>
        /// <exception cref="ArgumentException">when min is negative, or min is greater than max</exception>
        public static Parser<E, R> Repeat<E, T, A, R>(Parser<E, T> p, Collector<T, A, R> collector, int min = 0, int max = -1) {
            // Infinity if max < 0
            if (min < 0) {
                throw new ArgumentException("min must be a non-negative number", nameof(min));
            }
            if (max >= 0 && min > max) {
                throw new ArgumentException("min must not be greater than max");
            }
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(collector, nameof(collector));
            return (source, pos) => {
                A acc = collector.Supplier();
                int read = 0;
                IParseResult<T> tmp;
                int counter = 0;
                while (true) {
                    tmp = p(source, pos + read);
                    if (!tmp.IsOk) {
                        if (counter < min) {
                            return IParseResult<R>.Failure();
                        } else {
                            return IParseResult<R>.Success(read, collector.Finisher(acc));
                        }
                    }
                    acc = collector.Accumulator(acc, tmp.Value);
                    read += tmp.Consumed;
                    ++counter;
                    if (counter == max) {
                        return IParseResult<R>.Success(read, collector.Finisher(acc));
                    }
                }
            };
        }
        /// <summary>
        /// Create a parser that applies p again and again. <br />
        /// Same to Repeat(p, collector, 0, -1).
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <typeparam name="A">Type of the intermediate object for the collector</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="collector">collecting functions</param>
        /// <returns>a parser that applies p again and again</returns>
        public static Parser<E, R> ZeroOrMore<E, T, A, R>(Parser<E, T> p, Collector<T, A, R> collector) => Repeat(p, collector, 0, -1);
        /// <summary>
        /// Create a parser that applies p again and again. <br />
        /// Same to Repeat(p, collector, 1, -1).
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <typeparam name="A">Type of the intermediate object for the collector</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="collector">collecting functions</param>
        /// <returns>a parser that applies p again and again</returns>
        public static Parser<E, R> OneOrMore<E, T, A, R>(Parser<E, T> p, Collector<T, A, R> collector) => Repeat(p, collector, 1, -1);
        // Basic combinators - Lookahead
        // Basic combinators - Lookahead - Positive
        /// <summary>
        /// Create a parser that consumes no element, even if the parser p succeeds.<br />
        /// Implementation of positive lookahead.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <returns>a parser that consumes no element, even if the parser p succeeds</returns>
        public static Parser<E, T> And<E, T>(Parser<E, T> p) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            return (source, pos) => {
                IParseResult<T> r = p(source, pos);
                if (r.IsOk) {
                    return IParseResult<T>.Success(0, r.Value);
                }
                return IParseResult<T>.Failure();
            };
        }
        // Basic combinators - Lookahead - Negative
        /// <summary>
        /// Create a parser that succeeds when the parser p failed.<br />
        /// Implementation of negative lookahead.
        /// </summary>
        /// <typeparam name="E">Type of the element in the source</typeparam>
        /// <typeparam name="T">Type of the result of created parser</typeparam>
        /// <typeparam name="__">Type of the result of the parser p. Unused type parameter.</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="value">the result value when the created parser succeeds</param>
        /// <returns>a parser that consumes no element, even if the parser p succeeds</returns>
        public static Parser<E, T> Not<E, T, __>(Parser<E, __> p, T value) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            return (source, pos) => {
                IParseResult<__> r = p(source, pos);
                if (!r.IsOk) {
                    return IParseResult<T>.Success(0, value);
                }
                return IParseResult<T>.Failure();
            };
        }

        // Structural combinators
        // Structural combinators - Delimited
        /// <summary>
        /// Create a parser for delimited values.<br />
        /// When you parse "1+2-3+4-5" with value = Digit, and delimiter = Is('+').Alt('-'),<br />
        /// then headToCollector is called with the head value '1'.<br />
        /// And delimiterAndValue is called with (+, 2), (-, 3), (+, 4), (-, 5).<br />
        /// Basically, used as Delimited(value, delimiter, demiterAndValue, collector.Accept);
        /// </summary>
        /// <typeparam name="E">Type of the elements in the source</typeparam>
        /// <typeparam name="T">Type of the parsed value</typeparam>
        /// <typeparam name="D">Type of the parsed delimiter</typeparam>
        /// <typeparam name="U">Type of the pair of a delimieter and the following value</typeparam>
        /// <typeparam name="A">Type of the intermediate object</typeparam>
        /// <typeparam name="R">Type of the final value to return</typeparam>
        /// <param name="value">a parser to parse a value in the source</param>
        /// <param name="delimiter">a parser to parse a delimiter in the source</param>
        /// <param name="delimiterAndValue">a function to combine the delimiter and the value</param>
        /// <param name="headToCollector">a function to supply the collector from the head value</param>
        /// <returns>a parser for delimited values</returns>
        public static Parser<E, R> Delimited<E, T, D, U, A, R>(Parser<E, T> value, Parser<E, D> delimiter, Func<D, T, U> delimiterAndValue, Func<T, Collector<U, A, R>> headToCollector) {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(delimiter, nameof(delimiter));
            ArgumentNullException.ThrowIfNull(delimiterAndValue, nameof(delimiterAndValue));
            ArgumentNullException.ThrowIfNull(headToCollector, nameof(headToCollector));
            return Bind(value, head => {
                if (!head.IsOk) {
                    return (source, pos) => IParseResult<R>.Failure();
                }
                Parser<E, U> tails = delimiter.Cat(value, delimiterAndValue);
                return ZeroOrMore(tails, headToCollector(head.Value));
            });
        }

        /// <summary>
        /// Create a parser for delimited values.<br />
        /// Discards the result of delimiter.<br />
        /// Sugar syntax for <seealso cref="Delimited{E, T, D, U, A, R}(Parser{E, T}, Parser{E, D}, Func{D, T, U}, Func{T, Collector{U, A, R}})">Delimited(value, delimiter, (_, val) => val, collector.Accept)</seealso>
        /// </summary>
        /// <typeparam name="E">Type of the elements in the source</typeparam>
        /// <typeparam name="T">Type of the parsed value</typeparam>
        /// <typeparam name="A">Type of the intermediate object</typeparam>
        /// <typeparam name="R">Type of the final value to return</typeparam>
        /// <typeparam name="__">Type of the delimiter. Unused type parameter.</typeparam>
        /// <param name="value">a parser to parse a value in the source</param>
        /// <param name="delimiter">a parser to parse a delimiter in the source</param>
        /// <param name="collector">collecting functions</param>
        /// <returns>a parser for delimited values</returns>
        public static Parser<E, R> Delimited<E, T, A, R, __>(Parser<E, T> value, Parser<E, __> delimiter, Collector<T, A, R> collector) => Delimited(value, delimiter, (_, val) => val, collector.Accept);

        // Structural combinators - Recursive
        /// <summary>
        /// Create a parser for recursive grammar.<br />
        /// This corresponds to Y combinator in lambda calculus.<br />
        /// e.g. For parsing blanced parenthesis:<br />
        /// Parser&lt;char, char> parens = Recursive&lt;char, char>(self => Is('(').DiscardLeft(self).DiscardLeft(Is(')')).Optional('\0'));<br />
        /// All of parens("()"), parens("(())"), parens("((()))"), parens("(((())))"), ... succeed.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="f">function to recursive parser</param>
        /// <returns>a parser for recursive grammar</returns>
        public static Parser<E, T> Recursive<E, T>(Func<Parser<E, T>, Parser<E, T>> f) {
            ArgumentNullException.ThrowIfNull(f, nameof(f));
            // Original Y combinator is defined as: f(Y(f))
            // return f(Recursive(f)); does not work. Lazy evaluation is needed.
            return Lazy(() => f(Recursive(f)));
        }

        // Parsing positions
        // Parsing positions - Empty/Epsilong
        /// <summary>
        /// Create a parser that always succeeds, without comsumption.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the created parser</typeparam>
        /// <param name="value">the result value when the created parser succeeds</param>
        /// <returns>a parser that always succeeds, without comsumption</returns>
        public static Parser<E, T> Empty<E, T>(T value) {
            return (source, pos) => {
                return IParseResult<T>.Success(0, value);
            };
        }
        // Parsing positions - Start of source
        /// <summary>
        /// Create a parser that succeeds if and only if the position is the start of the source.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the created parser</typeparam>
        /// <param name="value">the result value when the created parser succeeds</param>
        /// <returns>a parser that succeeds if and only if the position is the start of the source</returns>
        public static Parser<E, T> StartOfSource<E, T>(T value) {
            return (source, pos) => {
                if (pos == 0) {
                    return IParseResult<T>.Success(0, value);
                } else {
                    return IParseResult<T>.Failure();
                }
            };
        }
        // Parsing positions - End of source
        /// <summary>
        /// Create a parser that succeeds if and only if the position is the end of the source.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the created parser</typeparam>
        /// <param name="value">the result value when the created parser succeeds</param>
        /// <returns>a parser that succeeds if and only if the position is the end of the source</returns>
        public static Parser<E, T> EndOfSource<E, T>(T value) {
            return (source, pos) => {
                if (pos == source.Length - 1) {
                    return IParseResult<T>.Success(0, value);
                } else {
                    return IParseResult<T>.Failure();
                }
            };
        }

        // Functional utilities
        // Functional utilities - Lazy
        /// <summary>
        /// Function to lazy evaluation.<br />
        /// e.g. To create a parser that treats nesting structure:<br />
        /// Parser&lt;char, char> p_ = default;<br />
        /// Parser&lt;char, char> p = Lazy(() => p_);<br />
        /// p_ = Is('[').DiscardLeft(Is(']'));<br />
        /// Finally, p can consume "[]", "[[]]", "[[[]]]", ..., and so on
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <returns>a parser that is lazy evaluated</returns>
        public static Parser<E, T> Lazy<E, T>(Func<Parser<E, T>> p) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            return (source, pos) => p()(source, pos);
        }
        // Functional utilities - Map
        /// <summary>
        /// Create a parser whose result is mapped by the function.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="mapper">a function to map the result of the parser to new value</param>
        /// <returns>a parser whose result is mapped by the function</returns>
        public static Parser<E, R> Map<E, T, R>(Parser<E, T> p, Func<T, R> mapper) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
            return (source, pos) => {
                IParseResult<T> r = p(source, pos);
                if (!r.IsOk) {
                    return IParseResult<R>.Failure();
                }
                return IParseResult<R>.Success(r.Consumed, mapper(r.Value));
            };
        }
        // Functional utilities - Bind
        /// <summary>
        /// Create a parser that is created from the result of the parser p.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <typeparam name="R">Type of the result of the created parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="binder">a function to map the result of the parser to a new parser</param>
        /// <returns>a parser that is created from the result of the parser p</returns>
        public static Parser<E, R> Bind<E, T, R>(Parser<E, T> p, Func<IParseResult<T>, Parser<E, R>> binder) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return (source, pos) => {
                IParseResult<T> r0 = p(source, pos);
                int cons0 = r0.IsOk ? r0.Consumed : 0;

                IParseResult<R> r1 = binder(r0)(source, pos + cons0);
                if (!r1.IsOk) {
                    return IParseResult<R>.Failure();
                }
                return IParseResult<R>.Success(cons0 + r1.Consumed, r1.Value);
            };
        }

        // Hooking
        // Hooking - Before
        /// <summary>
        /// Create a parser that calls the action before parsing.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="action">an action to call</param>
        /// <returns>a parser that calls the action before parsing</returns>
        public static Parser<E, T> Before<E, T>(Parser<E, T> p, Action<int> action) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return (source, pos) => {
                action(pos);
                return p(source, pos);
            };
        }
        // Hooking - On success
        /// <summary>
        /// Create a parser that calls the action if succeeds to parse.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="action">an action to call</param>
        /// <returns>a parser that calls the action if succeeds to parse</returns>
        public static Parser<E, T> OnSuccess<E, T>(Parser<E, T> p, Action<int, IParseResult<T>> action) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return (source, pos) => {
                IParseResult<T> r = p(source, pos);
                if (!r.IsOk) {
                    return IParseResult<T>.Failure();
                }
                action(pos, r);
                return r;
            };
        }
        // Hooking - On failure
        /// <summary>
        /// Create a parser that calls the action if fails to parse.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <param name="action">an action to call</param>
        /// <returns>a parser that calls the action if fails to parse</returns>
        public static Parser<E, T> OnFailure<E, T>(Parser<E, T> p, Action<int> action) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            return (source, pos) => {
                IParseResult<T> r = p(source, pos);
                if (!r.IsOk) {
                    action(pos);
                    return IParseResult<T>.Failure();
                }
                return r;
            };
        }

        // Concrete hooking
        /// <summary>
        /// Create a parser that do packrat parsing.
        /// </summary>
        /// <typeparam name="E">Type of elements in the source</typeparam>
        /// <typeparam name="T">Type of the result of the parser</typeparam>
        /// <param name="p">a parser</param>
        /// <returns>a parser that do packrat parsing</returns>
        public static Parser<E, T> Packrat<E, T>(Parser<E, T> p) {
            ArgumentNullException.ThrowIfNull(p, nameof(p));
            Dictionary<int, IParseResult<T>> memo = new();
            return (source, pos) => {
                if (memo.TryGetValue(pos, out IParseResult<T>? old)) {
                    return old;
                }

                IParseResult<T> result = p(source, pos);
                if (!result.IsOk) {
                    return IParseResult<T>.Failure();
                }
                return memo[pos] = result;
            };
        }
    }// class Parsers
}// namespace Nar