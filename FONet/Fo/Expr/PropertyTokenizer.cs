namespace Fonet.Fo.Expr
{
    internal class PropertyTokenizer
    {
        protected const int TOK_EOF = 0;
        protected const int TOK_NCNAME = TOK_EOF + 1;
        protected const int TOK_MULTIPLY = TOK_NCNAME + 1;
        protected const int TOK_LPAR = TOK_MULTIPLY + 1;
        protected const int TOK_RPAR = TOK_LPAR + 1;
        protected const int TOK_LITERAL = TOK_RPAR + 1;
        protected const int TOK_NUMBER = TOK_LITERAL + 1;
        protected const int TOK_FUNCTION_LPAR = TOK_NUMBER + 1;
        protected const int TOK_PLUS = TOK_FUNCTION_LPAR + 1;
        protected const int TOK_MINUS = TOK_PLUS + 1;
        protected const int TOK_MOD = TOK_MINUS + 1;
        protected const int TOK_DIV = TOK_MOD + 1;
        protected const int TOK_NUMERIC = TOK_DIV + 1;
        protected const int TOK_COMMA = TOK_NUMERIC + 1;
        protected const int TOK_PERCENT = TOK_COMMA + 1;
        protected const int TOK_COLORSPEC = TOK_PERCENT + 1;
        protected const int TOK_FLOAT = TOK_COLORSPEC + 1;
        protected const int TOK_INTEGER = TOK_FLOAT + 1;

        protected int currentToken = TOK_EOF;
        protected string currentTokenValue = null;
        protected int currentUnitLength = 0;

        private int currentTokenStartIndex = 0;
        private readonly string expr;
        private int exprIndex = 0;
        private readonly int exprLength;
        private bool recognizeOperator = false;

        protected PropertyTokenizer(string s)
        {
            this.expr = s;
            this.exprLength = s.Length;
        }

        protected void Next()
        {
            currentTokenValue = null;
            currentTokenStartIndex = exprIndex;
            bool bSawDecimal;
            recognizeOperator = true;
            for (; ; )
            {
                if (exprIndex >= exprLength)
                {
                    currentToken = TOK_EOF;
                    return;
                }
                char c = expr[exprIndex++];
                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        currentTokenStartIndex = exprIndex;
                        break;
                    case ',':
                        recognizeOperator = false;
                        currentToken = TOK_COMMA;
                        return;
                    case '+':
                        recognizeOperator = false;
                        currentToken = TOK_PLUS;
                        return;
                    case '-':
                        recognizeOperator = false;
                        currentToken = TOK_MINUS;
                        return;
                    case '(':
                        currentToken = TOK_LPAR;
                        recognizeOperator = false;
                        return;
                    case ')':
                        currentToken = TOK_RPAR;
                        return;
                    case '"':
                    case '\'':
                        exprIndex = expr.IndexOf(c, exprIndex);
                        if (exprIndex < 0)
                        {
                            exprIndex = currentTokenStartIndex + 1;
                            throw new PropertyException("missing quote");
                        }
                        currentTokenValue = expr.Substring(
                            currentTokenStartIndex + 1,
                            exprIndex++ - (currentTokenStartIndex + 1));
                        currentToken = TOK_LITERAL;
                        return;
                    case '*':
                        currentToken = TOK_MULTIPLY;
                        return;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        ScanDigits();
                        if (exprIndex < exprLength && expr[exprIndex] == '.')
                        {
                            exprIndex++;
                            bSawDecimal = true;
                            if (exprIndex < exprLength
                                && IsDigit(expr[exprIndex]))
                            {
                                exprIndex++;
                                ScanDigits();
                            }
                        }
                        else
                        {
                            bSawDecimal = false;
                        }
                        if (exprIndex < exprLength && expr[exprIndex] == '%')
                        {
                            exprIndex++;
                            currentToken = TOK_PERCENT;
                        }
                        else
                        {
                            currentUnitLength = exprIndex;
                            ScanName();
                            currentUnitLength = exprIndex - currentUnitLength;
                            currentToken = (currentUnitLength > 0) ? TOK_NUMERIC
                                : (bSawDecimal ? TOK_FLOAT : TOK_INTEGER);
                        }
                        currentTokenValue = expr.Substring(currentTokenStartIndex,
                                                           exprIndex - currentTokenStartIndex);
                        return;

                    case '.':
                        if (exprIndex < exprLength
                            && IsDigit(expr[exprIndex]))
                        {
                            ++exprIndex;
                            ScanDigits();
                            if (exprIndex < exprLength
                                && expr[exprIndex] == '%')
                            {
                                exprIndex++;
                                currentToken = TOK_PERCENT;
                            }
                            else
                            {
                                currentUnitLength = exprIndex;
                                ScanName();
                                currentUnitLength = exprIndex - currentUnitLength;
                                currentToken = (currentUnitLength > 0) ? TOK_NUMERIC
                                    : TOK_FLOAT;
                            }
                            currentTokenValue = expr.Substring(currentTokenStartIndex,
                                                               exprIndex - currentTokenStartIndex);
                            return;
                        }
                        throw new PropertyException("illegal character '.'");

                    case '#':
                        if (exprIndex < exprLength && IsHexDigit(expr[exprIndex]))
                        {
                            ++exprIndex;
                            ScanHexDigits();
                            currentToken = TOK_COLORSPEC;
                            currentTokenValue = expr.Substring(currentTokenStartIndex,
                                                               exprIndex - currentTokenStartIndex);
                            return;
                        }
                        else
                        {
                            throw new PropertyException("illegal character '#'");
                        }

                    default:
                        --exprIndex;
                        ScanName();
                        if (exprIndex == currentTokenStartIndex)
                        {
                            throw new PropertyException("illegal character");
                        }
                        currentTokenValue = expr.Substring(
                            currentTokenStartIndex, exprIndex - currentTokenStartIndex);
                        if (currentTokenValue.Equals("mod"))
                        {
                            currentToken = TOK_MOD;
                            return;
                        }
                        else if (currentTokenValue.Equals("div"))
                        {
                            currentToken = TOK_DIV;
                            return;
                        }
                        if (FollowingParen())
                        {
                            currentToken = TOK_FUNCTION_LPAR;
                            recognizeOperator = false;
                        }
                        else
                        {
                            currentToken = TOK_NCNAME;
                            recognizeOperator = false;
                        }
                        return;
                }
            }
        }

        private void ScanName()
        {
            if (exprIndex < exprLength && IsNameStartChar(expr[exprIndex]))
            {
                while (++exprIndex < exprLength && IsNameChar(expr[exprIndex]))
                {
                    ;
                }
            }
        }

        private void ScanDigits()
        {
            while (exprIndex < exprLength && IsDigit(expr[exprIndex]))
            {
                exprIndex++;
            }
        }

        private void ScanHexDigits()
        {
            while (exprIndex < exprLength && IsHexDigit(expr[exprIndex]))
            {
                exprIndex++;
            }
        }

        private bool FollowingParen()
        {
            for (int i = exprIndex; i < exprLength; i++)
            {
                switch (expr[i])
                {
                    case '(':
                        exprIndex = i + 1;
                        return true;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        break;
                    default:
                        return false;
                }
            }
            return false;
        }

        private const string nameStartChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string nameChars = ".-0123456789";
        private const string digits = "0123456789";
        private const string hexchars = digits + "abcdefABCDEF";

        private static bool IsDigit(char c)
        {
            return digits.IndexOf(c) >= 0;
        }

        private static bool IsHexDigit(char c)
        {
            return hexchars.IndexOf(c) >= 0;
        }

        private static bool IsSpace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                    return true;
            }
            return false;
        }

        private static bool IsNameStartChar(char c)
        {
            return nameStartChars.IndexOf(c) >= 0 || c >= 0x80;
        }

        private static bool IsNameChar(char c)
        {
            return nameStartChars.IndexOf(c) >= 0 || nameChars.IndexOf(c) >= 0 || c >= 0x80;
        }
    }
}