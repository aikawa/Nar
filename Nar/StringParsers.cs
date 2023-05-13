namespace Nar {
    using static Nar.Parsers;
    /// <summary>
    /// Utilities for parser combinators that consume characters
    /// </summary>
    public static class StringParsers {
        /// <summary>
        /// Create a parser that succeeds if and only if current character is the expected one.
        /// </summary>
        /// <param name="expected">the expected character to appear</param>
        /// <returns>a parser that succeeds if and only if current character is the expected one</returns>
        public static Parser<char, char> Is(char expected) => Satisfy<char>(ch => ch == expected);
        /// <summary>
        /// Create a parser that succeeds if and only if current character is not the unexpected one.
        /// </summary>
        /// <param name="unexpected">the unexpected character to appear</param>
        /// <returns>a parser that succeeds if and only if current character is not the unexpected one</returns>
        public static Parser<char, char> IsNot(char unexpected) => Satisfy<char>(ch => ch != unexpected);
        /// <summary>
        /// Create a parser that succeeds if and only if current character is in the range.
        /// </summary>
        /// <param name="min">the minimum of the range</param>
        /// <param name="max">the minimum of the range</param>
        /// <returns>a parser that succeeds if and only if current character is in the range</returns>
        public static Parser<char, char> InRange(char min, char max) => Satisfy<char>(ch => min <= ch && ch <= max);
        /// <summary>
        /// Create a parser that succeeds if and only if current character is contained the characters.
        /// </summary>
        /// <param name="characters">the characters to be expected to appear</param>
        /// <returns>a parser that succeeds if and only if current character is contained the characters</returns>
        public static Parser<char, char> OneOf(string characters) => Satisfy<char>(characters.Contains);
        /// <summary>
        /// Create a parser that succeeds if and only if current character is not contained the characters.
        /// </summary>
        /// <param name="characters">the characters to be expected not to appear</param>
        /// <returns>a parser that succeeds if and only if current character is not contained the characters</returns>
        public static Parser<char, char> NotOneOf(string characters) => Satisfy<char>(ch => !characters.Contains(ch));
        /// <summary>
        /// Create a parser that consumes a code point if it passes the test.
        /// </summary>
        /// <param name="test">a function to test the current code point</param>
        /// <returns>a parser that consumes a code point if it passes the test</returns>
        public static Parser<char, string> CodePointSatisfy(Func<int, bool> test) {
            ArgumentNullException.ThrowIfNull(test, nameof(test));
            return (source, pos) => {
                if (source.Length <= pos) {
                    return IParseResult<string>.Failure();
                } else {
                    // Check whether the code point is a surrogate pair.
                    string substring = source.Slice(pos, Math.Min(2, source.Length - pos)).ToString();
                    if (char.IsSurrogatePair(substring, 0)) {
                        int codePoint = char.ConvertToUtf32(substring, 0);
                        if (test(codePoint)) {
                            return IParseResult<string>.Success(2, char.ConvertFromUtf32(codePoint));
                        } else {
                            return IParseResult<string>.Failure();
                        }
                    } else {
                        char ch = substring[0];
                        if (test(ch)) {
                            return IParseResult<string>.Success(1, ch.ToString());
                        } else {
                            return IParseResult<string>.Failure();
                        }
                    }
                }
            };
        }
        /// <summary>
        /// Create a parser that succeeds if and only if current code point is in the range.
        /// </summary>
        /// <param name="min">the minimum of the range</param>
        /// <param name="max">the minimum of the range</param>
        /// <returns>a parser that succeeds if and only if current code point is in the range</returns>
        public static Parser<char, string> CodePointInRange(int min, int max) => CodePointSatisfy(ch => min <= ch && ch <= max);
        /// <summary>
        /// Create a parser that succeeds if and only if the substring from current character matches the expected one.
        /// </summary>
        /// <param name="expected">the expected string to appear</param>
        /// <returns>a parser that succeeds if and only if the substring from current character matches the expected one</returns>
        public static Parser<char, string> Is(string expected) {
            ArgumentNullException.ThrowIfNull(expected, nameof(expected));
            return (source, pos) => {
                for (int i = 0; i < expected.Length; ++i) {
                    int cur = pos + i;
                    if (source.Length <= cur || source[cur] != expected[i]) {
                        return IParseResult<string>.Failure();
                    }
                }
                return IParseResult<string>.Success(expected.Length, expected);
            };
        }
    }// class StringParsers
}// namespace Nar
