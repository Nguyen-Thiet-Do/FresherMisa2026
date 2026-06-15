using FresherMisa2026.Entities;

namespace FresherMisa2026.Application.Services;

/// <summary>
/// Kết quả validate công thức:
/// - lỗi cú pháp → IsValid=false, Error có nội dung
/// - mã không tồn tại → IsValid=false, MissingCodes liệt kê toàn bộ vị trí (gom đủ, không fail-fast)
/// - mã ngừng theo dõi → IsValid=true (cảnh báo mềm), InactiveCodes có nội dung
/// </summary>
public record FormulaValidationResult(
    bool IsValid,
    string? Error,
    IReadOnlyList<string> InactiveCodes,
    IReadOnlyList<MissingCodeInfo> MissingCodes)
{
    public static readonly FormulaValidationResult Empty =
        new(true, null, Array.Empty<string>(), Array.Empty<MissingCodeInfo>());
}

/// <summary>
/// Validator cú pháp công thức lương (ValueFormula / NormFormula).
/// Hỗ trợ: SUM, IF, AND, OR, INT, TODAY và phép tính +, -, *, /.
/// Phân biệt hoa thường với mã TPL.
/// Created By: ntdo (2026-06-08)
/// </summary>
public static class FormulaValidator
{
    /// <summary>Danh sách hàm dựng sẵn được phép dùng trong công thức.</summary>
    public static readonly IReadOnlySet<string> AllowedFunctions =
        new HashSet<string>(StringComparer.Ordinal) { "SUM", "IF", "AND", "OR", "INT", "TODAY" };

    private static readonly HashSet<string> _allowedFunctions = (HashSet<string>)AllowedFunctions;

    /// <summary>
    /// Trích xuất tất cả mã TPL từ công thức 
    /// bỏ qua tên hàm dựng sẵn và số. Dùng để pre-filter trước khi query DB.
    /// Công thức lỗi cú pháp sẽ bị bỏ qua (validation xử lý riêng).
    /// </summary>
    /// Created By: ntdo (2026-06-08)
    public static HashSet<string> ExtractIdentifiers(params string?[] formulas)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var formula in formulas)
        {
            if (string.IsNullOrWhiteSpace(formula)) continue;
            var input = formula.StartsWith('=') ? formula[1..].TrimStart() : formula;
            List<Token> tokens;
            try { tokens = Tokenize(input); }
            catch (FormulaException) { continue; }
            foreach (var token in tokens)
                if (token.Kind == TokenKind.Ident && !_allowedFunctions.Contains(token.Value))
                    result.Add(token.Value);
        }
        return result;
    }

    /// <summary>
    /// Validate cú pháp công thức và tham chiếu mã TPL.
    /// Mã tồn tại nhưng ngừng theo dõi → ghi vào InactiveCodes (cảnh báo mềm), không lỗi.
    /// Mã không tồn tại → ghi vào MissingCodes (gom đủ, không fail-fast) + IsValid=false.
    /// Lỗi cú pháp → IsValid=false, Error có nội dung; MissingCodes đã thu thập tới điểm fail vẫn được giữ lại.
    /// Trả về Empty nếu formula rỗng.
    /// Position trong MissingCodes là vị trí tuyệt đối trong chuỗi formula gốc (gồm cả '=').
    /// </summary>
    /// Created By: ntdo (2026-06-08)
    public static FormulaValidationResult Validate(string? formula, HashSet<string> activeCodes, HashSet<string>? allCodes = null)
    {
        if (string.IsNullOrWhiteSpace(formula)) return FormulaValidationResult.Empty;

        int offset = 0;
        var parseInput = formula;
        if (formula.StartsWith('='))
        {
            parseInput = formula[1..].TrimStart();
            offset = formula.Length - parseInput.Length;
        }

        List<Token> tokens;
        try
        {
            tokens = Tokenize(parseInput);
        }
        catch (FormulaException ex)
        {
            return new FormulaValidationResult(false, ex.Message,
                Array.Empty<string>(), Array.Empty<MissingCodeInfo>());
        }

        var parser = new FormulaParser(tokens, activeCodes, allCodes, offset);
        string? error = null;
        try
        {
            parser.ParseValueExpr();
            if (parser.Current.Kind != TokenKind.Eof)
                throw new FormulaException(
                    $"Ký tự không mong đợi '{parser.Current.Value}' tại vị trí {parser.Current.Pos + 1}");
        }
        catch (FormulaException ex)
        {
            error = ex.Message;
        }

        bool isValid = error == null && parser.MissingCodes.Count == 0;
        return new FormulaValidationResult(isValid, error, parser.InactiveCodes, parser.MissingCodes);
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

    /// Created By: ntdo (2026-06-09)
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
                bool hasLeadingMinus = input[i] == '-';
                if (hasLeadingMinus) i++;
                while (i < input.Length && char.IsDigit(input[i])) i++;

                // Sau dãy chữ số: nếu là chữ cái hoặc '_' thì đây là mã TPL bắt đầu bằng số (ví dụ "12A5"),
                // không phải số. Với leading '-': tách thành ArithOp '-' + Ident để unary minus của parser xử lý.
                if (i < input.Length && (char.IsLetter(input[i]) || input[i] == '_'))
                {
                    if (hasLeadingMinus)
                    {
                        result.Add(new Token(TokenKind.ArithOp, "-", start));
                        int identStart = start + 1;
                        while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                        result.Add(new Token(TokenKind.Ident, input[identStart..i], identStart));
                    }
                    else
                    {
                        while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                        result.Add(new Token(TokenKind.Ident, input[start..i], start));
                    }
                    continue;
                }

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

    // ─── Parser (đệ quy xuống) ─────────────────────────────────────────────────
    //
    // Ngữ pháp:
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
        private readonly int _offset;
        private readonly List<string> _inactiveCodes = new();
        private readonly List<MissingCodeInfo> _missingCodes = new();
        private int _pos;

        public Token Current => _tokens[_pos];
        public IReadOnlyList<string> InactiveCodes => _inactiveCodes;
        public IReadOnlyList<MissingCodeInfo> MissingCodes => _missingCodes;

        public FormulaParser(List<Token> tokens, HashSet<string> activeCodes, HashSet<string>? allCodes, int offset)
        {
            _tokens = tokens;
            _activeCodes = activeCodes;
            _allCodes = allCodes;
            _offset = offset;
        }

        /// <summary>Trả về token hiện tại rồi tiến con trỏ sang token tiếp theo.</summary>
        /// Created By: ntdo (2026-06-09)
        private Token Consume() => _tokens[_pos++];

        /// <summary>Kiểm tra token hiện tại đúng loại <paramref name="kind"/>; nếu đúng thì tiêu thụ, sai thì throw lỗi cú pháp.</summary>
        /// Created By: ntdo (2026-06-09)
        private void Expect(TokenKind kind, string errorHint)
        {
            if (Current.Kind != kind)
            {
                var got = Current.Kind == TokenKind.Eof ? "cuối công thức" : $"'{Current.Value}'";
                throw new FormulaException($"{errorHint}, nhưng gặp {got} tại vị trí {Current.Pos + 1}");
            }
            Consume();
        }

        /// <summary>Điểm vào của parser — parse toàn bộ biểu thức giá trị.</summary>
        public void ParseValueExpr() => ParseAdditive();

        /// <summary>Parse phép cộng/trừ — độ ưu tiên thấp nhất trong biểu thức số học.</summary>
        /// Created By: ntdo (2026-06-10)
        private void ParseAdditive()
        {
            ParseMultiplicative();
            while (Current.Kind == TokenKind.ArithOp && Current.Value is "+" or "-")
            {
                Consume();
                ParseMultiplicative();
            }
        }

        /// <summary>Parse phép nhân/chia — độ ưu tiên cao hơn cộng/trừ.</summary>
        /// Created By: ntdo (2026-06-10)
        private void ParseMultiplicative()
        {
            ParseUnary();
            while (Current.Kind == TokenKind.ArithOp && Current.Value is "*" or "/")
            {
                Consume();
                ParseUnary();
            }
        }

        /// <summary>Parse toán tử một ngôi — hiện tại chỉ hỗ trợ dấu âm (ví dụ: -SUM(A,B), -LUONG_CO_BAN).</summary>
        /// Created By: ntdo (2026-06-10)
        private void ParseUnary()
        {
            // Dấu âm một ngôi: -SUM(A,B), -LUONG_CO_BAN, ...
            if (Current.Kind == TokenKind.ArithOp && Current.Value == "-")
            {
                Consume();
                ParseUnary();
                return;
            }
            ParsePrimary();
        }

        /// <summary>Parse đơn vị cơ bản: số, biểu thức trong ngoặc, mã TPL, hoặc lời gọi hàm.</summary>
        /// Created By: ntdo (2026-06-10)
        private void ParsePrimary()
        {
            // Số (bao gồm số âm đã được tokenizer gộp thành một token)
            if (Current.Kind == TokenKind.Number)
            {
                Consume();
                return;
            }

            // Biểu thức nhóm trong ngoặc: (A + B) * C
            if (Current.Kind == TokenKind.LParen)
            {
                Consume();
                ParseValueExpr();
                Expect(TokenKind.RParen, "Mong đợi ')' đóng ngoặc nhóm");
                return;
            }

            if (Current.Kind == TokenKind.Ident)
            {
                var token = Current;
                Consume();

                if (Current.Kind == TokenKind.LParen)
                {
                    ParseFunctionAsValue(token.Value);
                    return;
                }

                // TPL code reference — phân biệt hoa thường
                if (!_activeCodes.Contains(token.Value))
                {
                    if (_allCodes != null && _allCodes.Contains(token.Value))
                        _inactiveCodes.Add(token.Value); // cảnh báo mềm — tiếp tục parse
                    else
                        // Gom mã không tồn tại + vị trí (tuyệt đối trong formula gốc) cho FE highlight, không fail-fast
                        _missingCodes.Add(new MissingCodeInfo(token.Value, token.Pos + _offset, token.Value.Length));
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
        /// Created By: ntdo (2026-06-11)
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
                    // IF(điều_kiện, giá_trị_nếu_đúng, giá_trị_nếu_sai)
                    ParseCondExpr();
                    Expect(TokenKind.Comma, "IF: mong đợi ',' sau điều kiện");
                    ParseValueExpr();
                    Expect(TokenKind.Comma, "IF: mong đợi ',' sau value_if_true");
                    ParseValueExpr();
                    break;

                case "INT":
                    // INT(số) — làm tròn xuống số nguyên
                    ParseValueExpr();
                    break;

                case "TODAY":
                    // TODAY() — không tham số
                    break;
            }

            Expect(TokenKind.RParen, $"Mong đợi ')' để đóng '{name}'");
        }

        /// <summary>Parse biểu thức điều kiện: AND/OR(...) hoặc so sánh hai giá trị (value CmpOp value).</summary>
        /// Created By: ntdo (2026-06-11)
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

            // So sánh hai giá trị — ví dụ: A + B > 1000, TODAY() >= NGAY_SINH
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
