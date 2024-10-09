using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace GwentCompiler
{
    [Serializable]
    public class SerializableEffect
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Action { get; set; }

        public SerializableEffect()
        {
            Parameters = new Dictionary<string, string>();
        }
    }


    public class EffectManager : MonoBehaviour
    {
        private List<SerializableEffect> effects = new List<SerializableEffect>();
        
        public void GenerateEffectsFromAST(ProgramNode program)
        {
            // Cargar los efectos existentes antes de generar nuevos
            LoadEffectsFromJson();

            foreach (var definition in program.Definitions)
            {
                if (definition is EffectDefinitionNode effectDef)
                {
                    SerializableEffect serializableEffect = new SerializableEffect
                    {
                        Name = effectDef.Name,
                        Parameters = effectDef.Parameters.ToDictionary(p => p.Name, p => p.Type),
                        Action = SerializeLambdaExpression(effectDef.Body)
                    };
                    
                    // Reemplazar el efecto existente si ya existe uno con el mismo nombre
                    int existingIndex = effects.FindIndex(e => e.Name == serializableEffect.Name);
                    if (existingIndex != -1)
                    {
                        effects[existingIndex] = serializableEffect;
                    }
                    else
                    {
                        effects.Add(serializableEffect);
                    }
                }
            }
            
            SaveEffectsToJson();
        }

        public List<SerializableEffect> GetEffects()
        {
            return effects;
        }
        private string SerializeLambdaExpression(LambdaExpressionNode lambda)
        {
            return JsonConvert.SerializeObject(lambda.Body.Select(SerializeStatement).ToList());
        }
        
        private object SerializeStatement(StatementNode statement)
        {
            switch (statement)
            {
                case ExpressionNode expression:
                    return SerializeExpression(expression);
                case ForLoopNode forLoop:
                    return new
                    {
                        type = "forLoop",
                        iterator = forLoop.IteratorName,
                        collection = SerializeExpression(forLoop.Collection),
                        body = forLoop.Body.Select(SerializeStatement).ToList()
                    };
                case WhileLoopNode whileLoop:
                    return new
                    {
                        type = "whileLoop",
                        condition = SerializeExpression(whileLoop.Condition),
                        body = whileLoop.Body.Select(SerializeStatement).ToList()
                    };
                case IfStatementNode ifStatement:
                    return new
                    {
                        type = "ifStatement",
                        condition = SerializeExpression(ifStatement.Condition),
                        thenBlock = ifStatement.ThenBlock.Select(SerializeStatement).ToList(),
                        elseBlock = ifStatement.ElseBlock?.Select(SerializeStatement).ToList()
                    };
                default:
                    return new { type = "unknown" };
            }
        }
        
        private object SerializeExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case BinaryExpressionNode binary:
                    return new
                    {
                        type = "binaryExpression",
                        left = SerializeExpression(binary.Left),
                        op = binary.Operator,
                        right = SerializeExpression(binary.Right)
                    };
                case UnaryExpressionNode unary:
                    return new
                    {
                        type = "unaryExpression",
                        op = unary.Operator,
                        operand = SerializeExpression(unary.Operand)
                    };
                case LiteralNode literal:
                    return new
                    {
                        type = "literal",
                        value = literal.Value
                    };
                case IdentifierNode identifier:
                    return new
                    {
                        type = "identifier",
                        name = identifier.Name
                    };
                case FunctionCallNode functionCall:
                    return new
                    {
                        type = "functionCall",
                        name = functionCall.Name,
                        arguments = functionCall.Arguments.Select(SerializeExpression).ToList()
                    };
                default:
                    return new { type = "unknown" };
            }
        }
        
        private void SaveEffectsToJson()
        {
            string json = JsonConvert.SerializeObject(effects, Formatting.Indented);
            
            string path = System.IO.Path.Combine(Application.dataPath, "Resources", "effects.json");
            System.IO.File.WriteAllText(path, json);
            
            Debug.Log($"Effects saved to {path}");
        }
        
        public List<SerializableEffect> LoadEffectsFromJson()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("effects");
            if (jsonFile != null)
            {
                List<SerializableEffect> loadedEffects = JsonConvert.DeserializeObject<List<SerializableEffect>>(jsonFile.text);
                effects.AddRange(loadedEffects); // Mantener los efectos previos y agregar los nuevos
                return effects;
            }
            
            Debug.LogWarning("Could not load effects.json");
            return new List<SerializableEffect>();
        }
    }
}
