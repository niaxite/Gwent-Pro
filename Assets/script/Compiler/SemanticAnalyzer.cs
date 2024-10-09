using System;
using System.Collections.Generic;
using System.Linq;

namespace GwentCompiler
{
    public class SemanticAnalyzer
    {
        private readonly Dictionary<string, EffectDefinitionNode> effectDefinitions = new Dictionary<string, EffectDefinitionNode>();
        private readonly Dictionary<string, CardDefinitionNode> cardDefinitions = new Dictionary<string, CardDefinitionNode>();
        private readonly List<string> errors = new List<string>();
        private readonly SymbolTable symbolTable;

        public SemanticAnalyzer(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        private void AnalyzeIdentifier(IdentifierNode identifier)
        {
            if (symbolTable.LookupSymbol(identifier.Name) == null)
            {
                errors.Add($"Error: Undefined symbol '{identifier.Name}'");
            }
        }

        public List<string> Analyze(ProgramNode program)
        {
            Console.WriteLine("Iniciando análisis semántico...");

            foreach (var definition in program.Definitions)
            {
                if (definition is EffectDefinitionNode effectDef)
                {
                    AnalyzeEffectDefinition(effectDef);
                }
                else if (definition is CardDefinitionNode cardDef)
                {
                    AnalyzeCardDefinition(cardDef);
                }
                else
                {
                    Console.WriteLine($"ADVERTENCIA: Tipo de definición desconocido: {definition.GetType().Name}");
                }
            }

            Console.WriteLine("Análisis semántico completado.");
            return errors;

        }

        private void AnalyzeEffectDefinition(EffectDefinitionNode effectDef)
        {
            Console.WriteLine($"Analizando definición de efecto: {effectDef.Name}");

            if (effectDefinitions.ContainsKey(effectDef.Name))
            {
                errors.Add($"Error: Efecto '{effectDef.Name}' ya definido.");
            }
            else
            {
                effectDefinitions[effectDef.Name] = effectDef;
            }

            AnalyzeLambdaExpression(effectDef.Body, effectDef.Parameters);
        }

        private void AnalyzeCardDefinition(CardDefinitionNode cardDef)
        {
            Console.WriteLine($"Analizando definición de carta: {cardDef.Name}");

            if (cardDefinitions.ContainsKey(cardDef.Name))
            {
                errors.Add($"Error: Carta '{cardDef.Name}' ya definida.");
            }
            else
            {
                cardDefinitions[cardDef.Name] = cardDef;
            }

            foreach (var effect in cardDef.OnActivation)
            {
                AnalyzeEffectInvocation(effect);
            }
        }

        private void AnalyzeLambdaExpression(LambdaExpressionNode lambda, List<ParameterNode> parameters)
        {
            Console.WriteLine("Analizando expresión lambda...");

            var symbolTable = new Dictionary<string, string>();
            foreach (var param in parameters)
            {
                symbolTable[param.Name.ToLower()] = param.Type.ToLower();
            };
            foreach (var statement in lambda.Body)
            {
                AnalyzeStatement(statement, symbolTable);
            }
        }

        private void AnalyzeStatement(StatementNode statement, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando statement: {statement.GetType().Name}");

            switch (statement)
            {
                case ForLoopNode forLoop:
                    AnalyzeForLoop(forLoop, symbolTable);
                    break;
                case WhileLoopNode whileLoop:
                    AnalyzeWhileLoop(whileLoop, symbolTable);
                    break;
                case IfStatementNode ifStatement:
                    AnalyzeIfStatement(ifStatement, symbolTable);
                    break;
                case intAssignamentNode assignment:
                    AnalyzeAssignment(assignment, symbolTable);
                    break;
                case ExpressionNode expression:
                    AnalyzeExpression(expression, symbolTable);
                    break;
                default:
                    errors.Add($"ADVERTENCIA: Tipo de statement no manejado: {statement.GetType().Name}");
                    break;
            }
        }

        private void AnalyzeForLoop(ForLoopNode forLoop, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando bucle for con iterador: {forLoop.IteratorName}");
            symbolTable[forLoop.Collection.Name] = "";
            AnalyzeExpression(forLoop.Collection, symbolTable);

            var loopSymbolTable = new Dictionary<string, string>(symbolTable);
            loopSymbolTable[forLoop.IteratorName] = "unknown"; // Tipo inferido del iterador

            foreach (var statement in forLoop.Body)
            {
                AnalyzeStatement(statement, loopSymbolTable);
            }
        }

        private void AnalyzeWhileLoop(WhileLoopNode whileLoop, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine("Analizando bucle while...");

            AnalyzeExpression(whileLoop.Condition, symbolTable);

            foreach (var statement in whileLoop.Body)
            {
                AnalyzeStatement(statement, symbolTable);
            }
        }

        private void AnalyzeIfStatement(IfStatementNode ifStatement, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine("Analizando statement if...");

            AnalyzeExpression(ifStatement.Condition, symbolTable);

            foreach (var statement in ifStatement.ThenBlock)
            {
                AnalyzeStatement(statement, symbolTable);
            }

            if (ifStatement.ElseBlock != null)
            {
                foreach (var statement in ifStatement.ElseBlock)
                {
                    AnalyzeStatement(statement, symbolTable);
                }
            }
        }

        private void AnalyzeAssignment(intAssignamentNode assignment, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine("Analizando asignación...");
            symbolTable[assignment.Variable] = "";
            try
            {
                if (assignment.Type == "int")
                {
                    int sd = int.Parse(assignment.Value);
                    symbolTable[assignment.Variable] = sd.ToString();
                }
            }
            catch
            {
                errors.Add($"Advertencia: Variable '{assignment.Variable}' no definida.");
            }
            // Aquí podrías agregar lógica para verificar la compatibilidad de tipos
        }

        private void AnalyzeExpression(ExpressionNode expression, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando expresión: {expression.GetType().Name}");

            switch (expression)
            {
                case BinaryExpressionNode binary:
                    AnalyzeExpression(binary.Left, symbolTable);
                    AnalyzeExpression(binary.Right, symbolTable);
                    break;
                case UnaryExpressionNode unary:
                    AnalyzeExpression(unary.Operand, symbolTable);
                    break;
                case LiteralNode literal:
                    // No se necesita análisis adicional para literales
                    break;
                case IdentifierNode identifier:
                    if (!symbolTable.ContainsKey(identifier.Name))
                    {
                        errors.Add($"Advertencia: Variable '{identifier.Name}' no definida.");
                    }
                    break;
                case FunctionCallNode functionCall:
                    AnalyzeAcceso(functionCall.Name, symbolTable);
                    break;
                case IncrementDecrementNode incrementDecrement:
                    AnalyzeIncrementDecrementNode(incrementDecrement, symbolTable);
                    break;
                case intAssignamentNode assignment:
                    AnalyzeIntAssignment(assignment, symbolTable);
                    break;
                case AssignmentNode assignament:
                    AnalyzeAssignment(assignament, symbolTable);
                    break;

                default:
                    Console.WriteLine($"ADVERTENCIA: Tipo de expresión no manejado: {expression.GetType().Name}");
                    break;
            }
        }
        private void AnalyzeIncrementDecrementNode(IncrementDecrementNode node, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando incremento/decremento: {node.Variable.Value}");

            // Verifica si la variable existe en la tabla de símbolos
            if (!symbolTable.ContainsKey(node.Variable.Value))
            {
                errors.Add($"Advertencia: Variable '{node.Variable.Value}' no definida.");
            }
        }
        private void AnalyzeAssignment(AssignmentNode assignment, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando asignación a la variable: {assignment.Target.Solution}");

            // Verificar que la variable en el lado izquierdo de la asignación esté definida en el contexto
            if (assignment.Target is ExpressionNode identifier)
            {
                symbolTable[identifier.Solution] = " ";
            }
            
                else
                {
                    errors.Add($"Error: La variable '{assignment.Target.Solution}' no está definida.");
                }

            // Analizar la expresión en el lado derecho de la asignación
            AnalyzeExpression(assignment.Value, symbolTable);
        }

        private void AnalyzeIntAssignment(intAssignamentNode assignment, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando asignación de entero: {assignment.Variable}");

            // Verificar si la variable ya está definida en el contexto
            if (!symbolTable.ContainsKey(assignment.Variable))
            {
                symbolTable[assignment.Variable] = "int"; // Registrar la variable en la tabla de símbolos
                Console.WriteLine($"Variable '{assignment.Variable}' declarada como 'int'.");
            }

            // Verificar si el valor asignado es numérico
            if (!int.TryParse(assignment.Value, out _))
            {
                errors.Add($"Error: El valor asignado a '{assignment.Variable}' no es un número válido.");
            }
        }

        private void AnalyzePropertyAccess(PropertyAccessNode propertyAccess, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando acceso a propiedad: {propertyAccess.PropertyName}");

            // Aquí puedes dividir el acceso por los puntos, por ejemplo, "object.property.subProperty"
            var parts = propertyAccess.PropertyName.Split('.');

            foreach (var part in parts)
            {
                if (!PropertyExist(part))
                {
                    errors.Add($"Advertencia: Propiedad '{part}' no definida.");
                }
            }
        }

        private bool PropertyExist(string propertyName)
        {
            List<string> propertys = new List<string>();
            propertys.Add("name");
            propertys.Add("type");
            propertys.Add("power");
            propertys.Add("range");
            propertys.Add("faction");
            if (propertys.Contains(propertyName)) return true;
            else return false;

        }

        private void AnalyzeAcceso(string Acesso, Dictionary<string, string> symbolTable)
        {
            string[] acceso = Acesso.Split('.');
            string Resto = "";
            for (int i = 1; i < acceso.Length; i++)
            {
                Resto += acceso[i];
                if (i < acceso.Length - 1) Resto += ".";
            }
            if (ContextExist(acceso[0], symbolTable))
            {

            }
            else errors.Add($"Error: Variable '{acceso[0]}' no definida.");
            //
        }

        public bool ContextExist(string context, Dictionary<string, string> symbolTable)
        {
            if (context == "context" || context == "unit" || symbolTable.ContainsKey(context)) return true;
            else return false;
        }
        private void AnalyzeFunctionCall(FunctionCallNode functionCall, Dictionary<string, string> symbolTable)
        {
            Console.WriteLine($"Analizando llamada a función: {functionCall.Name}");

            // Verificar si la función existe
            if (!symbolTable.ContainsKey(functionCall.Name))
            {
                errors.Add($"Error: Función '{functionCall.Name}' no definida.");
            }
            // Verificar los argumentos
            foreach (var argument in functionCall.Arguments)
            {
                AnalyzeExpression(argument, symbolTable); // Analizar cada argumento individualmente
            }
        }
        private void AnalyzeEffectInvocation(EffectInvocationNode effectInvocation)
        {
            Console.WriteLine($"Analizando invocación de efecto: {effectInvocation.EffectName}");

            if (!effectDefinitions.ContainsKey(effectInvocation.EffectName))
            {
                errors.Add($"Error: Efecto '{effectInvocation.EffectName}' no definido.");
            }
            else
            {
                var effectDef = effectDefinitions[effectInvocation.EffectName];
                var expectedParams = effectDef.Parameters.Select(p => p.Name).ToHashSet();
                var providedArgs = effectInvocation.Arguments.Keys.ToHashSet();

                var missingArgs = expectedParams.Except(providedArgs);
                var extraArgs = providedArgs.Except(expectedParams);

                foreach (var arg in missingArgs)
                {
                    errors.Add($"Error: Argumento faltante '{arg}' en la invocación de '{effectInvocation.EffectName}'.");
                }

                foreach (var arg in extraArgs)
                {
                    errors.Add($"Error: Argumento extra '{arg}' en la invocación de '{effectInvocation.EffectName}'.");
                }

                foreach (var arg in effectInvocation.Arguments)
                {
                    AnalyzeExpression(arg.Value, new Dictionary<string, string>());
                }
            }

            if (effectInvocation.Selector != null)
            {
                AnalyzeSelector(effectInvocation.Selector);
            }

            if (effectInvocation.PostAction != null)
            {
                AnalyzeEffectInvocation(effectInvocation.PostAction);
            }
        }

        private void AnalyzeSelector(SelectorNode selector)
        {
            Console.WriteLine($"Analizando selector de origen: {selector.Source}");

            if (selector.Predicate != null)
            {
                AnalyzeLambdaExpression(selector.Predicate, new List<ParameterNode>());
            }
        }

    }
}