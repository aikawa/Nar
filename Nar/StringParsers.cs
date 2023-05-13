namespace Nar {
    using static Nar.Parsers;
    public static class StringParsers {
        public static Parser<char, char> Is(char expected) => Satisfy<char>(ch => ch == expected);
        public static Parser<char, char> IsNot(char unexpected) => Satisfy<char>(ch => ch != unexpected);
        public static Parser<char, char> InRange(char min, char max) => Satisfy<char>(ch => min <= ch && ch <= max);
        public static Parser<char, char> OneOf(string characters) => Satisfy<char>(characters.Contains);
        public static Parser<char, char> NotOneOf(string characters) => Satisfy<char>(ch => !characters.Contains(ch));
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
        public static Parser<char, string> CodePointInRange(int min, int max) => CodePointSatisfy(ch => min <= ch && ch <= max);
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
