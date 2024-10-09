using System;
using System.Collections.Generic;
using System.Linq;

namespace GwentCompiler
{
    /*
    public class Compiler
    {
        private Lexer lexer;
        private Parser parser;
        private SemanticAnalyzer semanticAnalyzer;
        private SymbolTable symbolTable = new SymbolTable();

        public Compiler()
        {
            semanticAnalyzer = new SemanticAnalyzer(symbolTable);
        }

        public void Compile(string sourceCode)
        {
            try
            {
                // Paso 1: Análisis léxico (tokenización)
                Console.WriteLine("=== Análisis Léxico ===");
                lexer = new Lexer(sourceCode);
                List<Token> tokens = lexer.Tokenize();

                Console.WriteLine("Tokens generados:");
                foreach (var token in tokens)
                {
                    Console.WriteLine($"Token: {token.Type}, Value: {token.Value}, Line: {token.Line}, Column: {token.Column}");
                }

                // Paso 2: Análisis sintáctico (parsing)
                Console.WriteLine("\n=== Análisis Sintáctico ===");
                parser = new Parser(tokens);
                ProgramNode program = parser.Parse();

                // Visualizar el AST
                ASTVisualizer.PrintParserResult(program);

                // Paso 3: Análisis semántico
                Console.WriteLine("\n=== Análisis Semántico ===");
                List<string> semanticErrors = semanticAnalyzer.Analyze(program);

                // Visualizar resultados del análisis semántico
                ASTVisualizer.PrintSemanticAnalyzerResult(semanticErrors);

                if (semanticErrors.Count == 0)
                {
                    Console.WriteLine("\nCompilación completada con éxito.");
                }
                else
                {
                    Console.WriteLine("\nCompilación completada con errores.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError fatal durante la compilación: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public static class ASTVisualizer
    {
        public static void PrintParserResult(ProgramNode program)
        {
            Console.WriteLine("Abstract Syntax Tree:");
            PrintNode(program, "", true);
        }

        public static void PrintSemanticAnalyzerResult(List<string> errors)
        {
            if (errors.Count == 0)
            {
                Console.WriteLine("No se encontraron errores semánticos.");
            }
            else
            {
                Console.WriteLine($"Se encontraron {errors.Count} errores semánticos:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }

        private static void PrintNode(object node, string indent, bool isLast)
        {
            var marker = isLast ? "└── " : "├── ";

            Console.Write(indent);
            Console.Write(marker);

            switch (node)
            {
                case ProgramNode program:
                    Console.WriteLine("Program");
                    PrintChildren(program.Definitions, indent + (isLast ? "    " : "│   "));
                    break;

                case EffectDefinitionNode effect:
                    Console.WriteLine($"Effect: {effect.Name}");
                    Console.WriteLine($"{indent}    ├── Parameters:");
                    foreach (var param in effect.Parameters)
                    {
                        Console.WriteLine($"{indent}    │   └── {param.Name}: {param.Type}");
                    }
                    Console.WriteLine($"{indent}    └── Body:");
                    PrintNode(effect.Body, indent + "        ", true);
                    break;

                case CardDefinitionNode card:
                    Console.WriteLine($"Card: {card.Name}");
                    Console.WriteLine($"{indent}    ├── Type: {card.Type}");
                    Console.WriteLine($"{indent}    ├── Faction: {card.Faction}");
                    Console.WriteLine($"{indent}    ├── Power: {card.Power}");
                    Console.WriteLine($"{indent}    ├── Range: [{string.Join(", ", card.Range)}]");
                    if (card.OnActivation.Any())
                    {
                        Console.WriteLine($"{indent}    └── OnActivation:");
                        PrintChildren(card.OnActivation, indent + "        ");
                    }
                    break;

                case LambdaExpressionNode lambda:
                    Console.WriteLine($"Lambda: ({string.Join(", ", lambda.Parameters)})");
                    PrintChildren(lambda.Body, indent + "    ");
                    break;

                case EffectInvocationNode invocation:
                    Console.WriteLine($"Invoke Effect: {invocation.EffectName}");
                    if (invocation.Arguments.Any())
                    {
                        Console.WriteLine($"{indent}    ├── Arguments:");
                        foreach (var arg in invocation.Arguments)
                        {
                            Console.WriteLine($"{indent}    │   └── {arg.Key}: ");
                            PrintNode(arg.Value, indent + "    │       ", true);
                        }
                    }
                    if (invocation.Selector != null)
                    {
                        Console.WriteLine($"{indent}    └── Selector:");
                        PrintNode(invocation.Selector, indent + "        ", invocation.PostAction == null);
                    }
                    if (invocation.PostAction != null)
                    {
                        Console.WriteLine($"{indent}    └── PostAction:");
                        PrintNode(invocation.PostAction, indent + "        ", true);
                    }
                    break;

                case SelectorNode selector:
                    Console.WriteLine($"Selector: {selector.Source} (Single: {selector.Single})");
                    if (selector.Predicate != null)
                    {
                        Console.WriteLine($"{indent}    └── Predicate:");
                        PrintNode(selector.Predicate, indent + "        ", true);
                    }
                    break;

                case ForLoopNode forLoop:
                    Console.WriteLine($"For Loop: {forLoop.IteratorName}");
                    Console.WriteLine($"{indent}    ├── Collection:");
                    PrintNode(forLoop.Collection, indent + "    │   ", true);
                    Console.WriteLine($"{indent}    └── Body:");
                    PrintChildren(forLoop.Body, indent + "        ");
                    break;

                case WhileLoopNode whileLoop:
                    Console.WriteLine("While Loop");
                    Console.WriteLine($"{indent}    ├── Condition:");
                    PrintNode(whileLoop.Condition, indent + "    │   ", true);
                    Console.WriteLine($"{indent}    └── Body:");
                    PrintChildren(whileLoop.Body, indent + "        ");
                    break;

                case IfStatementNode ifStatement:
                    Console.WriteLine("If Statement");
                    Console.WriteLine($"{indent}    ├── Condition:");
                    PrintNode(ifStatement.Condition, indent + "    │   ", true);
                    Console.WriteLine($"{indent}    ├── Then:");
                    PrintChildren(ifStatement.ThenBlock, indent + "    │   ");
                    if (ifStatement.ElseBlock != null)
                    {
                        Console.WriteLine($"{indent}    └── Else:");
                        PrintChildren(ifStatement.ElseBlock, indent + "        ");
                    }
                    break;

                case BinaryExpressionNode binary:
                    Console.WriteLine($"Binary: {binary.Operator}");
                    Console.WriteLine($"{indent}    ├── Left:");
                    PrintNode(binary.Left, indent + "    │   ", true);
                    Console.WriteLine($"{indent}    └── Right:");
                    PrintNode(binary.Right, indent + "        ", true);
                    break;

                case UnaryExpressionNode unary:
                    Console.WriteLine($"Unary: {unary.Operator}");
                    PrintNode(unary.Operand, indent + "    ", true);
                    break;

                case LiteralNode literal:
                    Console.WriteLine($"Literal: {literal.Value}");
                    break;

                case IdentifierNode identifier:
                    Console.WriteLine($"Identifier: {identifier.Name}");
                    break;

                case FunctionCallNode functionCall:
                    Console.WriteLine($"Function Call: {functionCall.Name}");
                    if (functionCall.Arguments.Any())
                    {
                        Console.WriteLine($"{indent}    └── Arguments:");
                        PrintChildren(functionCall.Arguments, indent + "        ");
                    }
                    break;
                case IncrementDecrementNode incrementDecrement:
                    Console.WriteLine($"Increment/Decrement: {incrementDecrement.Variable.Value}");
                    break;
                case intAssignamentNode intAssign:
                    Console.WriteLine($"Int Assignment: {intAssign.Variable} = {intAssign.Value}");
                    break;
                case AssignmentNode assignment:
                    Console.WriteLine($"Assignment: {assignment.Target.GetType().Name} = {assignment.Value.GetType().Name}");
                    PrintNode(assignment.Target, indent + "    ", false);
                    PrintNode(assignment.Value, indent + "    ", true);
                    break;

                case ExpressionNode expression:
                    // Aquí imprimimos el tipo de expresión y la propiedad 'Solution'
                    Console.WriteLine($"Expression: {expression.GetType().Name}, Solution: {expression.Solution}");
                    PrintNode(expression.Solution, indent + "    ", true);  // Si deseas imprimir el valor de la solución
                    break;
                default:
                    Console.WriteLine($"Unknown Node Type: {node.GetType().Name}");
                    break;
            }
        }

        private static void PrintChildren<T>(IEnumerable<T> children, string indent)
        {
            var lastIndex = children.Count() - 1;
            var index = 0;
            foreach (var child in children)
            {
                PrintNode(child, indent, index == lastIndex);
                index++;
            }
        }
    }
*/
}