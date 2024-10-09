using System;
using System.Collections.Generic;
using System.Text;

namespace GwentCompiler
{
    public enum TokenType
    {
        Identifier, Keyword, Number, String, VerbatimString, Delimiter, Operator, Arrow, EOF, IncOrDec
    }

    public class Tokens
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Tokens(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"Token: {Type}, Value: {Value}, Line: {Line}, Column: {Column}";
        }
    }

    public class LexerUnity
    {
        private readonly string source;
        private int currentIndex;
        private int line;
        private int column;
        private readonly Dictionary<string, TokenType> keywords;

        public LexerUnity(string sourceCode)
        {
            source = sourceCode;
            currentIndex = 0;
            line = 1;
            column = 1;
            keywords = new Dictionary<string, TokenType>
            {
                { "effect", TokenType.Keyword },
                { "card", TokenType.Keyword },
                { "for", TokenType.Keyword },
                { "in", TokenType.Keyword },
                { "while", TokenType.Keyword },
                { "if", TokenType.Keyword },
                { "else", TokenType.Keyword },
                { "return", TokenType.Keyword },
                { "int", TokenType.Keyword}
            };
        }

        private char CurrentChar => currentIndex < source.Length ? source[currentIndex] : '\0';

        private void Advance()
        {
            currentIndex++;
            column++;
        }

        private char PeekNextChar() => currentIndex + 1 < source.Length ? source[currentIndex + 1] : '\0';

        private bool EndOfFile() => currentIndex >= source.Length;

        public List<Tokens> Tokenize()
        {
            List<Tokens> tokens = new List<Tokens>();

            while (!EndOfFile())
            {
                SkipWhiteSpaceAndComments();

                if (EndOfFile())
                    break;

                if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    tokens.Add(ConsumeIdentifierOrKeyword());
                }
                else if (char.IsDigit(CurrentChar))
                {
                    tokens.Add(ConsumeNumber());
                }
                else if (CurrentChar == '@')
                {
                    tokens.Add(ConsumeAtSymbol());
                }
                else if (CurrentChar == '"')
                {
                    tokens.Add(ConsumeString());
                }
                else if (IsOperator(CurrentChar))
                {
                    tokens.Add(ConsumeOperatorOrArrow());
                }
                else if (IsDelimiter(CurrentChar))
                {
                    tokens.Add(ConsumeDelimiter());
                }
                else
                {
                    throw new Exception($"Unexpected character '{CurrentChar}' at line {line}, column {column}");
                }
            }

            tokens.Add(new Tokens(TokenType.EOF, string.Empty, line, column));
            return tokens;
        }

        public void PrintTokens()
        {
            var tokens = Tokenize();
            Console.WriteLine("Tokens generated:");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        private void SkipWhiteSpaceAndComments()
        {
            while (char.IsWhiteSpace(CurrentChar) || IsComment())
            {
                if (char.IsWhiteSpace(CurrentChar))
                {
                    if (CurrentChar == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    Advance();
                }

                if (IsComment())
                {
                    if (CurrentChar == '/' && PeekNextChar() == '/')
                    {
                        // Single-line comment
                        while (CurrentChar != '\n' && !EndOfFile())
                        {
                            Advance();
                        }
                    }
                    else if (CurrentChar == '/' && PeekNextChar() == '*')
                    {
                        // Block comment
                        Advance();
                        Advance();
                        while (!(CurrentChar == '*' && PeekNextChar() == '/') && !EndOfFile())
                        {
                            if (CurrentChar == '\n')
                            {
                                line++;
                                column = 1;
                            }
                            Advance();
                        }
                        if (!EndOfFile())
                        {
                            Advance(); // Skip '*'
                            Advance(); // Skip '/'
                        }
                        else
                        {
                            throw new Exception($"Unterminated block comment at line {line}, column {column}");
                        }
                    }
                }
            }
        }

        private bool IsComment()
        {
            return CurrentChar == '/' && (PeekNextChar() == '/' || PeekNextChar() == '*');
        }

        private Tokens ConsumeIdentifierOrKeyword()
        {
            var identifier = new StringBuilder();
            int startColumn = column;

            while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
            {
                identifier.Append(CurrentChar);
                Advance();
            }

            string identifierValue = identifier.ToString();
            if (keywords.ContainsKey(identifierValue))
            {
                return new Tokens(keywords[identifierValue], identifierValue, line, startColumn);
            }
            else
            {
                return new Tokens(TokenType.Identifier, identifierValue, line, startColumn);
            }
        }

        private Tokens ConsumeNumber()
        {
            var number = new StringBuilder();
            int startColumn = column;

            while (char.IsDigit(CurrentChar))
            {
                number.Append(CurrentChar);
                Advance();
            }

            return new Tokens(TokenType.Number, number.ToString(), line, startColumn);
        }

        private Tokens ConsumeString()
        {
            var stringLiteral = new StringBuilder();
            int startColumn = column;

            Advance(); // Skip the opening quote

            while (CurrentChar != '"' && !EndOfFile())
            {
                stringLiteral.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar == '"')
            {
                Advance(); // Skip the closing quote
                return new Tokens(TokenType.String, stringLiteral.ToString(), line, startColumn);
            }
            else
            {
                throw new Exception($"Unterminated string literal at line {line}, column {startColumn}");
            }
        }

        private Tokens ConsumeAtSymbol()
        {
            int startColumn = column;
            Advance(); // Skip '@'

            if (CurrentChar == '"')
            {
                // Handle verbatim string
                return ConsumeVerbatimString();
            }
            else if (CurrentChar == '@')
            {
                Advance(); // Skip second '@'
                return new Tokens(TokenType.Operator, "@@", line, startColumn);
            }
            else
            {
                throw new Exception($"Unexpected character after '@' at line {line}, column {startColumn}");
            }
        }

        private Tokens ConsumeVerbatimString()
        {
            var stringLiteral = new StringBuilder();
            int startColumn = column;

            Advance(); // Skip the opening quote after '@'

            while (!(CurrentChar == '"' && PeekNextChar() != '"') && !EndOfFile())
            {
                if (CurrentChar == '"' && PeekNextChar() == '"')
                {
                    // Allow double quote inside verbatim string
                    stringLiteral.Append('"');
                    Advance();
                }
                stringLiteral.Append(CurrentChar);
                Advance();
            }

            if (CurrentChar == '"')
            {
                Advance(); // Skip the closing quote
                return new Tokens(TokenType.VerbatimString, stringLiteral.ToString(), line, startColumn);
            }
            else
            {
                throw new Exception($"Unterminated verbatim string at line {line}, column {startColumn}");
            }
        }

        private Tokens ConsumeOperatorOrArrow()
        {
            int startColumn = column;

            // Manejar operadores compuestos como '+=', '-=', '*=', '/='
            if (CurrentChar == '+' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '+'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "+=", line, startColumn);
            }
            if (CurrentChar == '=' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '+'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "==", line, startColumn);
            }
            if (CurrentChar == '-' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '-'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "-=", line, startColumn);
            }
            if (CurrentChar == '*' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '*'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "*=", line, startColumn);
            }
            if (CurrentChar == '/' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '/'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "/=", line, startColumn);
            }
            // Manejar otros operadores como '++', '--', '=>', '&&', '||', '!='
            if (CurrentChar == '+' && PeekNextChar() == '+')
            {
                Advance(); // Saltar '+'
                Advance(); // Saltar '+'
                return new Tokens(TokenType.IncOrDec, "++", line, startColumn);
            }
            if (CurrentChar == '-' && PeekNextChar() == '-')
            {
                Advance(); // Saltar '-'
                Advance(); // Saltar '-'
                return new Tokens(TokenType.IncOrDec, "--", line, startColumn);
            }
            if (CurrentChar == '=' && PeekNextChar() == '>')
            {
                Advance(); // Saltar '='
                Advance(); // Saltar '>'
                return new Tokens(TokenType.Arrow, "=>", line, startColumn);
            }
            if (CurrentChar == '&' && PeekNextChar() == '&')
            {
                Advance(); // Saltar '&'
                Advance(); // Saltar '&'
                return new Tokens(TokenType.Operator, "&&", line, startColumn);
            }
            if (CurrentChar == '|' && PeekNextChar() == '|')
            {
                Advance(); // Saltar '|'
                Advance(); // Saltar '|'
                return new Tokens(TokenType.Operator, "||", line, startColumn);
            }
            if (CurrentChar == '!' && PeekNextChar() == '=')
            {
                Advance(); // Saltar '!'
                Advance(); // Saltar '='
                return new Tokens(TokenType.Operator, "!=", line, startColumn);  // Handling '!=' operator
            }
            if (CurrentChar == '!')
            {
                Advance(); // Saltar '!'
                return new Tokens(TokenType.Operator, "!", line, startColumn);  // Handling '!' operator
            }

            // Manejar operadores de un solo carácter
            char currentOp = CurrentChar;
            Advance();
            return new Tokens(TokenType.Operator, currentOp.ToString(), line, startColumn);
        }
        private Tokens ConsumeDelimiter()
        {
            char delimiter = CurrentChar;
            int startColumn = column;
            Advance();
            return new Tokens(TokenType.Delimiter, delimiter.ToString(), line, startColumn);
        }

        private bool IsOperator(char c)
        {
            return "+-*/^=&|<>!@".IndexOf(c) >= 0;
        }

        private bool IsDelimiter(char c)
        {
            return "{}[](),;:.".IndexOf(c) >= 0;
        }
    }
    
}
