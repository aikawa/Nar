using System.Globalization;

namespace Nar {
    using static Nar.Collectors;
    using static Nar.Parsers;
    using static Nar.StringParsers;

    public static class CommonStringParsers {
        /// <summary>
        /// any character
        /// </summary>
        public static readonly Parser<char, char> Any = Satisfy<char>(_ => true);
        /// <summary>
        /// a | b | c | d | e | f | g | h | i | j | k | l | m | n | o | p | q | r | s | t | u | v | w | x | y | z
        /// </summary>
        public static readonly Parser<char, char> Lower = Satisfy<char>(ch => 'a' <= ch && ch <= 'z');
        /// <summary>
        /// A | B | C | D | E | F | G | H | I | J | K | L | M | N | O | P | Q | R | S | T | U | V | W | X | Y | Z
        /// </summary>
        public static readonly Parser<char, char> Upper = Satisfy<char>(ch => 'A' <= ch && ch <= 'Z');
        /// <summary>
        /// <see cref="Lower">&lt;lower></see> | <see cref="Upper">&lt;upper></see>
        /// </summary>
        public static readonly Parser<char, char> Alphabet = Alt(Lower, Upper);

        /// <summary>
        /// satisfy <see cref="char.IsWhiteSpace(char)"/><br/>
        /// If you need ' ', then use <see cref="Space"/>.<br/>
        /// If you need white spaces in ASCII, then use <see cref="AsciiWhiteSpace"/>.
        /// </summary>
        public static readonly Parser<char, char> WhiteSpace = Satisfy<char>(ch => char.IsWhiteSpace(ch));
        /// <summary>
        /// ' ' | '\t' | '\r' | '\n' | '\f'<br/>
        /// <see href="https://infra.spec.whatwg.org/#ascii-whitespace">ASCII white spaces in HTML</see>
        /// </summary>
        public static readonly Parser<char, char> AsciiWhiteSpace = OneOf(" \t\r\n\f");
        /// <summary>
        /// ' ' | '\t' | '\r' | '\n' | '\f' | '\v' | &lt;NBSP> | &lt;USP><br/>
        /// <see href="https://tc39.es/ecma262/#sec-white-space">White spaces in javascript</see>
        /// </summary>
        public static readonly Parser<char, char> JsWhiteSpace = Alt(OneOf(" \t\r\n\f\v\u00a0\ufefe"), Satisfy<char>(ch => char.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator));

        /// <summary>
        /// "\n"
        /// </summary>
        public static readonly Parser<char, char> LF = Is('\n');
        /// <summary>
        /// "\r" "\n"
        /// </summary>
        public static readonly Parser<char, string> CRLF = Is("\r\n");
        /// <summary>
        /// " "
        /// </summary>
        public static readonly Parser<char, char> Space = Is(' ');
        /// <summary>
        /// "\t"
        /// </summary>
        public static readonly Parser<char, char> Tab = Is('\t');

        /// <summary>
        /// + | -
        /// </summary>
        public static readonly Parser<char, char> Sign = Satisfy<char>(ch => ch == '-' || ch == '+');
        /// <summary>
        /// 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
        /// </summary>
        public static readonly Parser<char, char> Digit = Satisfy<char>(ch => '0' <= ch && ch <= '9');
        /// <summary>
        /// Digit | a | b | c | d | e | f | A | B | C | D | E | F
        /// </summary>
        public static readonly Parser<char, char> HexDigit = Digit.Alt(Satisfy<char>(ch => 'a' <= ch && ch <= 'f')).Alt(Satisfy<char>(ch => 'A' <= ch && ch <= 'F'));
        /// <summary>
        /// <see cref="Alphabet">&lt;alphabet></see> | <see cref="Digit">&lt;digit></see>
        /// </summary>
        public static readonly Parser<char, char> AlphaNum = Alt(Alphabet, Digit);

        /// <summary>
        /// <see cref="Digit">&lt;digit></see> | <see cref="Digit">&lt;digit></see> <see cref="Digits">&lt;digits></see>
        /// </summary>
        public static readonly Parser<char, string> Digits = Repeat(Digit, Stringify(), 1, -1);

        /// <summary>
        /// Convert <see cref="Digits">&lt;digits></see> into uint
        /// </summary>
        public static readonly Parser<char, uint> UnsignedInteger = Map(Digits, x => uint.Parse(x));
        /// <summary>
        /// Convert <see cref="Digits">&lt;digits></see> into ulong
        /// </summary>
        public static readonly Parser<char, ulong> UnsignedLong = Map(Digits, x => ulong.Parse(x));

        /// <summary>
        /// Convert [<see cref="Sign">&lt;sign></see>] <see cref="Digits">&lt;digits></see> into int
        /// </summary>
        public static readonly Parser<char, int> Integer = Optional(Sign, '+').And(Map(Digits, x => int.Parse(x)), (sign, num) => sign == '-' ? -num : num);
        /// <summary>
        /// Convert [<see cref="Sign">&lt;sign></see>] <see cref="Digits">&lt;digits></see> into long
        /// </summary>
        public static readonly Parser<char, long> Long = Optional(Sign, '+').And(Map(Digits, x => long.Parse(x)), (sign, num) => sign == '-' ? -num : num);

        /// <summary>
        /// <see cref="Digits">&lt;digits></see> ["." [<see cref="Digits">&lt;digits></see>]]
        /// </summary>
        public static readonly Parser<char, string> Decimal = Digits.And(Optional(Is('.').And(Optional(Digits, ""), (s0, s1) => s0 + s1), ""), (s0, s1) => s0 + s1);

        /// <summary>
        /// Convert [<see cref="Sign">&lt;sign></see>] <see cref="Decimal">&lt;decimal></see> into double
        /// </summary>
        public static readonly Parser<char, double> Double = Optional(Sign, '+').And(Map(Decimal, x => double.Parse(x)), (sign, num) => sign == '-' ? -num : num);
        /// <summary>
        /// Convert [<see cref="Sign">&lt;sign></see>] <see cref="Decimal">&lt;decimal></see> into float
        /// </summary>
        public static readonly Parser<char, float> Float = Optional(Sign, '+').And(Map(Decimal, x => float.Parse(x)), (sign, num) => sign == '-' ? -num : num);

    }// class CommonStringParsers
}// namespace Nar
