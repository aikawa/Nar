namespace Nar {
    using static Nar.Parsers;
    public static class BinaryParsers {
        public static Parser<byte, byte> Is<S>(byte expected) => Satisfy<byte>(bt => bt == expected);
        public static Parser<byte, byte> InRange<S>(byte min, byte max) => Satisfy<byte>(bt => min <= bt && bt <= max);

        public static Parser<byte, short> Int16Satisfy<S>(Func<short, bool> test) => Int16Satisfy<S>(test, !BitConverter.IsLittleEndian);
        public static Parser<byte, short> Int16Satisfy<S>(Func<short, bool> test, bool bigEndian) {
            return (source, pos) => {
                if (source.Length <= pos + 1) {
                    return IParseResult<short>.Failure();
                } else {
                    byte b0 = source[pos];
                    byte b1 = source[pos + 1];

                    short result;
                    if (bigEndian) {
                        result = (short) ((b0 << 8) | b1);
                    } else {
                        result = (short) ((b1 << 8) | b0);
                    }
                    if (test(result)) {
                        return IParseResult<short>.Success(2, result);
                    } else {
                        return IParseResult<short>.Failure();
                    }
                }
            };
        }

        public static Parser<byte, int> Int32Satisfy<S>(Func<int, bool> test) => Int32Satisfy<S>(test, !BitConverter.IsLittleEndian);
        public static Parser<byte, int> Int32Satisfy<S>(Func<int, bool> test, bool bigEndian) {
            return (source, pos) => {
                if (source.Length <= pos + 3) {
                    return IParseResult<int>.Failure();
                } else {
                    byte b0 = source[pos];
                    byte b1 = source[pos + 1];
                    byte b2 = source[pos + 2];
                    byte b3 = source[pos + 3];

                    int result;
                    if (bigEndian) {
                        result = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
                    } else {
                        result = (b3 << 24) | (b2 << 16) | (b1 << 8) | b0;
                    }
                    if (test(result)) {
                        return IParseResult<int>.Success(4, result);
                    } else {
                        return IParseResult<int>.Failure();
                    }
                }
            };
        }

        public static Parser<byte, long> Int64Satisfy<S>(Func<long, bool> test) => Int64Satisfy<S>(test, !BitConverter.IsLittleEndian);
        public static Parser<byte, long> Int64Satisfy<S>(Func<long, bool> test, bool bigEndian) {
            return (source, pos) => {
                if (source.Length <= pos + 7) {
                    return IParseResult<long>.Failure();
                } else {
                    long b0 = source[pos];
                    long b1 = source[pos + 1];
                    long b2 = source[pos + 2];
                    long b3 = source[pos + 3];
                    long b4 = source[pos + 4];
                    long b5 = source[pos + 5];
                    long b6 = source[pos + 6];
                    long b7 = source[pos + 7];

                    long result;
                    if (bigEndian) {
                        result = (b0 << 56)
                               | (b1 << 48)
                               | (b2 << 40)
                               | (b3 << 32)
                               | (b4 << 24)
                               | (b5 << 16)
                               | (b6 << 8)
                               | b7;
                    } else {
                        result = (b7 << 56)
                               | (b6 << 48)
                               | (b5 << 40)
                               | (b4 << 32)
                               | (b3 << 24)
                               | (b2 << 16)
                               | (b1 << 8)
                               | b0;
                    }
                    if (test(result)) {
                        return IParseResult<long>.Success(8, result);
                    } else {
                        return IParseResult<long>.Failure();
                    }
                }
            };
        }
    }// class BinaryParsers
}// namespace Nar
