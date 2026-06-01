namespace FresherMisa2026.Application.Services;

/// <summary>
/// Kết quả validate công thức: lỗi cứng (cú pháp, mã không tồn tại) và cảnh báo mềm (mã ngừng theo dõi).
/// </summary>
public record FormulaValidationResult(bool IsValid, string? Error, IReadOnlyList<string> InactiveCodes)
{
    public static FormulaValidationResult Ok(IReadOnlyList<string> inactiveCodes) => new(true, null, inactiveCodes);
    public static FormulaValidationResult Fail(string error) => new(false, error, Array.Empty<string>());
    public static readonly FormulaValidationResult Empty = new(true, null, Array.Empty<string>());
}

/// <summary>
/// Validator cú pháp công thức lương (ValueFormula / NormFormula).
/// Hỗ trợ: SUM, IF, AND, OR, INT, TODAY và phép tính +, -, *, /.
/// Phân biệt hoa thường với mã TPL.
/// Created By: Nguyen Thiet Do (2026-05-27)
/// </summary>
public static class FormulaValidator
{
    private static readonly HashSet<string> _allowedFunctions = new(StringComparer.Ordinal)
        { "SUM", "IF", "AND", "OR", "INT", "TODAY" };

    /// <summary>
    /// Validate cú pháp công thức và tham chiếu mã TPL.
    /// Mã tồn tại nhưng ngừng theo dõi → ghi vào InactiveCodes (cảnh báo mềm), không lỗi.
    /// Mã không tồn tại / lỗi cú pháp → IsValid = false.
    /// Trả về Empty nếu formula rỗng.
    /// </summary>
    /// Created By: Nguyen Thiet Do (2026-05-27)
    public static FormulaValidationResult Validate(string? formula, HashSet<string> activeCodes, HashSet<string>? allCodes = null)
    {
        if (string.IsNullOrWhiteSpace(formula)) return FormulaValidationResult.Empty;

        try
        {
            var tokens = Tokenize(formula);
            var parser = new FormulaParser(tokens, activeCodes, allCodes);
            parser.ParseValueExpr();

            if (parser.Current.Kind != TokenKind.Eof)
                throw new FormulaException(
                    $"Ký tự không mong đợi '{parser.Current.Value}' tại vị trí {parser.Current.Pos + 1}");

            return FormulaValidationResult.Ok(parser.InactiveCodes);
        }
        catch (FormulaException ex)
        {
            return FormulaValidationResult.Fail(ex.Message);
        }
    }

    // ─── Tokens ────────────────────────────────────────────────────────────────

    private enum TokenKind
    {
        Ident, Number,
        LParen, RParen, Comma,
        CmpOp,    // >, <, >=, <=, =, <>
        ArithOp,  // +, -, *, /
        Eof
    }

    private readonly record struct Token(TokenKind Kind, string Value, int Pos);

    // ─── Tokenizer ─────────────────────────────────────────────────────────────

    private static List<Token> Tokenize(string input)
    {
        var result = new List<Token>();
        int i = 0;

        while (i < input.Length)
        {
            if (char.IsWhiteSpace(input[i])) { i++; continue; }

            // Identifier: function names and TPL codes
            if (char.IsLetter(input[i]) || input[i] == '_')
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                result.Add(new Token(TokenKind.Ident, input[start..i], start));
                continue;
            }

            // Negative number literal: only valid at start or right after '(', ',', operator
            bool atValueStart = result.Count == 0
                || result[^1].Kind is TokenKind.LParen or TokenKind.Comma
                                   or TokenKind.CmpOp or TokenKind.ArithOp;

            if (char.IsDigit(input[i])
                || (input[i] == '-' && atValueStart && i + 1 < input.Length && char.IsDigit(input[i + 1])))
            {
                int start = i;
                if (input[i] == '-') i++;
                while (i < input.Length && char.IsDigit(input[i])) i++;
                if (i < input.Length && input[i] == '.' && i + 1 < input.Length && char.IsDigit(input[i + 1]))
                {
                    i++; // '.'
                    while (i < input.Length && char.IsDigit(input[i])) i++;
                }
                result.Add(new Token(TokenKind.Number, input[start..i], start));
                continue;
            }

            // Two-char comparison operators first
            if (i + 1 < input.Length && input.Substring(i, 2) is ">=" or "<=" or "<>")
            {
                result.Add(new Token(TokenKind.CmpOp, input.Substring(i, 2), i));
                i += 2;
                continue;
            }

            switch (input[i])
            {
                case '(': result.Add(new Token(TokenKind.LParen,  "(", i++)); break;
                case ')': result.Add(new Token(TokenKind.RParen,  ")", i++)); break;
                case ',': result.Add(new Token(TokenKind.Comma,   ",", i++)); break;
                case '>': result.Add(new Token(TokenKind.CmpOp,   ">", i++)); break;
                case '<': result.Add(new Token(TokenKind.CmpOp,   "<", i++)); break;
                case '=': result.Add(new Token(TokenKind.CmpOp,   "=", i++)); break;
                case '+': result.Add(new Token(TokenKind.ArithOp, "+", i++)); break;
                case '-': result.Add(new Token(TokenKind.ArithOp, "-", i++)); break;
                case '*': result.Add(new Token(TokenKind.ArithOp, "*", i++)); break;
                case '/': result.Add(new Token(TokenKind.ArithOp, "/", i++)); break;
                default:
                    throw new FormulaException($"Ký tự không hợp lệ '{input[i]}' tại vị trí {i + 1}");
            }
        }

        result.Add(new Token(TokenKind.Eof, string.Empty, input.Length));
        return result;
    }

    // ─── Parser (recursive descent) ────────────────────────────────────────────
    //
    // Grammar:
    //   value_expr     = additive
    //   additive       = multiplicative (('+' | '-') multiplicative)*
    //   multiplicative = unary (('*' | '/') unary)*
    //   unary          = '-' unary | primary
    //   primary        = NUMBER | '(' value_expr ')' | IDENT | IDENT '(' args ')'
    //   cond_expr      = AND/OR call | value_expr CmpOp value_expr

    private sealed class FormulaParser
    {
        private readonly List<Token> _tokens;
        private readonly HashSet<string> _activeCodes;
        private readonly HashSet<string>? _allCodes;
        private readonly List<string> _inactiveCodes = new();
        private int _pos;

        public Token Current => _tokens[_pos];
        public IReadOnlyList<string> InactiveCodes => _inactiveCodes;

        public FormulaParser(List<Token> tokens, HashSet<string> activeCodes, HashSet<string>? allCodes)
        {
            _tokens = tokens;
            _activeCodes = activeCodes;
            _allCodes = allCodes;
        }

        private Token Consume() => _tokens[_pos++];

        private void Expect(TokenKind kind, string errorHint)
        {
            if (Current.Kind != kind)
            {
                var got = Current.Kind == TokenKind.Eof ? "cuối công thức" : $"'{Current.Value}'";
                throw new FormulaException($"{errorHint}, nhưng gặp {got} tại vị trí {Current.Pos + 1}");
            }
            Consume();
        }

        public void ParseValueExpr() => ParseAdditive();

        private void ParseAdditive()
        {
            ParseMultiplicative();
            while (Current.Kind == TokenKind.ArithOp && Current.Value is "+" or "-")
            {
                Consume();
                ParseMultiplicative();
            }
        }

        private void ParseMultiplicative()
        {
            ParseUnary();
            while (Current.Kind == TokenKind.ArithOp && Current.Value is "*" or "/")
            {
                Consume();
                ParseUnary();
            }
        }

        private void ParseUnary()
        {
            // Unary minus: -SUM(A,B), -LUONG_CO_BAN, ...
            if (Current.Kind == TokenKind.ArithOp && Current.Value == "-")
            {
                Consume();
                ParseUnary();
                return;
            }
            ParsePrimary();
        }

        private void ParsePrimary()
        {
            // Number literal (including negative literals tokenized as one token)
            if (Current.Kind == TokenKind.Number)
            {
                Consume();
                return;
            }

            // Parenthesized sub-expression for grouping: (A + B) * C
            if (Current.Kind == TokenKind.LParen)
            {
                Consume();
                ParseValueExpr();
                Expect(TokenKind.RParen, "Mong đợi ')' đóng ngoặc nhóm");
                return;
            }

            if (Current.Kind == TokenKind.Ident)
            {
                string name = Current.Value;
                Consume();

                if (Current.Kind == TokenKind.LParen)
                {
                    ParseFunctionAsValue(name);
                    return;
                }

                // TPL code reference — phân biệt hoa thường
                if (!_activeCodes.Contains(name))
                {
                    if (_allCodes != null && _allCodes.Contains(name))
                        _inactiveCodes.Add(name); // cảnh báo mềm — tiếp tục parse
                    else
                        throw new FormulaException($"Mã thành phần lương '{name}' không tồn tại");
                }
                return;
            }

            var found = Current.Kind == TokenKind.Eof ? "cuối công thức" : $"'{Current.Value}'";
            throw new FormulaException(
                $"Mong đợi số hoặc thành phần lương tại vị trí {Current.Pos + 1}, nhưng gặp {found}");
        }

        /// <summary>
        /// Hàm trong ngữ cảnh giá trị: SUM, IF, INT, TODAY.
        /// AND/OR chỉ được dùng trong điều kiện của IF.
        /// </summary>
        private void ParseFunctionAsValue(string name)
        {
            if (!_allowedFunctions.Contains(name))
                throw new FormulaException(
                    $"Hàm '{name}' không được hỗ trợ. Hàm hợp lệ: {string.Join(", ", _allowedFunctions)}");

            if (name is "AND" or "OR")
                throw new FormulaException(
                    $"Hàm '{name}' chỉ dùng được trong điều kiện của IF, không thể dùng làm giá trị trả về");

            Expect(TokenKind.LParen, $"Mong đợi '(' sau '{name}'");

            switch (name)
            {
                case "SUM":
                    // SUM(v, v, ...) — ít nhất 1 tham số, mỗi tham số là value_expr
                    ParseValueExpr();
                    while (Current.Kind == TokenKind.Comma) { Consume(); ParseValueExpr(); }
                    break;

                case "IF":
                    // IF(logical_test, value_if_true, value_if_false)
                    ParseCondExpr();
                    Expect(TokenKind.Comma, "IF: mong đợi ',' sau điều kiện");
                    ParseValueExpr();
                    Expect(TokenKind.Comma, "IF: mong đợi ',' sau value_if_true");
                    ParseValueExpr();
                    break;

                case "INT":
                    // INT(number)
                    ParseValueExpr();
                    break;

                case "TODAY":
                    // TODAY() — không tham số
                    break;
            }

            Expect(TokenKind.RParen, $"Mong đợi ')' để đóng '{name}'");
        }

        /// <summary>
        /// cond_expr = AND/OR(...) | value_expr CmpOp value_expr
        /// </summary>
        private void ParseCondExpr()
        {
            if (Current.Kind == TokenKind.Ident && Current.Value is "AND" or "OR")
            {
                string name = Current.Value;
                Consume();
                Expect(TokenKind.LParen, $"Mong đợi '(' sau '{name}'");

                int count = 0;
                ParseCondExpr(); count++;
                while (Current.Kind == TokenKind.Comma) { Consume(); ParseCondExpr(); count++; }
                if (count < 2)
                    throw new FormulaException($"Hàm '{name}' cần ít nhất 2 điều kiện");

                Expect(TokenKind.RParen, $"Mong đợi ')' để đóng '{name}'");
                return;
            }

            // comparison: value CmpOp value — ví dụ: A + B > 1000, TODAY() >= NGAY_SINH
            ParseValueExpr();
            if (Current.Kind != TokenKind.CmpOp)
                throw new FormulaException(
                    $"Mong đợi toán tử so sánh (>, <, >=, <=, =, <>) tại vị trí {Current.Pos + 1}");
            Consume();
            ParseValueExpr();
        }
    }

    private sealed class FormulaException(string message) : Exception(message);
}
