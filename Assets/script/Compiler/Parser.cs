using System;
using System.Collections.Generic;
using System.Linq;

namespace GwentCompiler
{
    public class ParserUnity
    {
        private readonly List<Tokens> tokens;
        private readonly SymbolTable symbolTable = new SymbolTable();
        private int currentToken = 0;
        

        public ParserUnity(List<Tokens> tokens)
        {
            this.tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        }

        private Tokens Current => tokens[currentToken];

        public ProgramNode Parse()
        {
            List<DefinitionNode> definitions = new List<DefinitionNode>();

            while (currentToken < tokens.Count && Current.Type != TokenType.EOF)
            {
                definitions.Add(ParseDefinition());
            }

            return new ProgramNode(definitions);
        }

        private DefinitionNode ParseDefinition()
        {
            if (Match(TokenType.Keyword, "effect"))
                return ParseEffectDefinition();
            else if (Match(TokenType.Keyword, "card"))
                return ParseCardDefinition();
            else
                throw new ParserException($"Expected 'effect' or 'card', got {Current.Value}", Current);
        }

        private EffectDefinitionNode ParseEffectDefinition()
        {
            Consume(TokenType.Keyword, "effect");
            Consume(TokenType.Delimiter, "{");

            string name = ParseNameProperty();
            List<ParameterNode> parameters = new List<ParameterNode>();

            if (Match(TokenType.Identifier, "Params"))
            {
                Consume(TokenType.Identifier, "Params");
                parameters = ParseParameters();
                Consume(TokenType.Delimiter, ",");
            }

            Consume(TokenType.Identifier, "Action");
            Consume(TokenType.Delimiter, ":");
            var lambdaExpression = ParseLambdaExpression();

            Consume(TokenType.Delimiter, "}");

            var effectNode = new EffectDefinitionNode(name, parameters, lambdaExpression);
            symbolTable.DeclareEffect(effectNode);

            return effectNode;
        }

        private List<ParameterNode> ParseParameters()
        {
            List<ParameterNode> parameters = new List<ParameterNode>();

            Consume(TokenType.Delimiter, ":");
            Consume(TokenType.Delimiter, "{");

            while (!Match(TokenType.Delimiter, "}"))
            {
                string paramName = Consume(TokenType.Identifier).Value;
                Consume(TokenType.Delimiter, ":");
                string paramType = Consume(TokenType.Identifier).Value;

                parameters.Add(new ParameterNode(paramName, paramType));

                if (Match(TokenType.Delimiter, ","))
                    Consume(TokenType.Delimiter, ",");
                else
                    break;
            }

            Consume(TokenType.Delimiter, "}");

            return parameters;
        }

        private LambdaExpressionNode ParseLambdaExpression()
        {
            Consume(TokenType.Delimiter, "(");
            List<string> parameters = ParseLambdaParameters();
            Consume(TokenType.Delimiter, ")");

            Consume(TokenType.Arrow, "=>");

            if (Match(TokenType.Delimiter, "{"))
            {
                Consume(TokenType.Delimiter, "{");
                List<StatementNode> body = ParseStatements();
                Consume(TokenType.Delimiter, "}");
                return new LambdaExpressionNode(parameters, body);
            }
            else
            {
                // Manejar expresiones lambda de una sola línea
                ExpressionNode expression = ParseExpression();
                return new LambdaExpressionNode(parameters, new List<StatementNode> { expression });
            }
        }
        private List<string> ParseLambdaParameters()
        {
            List<string> parameters = new List<string>();

            while (!Match(TokenType.Delimiter, ")"))
            {
                parameters.Add(Consume(TokenType.Identifier).Value);

                if (Match(TokenType.Delimiter, ","))
                    Consume(TokenType.Delimiter, ",");
                else
                    break;
            }

            return parameters;
        }

        private List<StatementNode> ParseStatements()
        {
            List<StatementNode> statements = new List<StatementNode>();

            while (!Match(TokenType.Delimiter, "}"))
            {
                statements.Add(ParseStatement());
                if (Current.Value == ";") Consume(TokenType.Delimiter, ";");
            }

            return statements;
        }

        private StatementNode ParseStatement()
        {
            if (Match(TokenType.Keyword, "for"))
                return ParseForLoop();
            else if (Match(TokenType.Keyword, "while"))
                return ParseWhileLoop();
            else if (Match(TokenType.Keyword, "if"))
                return ParseIfStatement();
            else if (Match(TokenType.Keyword, "int"))
                return ParseIntAssignament();
            else if (Match(TokenType.Identifier))
                return ParseAssignmentOrExpressionStatement();

            else
                throw new ParserException($"Unexpected token in statement: {Current.Value}", Current);
        }

        private ForLoopNode ParseForLoop()
        {
            Consume(TokenType.Keyword, "for");
            string iterator = Consume(TokenType.Identifier).Value;
            Consume(TokenType.Keyword, "in");
            IdentifierNode collection = new IdentifierNode(Consume(TokenType.Identifier).Value);
            Consume(TokenType.Delimiter, "{");
            List<StatementNode> body = ParseStatements();
            Consume(TokenType.Delimiter, "}");

            return new ForLoopNode(iterator, collection, body);
        }

        private CardDefinitionNode ParseCardDefinition()
        {
            List<EffectInvocationNode> onActivation = new List<EffectInvocationNode>();
            Consume(TokenType.Keyword, "card");
            Consume(TokenType.Delimiter, "{");

            string name = ParseNameProperty();
            string type = ParseStringProperty("type");
            string faction = ParseStringProperty("faction");
            int power = ParseNumberProperty("power");
            List<string> range = ParseRangeProperty();
            if (Current.Value == "OnActivation") onActivation = ParseOnActivationProperty();

            Consume(TokenType.Delimiter, "}");

            var cardNode = new CardDefinitionNode(name, type, faction, power, range, onActivation);
            symbolTable.DeclareCard(cardNode);

            return cardNode;
        }

        private string ParseNameProperty()
        {
            Consume(TokenType.Identifier, "Name");
            Consume(TokenType.Delimiter, ":");
            string name = Consume(TokenType.String).Value;
            Consume(TokenType.Delimiter, ",");
            return name;
        }

        private string ParseStringProperty(string propertyName)
        {
            Consume(TokenType.Identifier, propertyName);
            Consume(TokenType.Delimiter, ":");
            string value = Consume(TokenType.String).Value;
            Consume(TokenType.Delimiter, ",");
            return value;
        }

        private int ParseNumberProperty(string propertyName)
        {
            Consume(TokenType.Identifier, propertyName);
            Consume(TokenType.Delimiter, ":");
            int value = int.Parse(Consume(TokenType.Number).Value);
            Consume(TokenType.Delimiter, ",");
            return value;
        }

        private List<string> ParseRangeProperty()
        {
            Consume(TokenType.Identifier, "range");
            Consume(TokenType.Delimiter, ":");
            Consume(TokenType.Delimiter, "[");

            List<string> range = new List<string>();

            while (!Match(TokenType.Delimiter, "]"))
            {
                range.Add(Consume(TokenType.String).Value);

                if (Match(TokenType.Delimiter, ","))
                    Consume(TokenType.Delimiter, ",");
                else
                    break;
            }

            Consume(TokenType.Delimiter, "]");
            Consume(TokenType.Delimiter, ",");

            return range;
        }

        private List<EffectInvocationNode> ParseOnActivationProperty()
        {
            Consume(TokenType.Identifier, "OnActivation");
            Consume(TokenType.Delimiter, ":");
            Consume(TokenType.Delimiter, "[");

            List<EffectInvocationNode> effects = new List<EffectInvocationNode>();

            while (!Match(TokenType.Delimiter, "]"))
            {
                effects.Add(ParseEffectInvocation());

                if (Match(TokenType.Delimiter, ","))
                    Consume(TokenType.Delimiter, ",");
                else
                    break;
            }

            Consume(TokenType.Delimiter, "]");

            return effects;
        }

        private EffectInvocationNode ParseEffectInvocation()
        {
            Consume(TokenType.Delimiter, "{");

            string effectName;
            Dictionary<string, ExpressionNode> arguments = new Dictionary<string, ExpressionNode>();

            Consume(TokenType.Identifier, "Effect");
            Consume(TokenType.Delimiter, ":");

            if (Match(TokenType.String))
            {
                effectName = Consume(TokenType.String).Value;
            }
            else
            {
                Consume(TokenType.Delimiter, "{");
                Consume(TokenType.Identifier, "Name");
                Consume(TokenType.Delimiter, ":");
                effectName = Consume(TokenType.String).Value;
                Consume(TokenType.Delimiter, ",");

                // Parse arguments
                while (!Match(TokenType.Delimiter, "}"))
                {
                    string argName = Consume(TokenType.Identifier).Value;
                    Consume(TokenType.Delimiter, ":");
                    ExpressionNode argValue = ParseExpression();
                    arguments.Add(argName, argValue);

                    if (Match(TokenType.Delimiter, ","))
                        Consume(TokenType.Delimiter, ",");
                    else
                        break;
                }

                Consume(TokenType.Delimiter, "}");
            }

            SelectorNode? selector = null;
            if (Match(TokenType.Delimiter, ","))
            {
                Consume(TokenType.Delimiter, ",");
                if (Match(TokenType.Identifier, "Selector"))
                {
                    selector = ParseSelector();
                }
            }

            EffectInvocationNode? postAction = null;
            if (Match(TokenType.Delimiter, ","))
            {
                Consume(TokenType.Delimiter, ",");
                if (Match(TokenType.Identifier, "PostAction"))
                {
                    Consume(TokenType.Identifier, "PostAction");
                    Consume(TokenType.Delimiter, ":");
                    postAction = ParseEffectInvocation();
                }
            }

            Consume(TokenType.Delimiter, "}");

            return new EffectInvocationNode(effectName, arguments, selector, postAction);
        }

        private SelectorNode ParseSelector()
        {
            Consume(TokenType.Identifier, "Selector");
            Consume(TokenType.Delimiter, ":");
            Consume(TokenType.Delimiter, "{");

            Consume(TokenType.Identifier, "Source");
            Consume(TokenType.Delimiter, ":");
            string source = Consume(TokenType.String).Value;
            Consume(TokenType.Delimiter, ",");

            Consume(TokenType.Identifier, "Single");
            Consume(TokenType.Delimiter, ":");
            bool single = bool.Parse(Consume(TokenType.Identifier).Value.ToLower());

            LambdaExpressionNode? predicate = null;
            if (Match(TokenType.Delimiter, ","))
            {
                Consume(TokenType.Delimiter, ",");
                Consume(TokenType.Identifier, "Predicate");
                Consume(TokenType.Delimiter, ":");
                predicate = ParseLambdaExpression();
            }

            Consume(TokenType.Delimiter, "}");

            return new SelectorNode(source, single, predicate);
        }

        private bool Match(TokenType type, string? value = null)
        {
            return Current.Type == type && (value == null || Current.Value == value);
        }

        private Tokens Consume(TokenType type, string? value = null)
        {
            if (!Match(type, value))
            {
                throw new ParserException(
                    $"Expected {type} {(value != null ? $"with value '{value}'" : "")}, got {Current.Type} '{Current.Value}'",
                    Current);
            }

            Tokens token = Current;
            currentToken++;
            return token;
        }
        private WhileLoopNode ParseWhileLoop()
        {
            Consume(TokenType.Keyword, "while");
            Consume(TokenType.Delimiter, "(");
            ExpressionNode condition = ParseExpression();
            Consume(TokenType.Delimiter, ")");

            Consume(TokenType.Delimiter, "{");
            List<StatementNode> body = ParseStatements();
            Consume(TokenType.Delimiter, "}");

            return new WhileLoopNode(condition, body);
        }

        private IfStatementNode ParseIfStatement()
        {
            Consume(TokenType.Keyword, "if");
            Consume(TokenType.Delimiter, "(");
            ExpressionNode condition = ParseExpression();
            Consume(TokenType.Delimiter, ")");

            Consume(TokenType.Delimiter, "{");
            List<StatementNode> thenBlock = ParseStatements();
            Consume(TokenType.Delimiter, "}");

            List<StatementNode>? elseBlock = null;
            if (Match(TokenType.Keyword, "else"))
            {
                Consume(TokenType.Keyword, "else");
                Consume(TokenType.Delimiter, "{");
                elseBlock = ParseStatements();
                Consume(TokenType.Delimiter, "}");
            }

            return new IfStatementNode(condition, thenBlock, elseBlock);
        }

        private StatementNode ParseAssignmentOrExpressionStatement()
        {
            string variable = Current.Value;
            ExpressionNode expression = new();
            expression = ParseExpression();
            expression.Solution = variable;

            if (Match(TokenType.Operator, "="))
            {
                Consume(TokenType.Operator, "=");
                ExpressionNode value = ParseExpression();
                Consume(TokenType.Delimiter, ";");
                return new AssignmentNode(expression, value);
            }

            // Si no hay un operador de asignación, es una expresión statement
            return expression;
        }

        private StatementNode ParseIntAssignament()
        {
            string varType = Consume(TokenType.Keyword, "int").Value; // Verifica la palabra clave 'int'
            string varName = Consume(TokenType.Identifier).Value; // Identifica el nombre de la variable

            if (Match(TokenType.Operator, "="))
            {
                Consume(TokenType.Operator, "=");
                string value = Consume(TokenType.Number).Value; // Captura el valor asignado
                Consume(TokenType.Delimiter, ";");
                return new intAssignamentNode(varName, value, varType); // Crear el nodo correctamente
            }

            // Si no hay asignación, devolver una asignación por defecto (ejemplo: 0)
            return new intAssignamentNode(varName, "0", varType);
        }
        private ExpressionNode ParseExpression()
        {
            return ParseBinaryExpression();
        }

        private ExpressionNode ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionNode left = ParseUnaryExpression();
            if (Current.Value == ";")
            {
                return left;
            }
; if (Current.Type == TokenType.IncOrDec)
            {
                left = new IncrementDecrementNode(tokens[currentToken - 1], Current);
                Advance();
            }
            while (true)
            {
                int precedence = GetCurrentPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                string op = Current.Value;
                Advance();

                ExpressionNode right = ParseBinaryExpression(precedence);
                if (Current.Value == ";")
                {
                    Consume(TokenType.Delimiter, ";");
                    break;
                }
                left = new BinaryExpressionNode(left, op, right);
            }

            return left;
        }

        private int GetCurrentPrecedence()
        {

            if (Current.Type != TokenType.Operator)
                return 0;

            switch (Current.Value)
            {
                case "||": return 1;
                case "&&": return 2;
                case "==": return 3;
                case "!=": return 3;
                case "<": return 4;
                case "<=": return 4;
                case ">": return 4;
                case ">=": return 4;
                case "+": return 5;
                case "+=": return 5;
                case "-": return 5;
                case "-=": return 5;
                case "*": return 6;
                case "/": return 6;
                default: return 0;
            }
        }

        private ExpressionNode ParseUnaryExpression()
        {
            if (Current.Type == TokenType.Operator && (Current.Value == "!" || Current.Value == "-"))
            {

                string op = Current.Value;
                Advance();
                ExpressionNode operand = ParseUnaryExpression();
                return new UnaryExpressionNode(op, operand);
            }
            if (Current.Type == TokenType.IncOrDec)
            {
                var token = Current;
                Advance();
                return new IncrementDecrementNode(token, token); // Crear el nodo aquí
            }

            return ParsePrimaryExpression();
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            switch (Current.Type)
            {
                case TokenType.Number:
                    return ParseLiteral();
                case TokenType.String:
                    return ParseLiteral();
                case TokenType.Identifier:
                    return ParseIdentifierExpression();
                case TokenType.Delimiter when Current.Value == "(":
                    return ParseParenthesizedExpression();
                default:
                    throw new ParserException($"Unexpected token in expression: {Current.Value}", Current);
            }
        }

        private LiteralNode ParseLiteral()
        {
            Tokens token = Current;
            Advance();
            object value = token.Type == TokenType.Number
                ? int.Parse(token.Value)
                : token.Value;
            return new LiteralNode(value);
        }


        private FunctionCallNode ParseIdentifierExpression()
        {
            string identifier = Consume(TokenType.Identifier).Value;

            if (Match(TokenType.Delimiter, "("))
                return ParseFunctionCall(identifier);
            if (Match(TokenType.Delimiter, "."))
            {
                identifier += Consume(TokenType.Delimiter, ".").Value;
                identifier += Consume(TokenType.Identifier).Value;
                if (Current.Value == "(")
                {
                    identifier += "(";
                    Consume(TokenType.Delimiter, "(");
                    if (Current.Type == TokenType.Identifier)
                    {
                        identifier += Current.Value;
                        Consume(TokenType.Identifier);
                    }
                    identifier += ")";
                    Consume(TokenType.Delimiter, ")");
                }
                identifier = identifier.ToLower();
                if (Current.Value == ".") return ParsePropertyAccess(identifier);
            }
            identifier = identifier.ToLower();
            return new FunctionCallNode(identifier, new List<ExpressionNode>());
        }

        private FunctionCallNode ParseFunctionCall(string name)
        {
            Consume(TokenType.Delimiter, "(");
            List<ExpressionNode> arguments = new List<ExpressionNode>();

            while (!Match(TokenType.Delimiter, ")"))
            {
                arguments.Add(ParseExpression());

                if (Match(TokenType.Delimiter, ","))
                    Consume(TokenType.Delimiter, ",");
                else
                    break;
            }
            Consume(TokenType.Delimiter, ")");
            return new FunctionCallNode(name, arguments);
        }

        private new FunctionCallNode ParsePropertyAccess(string Identifi)
        {
            Consume(TokenType.Delimiter, ".");
            string propertyName = Consume(TokenType.Identifier).Value;
            Identifi += "." + propertyName;
            Identifi += "(";
            Consume(TokenType.Delimiter, "(");
            if (Current.Type == TokenType.Identifier)
            {
                Identifi += Current.Value;
                Consume(TokenType.Identifier);
            }
            Identifi += ")";
            Consume(TokenType.Delimiter, ")");
            Identifi = Identifi.ToLower();
            return new FunctionCallNode(Identifi, new List<ExpressionNode>());
        }

        private ExpressionNode ParseParenthesizedExpression()
        {
            Consume(TokenType.Delimiter, "(");
            ExpressionNode expression = ParseExpression();
            Consume(TokenType.Delimiter, ")");
            return expression;
        }
        public class ParserException : Exception
        {
            public Tokens Token { get; }

            public ParserException(string message, Tokens token) : base($"{message} at line {token.Line}, column {token.Column}")
            {
                Token = token;
            }
        }

        private void Advance()
        {
            currentToken++;
        }
    }
}
