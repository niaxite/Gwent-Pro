using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GwentCompiler;

public class DefinitionTree : MonoBehaviour
{
    public static class ASTVisualizer
    {
        public static void PrintParserResult(ProgramNode program)
        {
            Debug.Log("Abstract Syntax Tree:");
            PrintNode(program, "", true);
        }

        public static void PrintSemanticAnalyzerResult(List<string> errors)
        {
            if (errors.Count == 0)
            {
                Debug.Log("No se encontraron errores semánticos.");
            }
            else
            {
                Debug.Log($"Se encontraron {errors.Count} errores semánticos:");
                foreach (var error in errors)
                {
                    Debug.Log($"  - {error}");
                }
            }
        }

        private static void PrintNode(object node, string indent, bool isLast)
        {
            var marker = isLast ? "└── " : "├── ";

            Debug.Log(indent);
            Debug.Log(marker);

            switch (node)
            {
                case ProgramNode program:
                    Debug.Log("Program");
                    PrintChildren(program.Definitions, indent + (isLast ? "    " : "│   "));
                    break;

                case EffectDefinitionNode effect:
                    Debug.Log($"Effect: {effect.Name}");
                    Debug.Log($"{indent}    ├── Parameters:");
                    foreach (var param in effect.Parameters)
                    {
                        Debug.Log($"{indent}    │   └── {param.Name}: {param.Type}");
                    }
                    Debug.Log($"{indent}    └── Body:");
                    PrintNode(effect.Body, indent + "        ", true);
                    break;

                case CardDefinitionNode card:
                    Debug.Log($"Card: {card.Name}");
                    Debug.Log($"{indent}    ├── Type: {card.Type}");
                    Debug.Log($"{indent}    ├── Faction: {card.Faction}");
                    Debug.Log($"{indent}    ├── Power: {card.Power}");
                    Debug.Log($"{indent}    ├── Range: [{string.Join(", ", card.Range)}]");
                    if (card.OnActivation.Any())
                    {
                        Debug.Log($"{indent}    └── OnActivation:");
                        PrintChildren(card.OnActivation, indent + "        ");
                    }
                    break;

                case LambdaExpressionNode lambda:
                    Debug.Log($"Lambda: ({string.Join(", ", lambda.Parameters)})");
                    PrintChildren(lambda.Body, indent + "    ");
                    break;

                case EffectInvocationNode invocation:
                    Debug.Log($"Invoke Effect: {invocation.EffectName}");
                    if (invocation.Arguments.Any())
                    {
                        Debug.Log($"{indent}    ├── Arguments:");
                        foreach (var arg in invocation.Arguments)
                        {
                            Debug.Log($"{indent}    │   └── {arg.Key}: ");
                            PrintNode(arg.Value, indent + "    │       ", true);
                        }
                    }
                    if (invocation.Selector != null)
                    {
                        Debug.Log($"{indent}    └── Selector:");
                        PrintNode(invocation.Selector, indent + "        ", invocation.PostAction == null);
                    }
                    if (invocation.PostAction != null)
                    {
                        Debug.Log($"{indent}    └── PostAction:");
                        PrintNode(invocation.PostAction, indent + "        ", true);
                    }
                    break;

                case SelectorNode selector:
                    Debug.Log($"Selector: {selector.Source} (Single: {selector.Single})");
                    if (selector.Predicate != null)
                    {
                        Debug.Log($"{indent}    └── Predicate:");
                        PrintNode(selector.Predicate, indent + "        ", true);
                    }
                    break;

                case ForLoopNode forLoop:
                    Debug.Log($"For Loop: {forLoop.IteratorName}");
                    Debug.Log($"{indent}    ├── Collection:");
                    PrintNode(forLoop.Collection, indent + "    │   ", true);
                    Debug.Log($"{indent}    └── Body:");
                    PrintChildren(forLoop.Body, indent + "        ");
                    break;

                case WhileLoopNode whileLoop:
                    Debug.Log("While Loop");
                    Debug.Log($"{indent}    ├── Condition:");
                    PrintNode(whileLoop.Condition, indent + "    │   ", true);
                    Debug.Log($"{indent}    └── Body:");
                    PrintChildren(whileLoop.Body, indent + "        ");
                    break;

                case IfStatementNode ifStatement:
                    Debug.Log("If Statement");
                    Debug.Log($"{indent}    ├── Condition:");
                    PrintNode(ifStatement.Condition, indent + "    │   ", true);
                    Debug.Log($"{indent}    ├── Then:");
                    PrintChildren(ifStatement.ThenBlock, indent + "    │   ");
                    if (ifStatement.ElseBlock != null)
                    {
                        Debug.Log($"{indent}    └── Else:");
                        PrintChildren(ifStatement.ElseBlock, indent + "        ");
                    }
                    break;

                case BinaryExpressionNode binary:
                    Debug.Log($"Binary: {binary.Operator}");
                    Debug.Log($"{indent}    ├── Left:");
                    PrintNode(binary.Left, indent + "    │   ", true);
                    Debug.Log($"{indent}    └── Right:");
                    PrintNode(binary.Right, indent + "        ", true);
                    break;

                case UnaryExpressionNode unary:
                    Debug.Log($"Unary: {unary.Operator}");
                    PrintNode(unary.Operand, indent + "    ", true);
                    break;

                case LiteralNode literal:
                    Debug.Log($"Literal: {literal.Value}");
                    break;

                case IdentifierNode identifier:
                    Debug.Log($"Identifier: {identifier.Name}");
                    break;

                case FunctionCallNode functionCall:
                    Debug.Log($"Function Call: {functionCall.Name}");
                    if (functionCall.Arguments.Any())
                    {
                        Debug.Log($"{indent}    └── Arguments:");
                        PrintChildren(functionCall.Arguments, indent + "        ");
                    }
                    break;
                case IncrementDecrementNode incrementDecrement:
                    Debug.Log($"Increment/Decrement: {incrementDecrement.Variable.Value}");
                    break;
                case intAssignamentNode intAssign:
                    Debug.Log($"Int Assignment: {intAssign.Variable} = {intAssign.Value}");
                    break;
                case AssignmentNode assignment:
                    Debug.Log($"Assignment: {assignment.Target.GetType().Name} = {assignment.Value.GetType().Name}");
                    PrintNode(assignment.Target, indent + "    ", false);
                    PrintNode(assignment.Value, indent + "    ", true);
                    break;

                case ExpressionNode expression:
                    // Aquí imprimimos el tipo de expresión y la propiedad 'Solution'
                    Debug.Log($"Expression: {expression.GetType().Name}, Solution: {expression.Solution}");
                    PrintNode(expression.Solution, indent + "    ", true);  // Si deseas imprimir el valor de la solución
                    break;
                default:
                    Debug.Log($"Unknown Node Type: {node.GetType().Name}");
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
}